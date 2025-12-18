using EntityModule.Data;
using UnityEngine;

namespace EntityModule.Component
{
    /// <summary>
    /// 攻击状态枚举（逻辑层状态机）
    /// </summary>
    public enum AttackState
    {
        Idle,       // 空闲
        Prepare,    // 准备阶段（可选）
        WindUp,     // 前摇阶段
        Impact,     // 伤害判定阶段
        Recovery,   // 后摇阶段
        Reset       // 重置阶段（回到Idle）
    }

    /// <summary>
    /// 受击状态枚举（逻辑层状态机）
    /// </summary>
    public enum HitState
    {
        None,       // 无受击
        Hit,        // 受击瞬间
        Stun,       // 硬直阶段
        Recover     // 恢复阶段（回到None）
    }

    /// <summary>
    /// 战斗组件（逻辑状态机，Controller层）
    /// 负责把Data里的死数据跑活，维护瞬时状态
    /// </summary>
    public class CombatComponent : Component
    {
        // === 数据层 (Model) ===
        private CombatData data;

        // === 攻击状态机 (Transient State) ===
        public AttackState AttackState { get; private set; } = AttackState.Idle;
        private float attackTimer = 0f;
        private bool damageDealt = false; // 标记是否已扣血
        private Entity attackTarget = null; // 当前攻击目标

        // === 受击状态机 (Transient State) ===
        public HitState HitState { get; private set; } = HitState.None;
        private float hitTimer = 0f;

        // === 冷却时间 ===
        private float attackCooldownTimer = 0f;

        // === 事件：用于通知表现层（解耦的关键） ===
        public System.Action<string> OnPlayAction;           // 播放动作事件（参数：动作名）
        public System.Action<Entity> OnHitFeedback;         // 受击反馈事件（参数：目标实体）
        public System.Action OnAttackCanceled;               // 攻击被打断事件
        public System.Action OnHitStunStart;                  // 硬直开始事件
        public System.Action OnHitStunEnd;                    // 硬直结束事件

        // === 公共属性（兼容旧接口） ===
        public float MaxHealth => data?.MaxHP ?? 0f;
        public float CurrentHealth => data?.CurrentHP ?? 0f;
        public float AttackPower => data?.AttackPower ?? 0f;
        public float AttackRange => data?.AttackRange ?? 0f;
        public float AttackCooldown => data?.AttackCooldown ?? 0f;
        public float HitStunDuration => data?.HitStunDuration ?? 0f;

        public float AttackCooldownRemaining => attackCooldownTimer;
        public bool HasAttackCooldown => attackCooldownTimer > 0f;
        public bool CanAttack => attackCooldownTimer <= 0f && AttackState == AttackState.Idle && HitState != HitState.Stun;
        public bool IsInHitStun => HitState == HitState.Stun;
        public bool CanMove => HitState != HitState.Stun;
        public bool IsAlive => data != null && data.IsAlive;

        /// <summary>
        /// 初始化战斗数据
        /// </summary>
        public void InitializeData(CombatData combatData)
        {
            data = combatData;
        }

        /// <summary>
        /// 从现有属性初始化数据（兼容旧代码）
        /// </summary>
        public void InitializeFromProperties(float maxHealth, float attackPower, float attackRange, float attackCooldown, float hitStunDuration)
        {
            data = new CombatData
            {
                MaxHP = maxHealth,
                CurrentHP = maxHealth,
                AttackPower = attackPower,
                AttackRange = attackRange,
                AttackCooldown = attackCooldown,
                HitStunDuration = hitStunDuration
            };
        }

        public override void Initialize()
        {
            base.Initialize();
            
            // 如果没有数据，使用默认值初始化
            if (data == null)
            {
                InitializeFromProperties(100f, 10f, 1f, 1f, 0.5f);
            }
        }

        public override void TickLogic(float deltaTime)
        {
            base.TickLogic(deltaTime);
            
            if (!Enabled || data == null) return;

            // 更新攻击CD
            if (attackCooldownTimer > 0f)
            {
                attackCooldownTimer -= deltaTime;
                if (attackCooldownTimer < 0f)
                {
                    attackCooldownTimer = 0f;
                }
            }

            // 更新攻击状态机
            // 注意：逻辑时间轴完全独立于动画，这是纯数据驱动的
            // 时间轴配置在CombatData中，与动画长度无关
            if (AttackState != AttackState.Idle)
            {
                TickAttackStateMachine(deltaTime);
            }

            // 更新受击状态机
            if (HitState != HitState.None)
            {
                TickHitStateMachine(deltaTime);
            }

            // 检查死亡
            if (!IsAlive && Owner != null)
            {
                OnHitStunEnd?.Invoke(); // 通知硬直结束（如果还在硬直）
                var entityManager = EntityManager.Instance;
                if (entityManager != null)
                {
                    entityManager.DestroyEntity(Owner);
                }
            }
        }

        public override void UpdateView()
        {
            base.UpdateView();
            // View层由CombatViewComponent负责，这里不需要操作
        }

        // === 攻击状态机逻辑 ===

        /// <summary>
        /// 尝试开始攻击
        /// </summary>
        public bool TryStartAttack(Entity target)
        {
            if (!IsAlive || target == null) return false;
            if (!CanAttack) return false; // CD未冷却或正在攻击或硬直中

            var targetCombat = target.GetComponent<CombatComponent>();
            if (targetCombat == null || !targetCombat.IsAlive) return false;

            // 检查攻击范围
            if (Owner != null)
            {
                float distance = Vector2Int.Distance(Owner.GridPosition, target.GridPosition);
                if (distance > data.AttackRange) return false;
            }

            // 开始攻击状态机
            AttackState = AttackState.Prepare;
            attackTimer = 0f;
            damageDealt = false;
            attackTarget = target;

            // 通知表现层：该播动画了（逻辑驱动表现）
            OnPlayAction?.Invoke("Attack");
            
            Debug.Log($"[CombatComponent] {Owner?.name} 开始攻击，状态: {AttackState}, PrepareTime={data.PrepareTime}, WindUpTime={data.WindUpTime}");
            return true;
        }

        /// <summary>
        /// 更新攻击状态机（每帧只推进一次，不循环）
        /// </summary>
        public void TickAttackStateMachine(float deltaTime)
        {
            if (data == null)
            {
                Debug.LogError($"[CombatComponent] {Owner?.name} data为null！");
                return;
            }
            
            // 防止deltaTime过大导致状态机跳跃（比如卡顿后恢复）
            float clampedDeltaTime = Mathf.Min(deltaTime, 0.2f); // 最大0.2秒
            attackTimer += clampedDeltaTime;

            switch (AttackState)
            {
                case AttackState.Prepare:
                    // 准备阶段 -> 前摇
                    if (attackTimer >= data.PrepareTime)
                    {
                        AttackState = AttackState.WindUp;
                        attackTimer = 0f;
                        Debug.Log($"[CombatComponent] {Owner?.name} 状态推进: Prepare -> WindUp (PrepareTime={data.PrepareTime}, 实际用时={attackTimer + clampedDeltaTime})");
                    }
                    break;

                case AttackState.WindUp:
                    // 前摇阶段 -> 伤害判定
                    if (attackTimer >= data.WindUpTime)
                    {
                        // 执行伤害判定
                        if (!damageDealt && attackTarget != null)
                        {
                            Debug.Log($"[CombatComponent] {Owner?.name} 执行伤害判定 (WindUpTime={data.WindUpTime}, timer={attackTimer})");
                            ApplyDamageLogic(attackTarget);
                            damageDealt = true;
                            
                            // 通知表现层：播放受击特效/飘血
                            OnHitFeedback?.Invoke(attackTarget);
                        }

                        AttackState = AttackState.Impact;
                        attackTimer = 0f;
                        Debug.Log($"[CombatComponent] {Owner?.name} 状态推进: WindUp -> Impact");
                    }
                    break;

                case AttackState.Impact:
                    // 伤害判定阶段 -> 后摇
                    if (attackTimer >= data.ImpactTime)
                    {
                        AttackState = AttackState.Recovery;
                        attackTimer = 0f;
                        Debug.Log($"[CombatComponent] {Owner?.name} 状态推进: Impact -> Recovery");
                    }
                    break;

                case AttackState.Recovery:
                    // 后摇阶段 -> 重置
                    if (attackTimer >= data.RecoveryTime)
                    {
                        AttackState = AttackState.Reset;
                        attackTimer = 0f;
                        Debug.Log($"[CombatComponent] {Owner?.name} 状态推进: Recovery -> Reset");
                    }
                    break;

                case AttackState.Reset:
                    // 重置阶段：立即回到空闲
                    AttackState = AttackState.Idle;
                    attackTimer = 0f;
                    damageDealt = false;
                    attackTarget = null;
                    
                    // 重置CD
                    attackCooldownTimer = data.AttackCooldown;
                    Debug.Log($"[CombatComponent] {Owner?.name} 状态推进: Reset -> Idle (攻击完成)");
                    break;
            }
        }

        /// <summary>
        /// 取消攻击（比如被眩晕、被移动打断、被攻击）
        /// </summary>
        public void CancelAttack()
        {
            if (AttackState != AttackState.Idle && AttackState != AttackState.Reset)
            {
                AttackState = AttackState.Idle;
                attackTimer = 0f;
                damageDealt = false;
                attackTarget = null;
                
                // 通知表现层：攻击被打断
                OnAttackCanceled?.Invoke();
            }
        }

        /// <summary>
        /// 应用伤害逻辑（纯逻辑计算）
        /// </summary>
        private void ApplyDamageLogic(Entity target)
        {
            if (target == null || data == null) return;

            var targetCombat = target.GetComponent<CombatComponent>();
            if (targetCombat == null || !targetCombat.IsAlive) return;

            // 检查攻击范围（二次校验，防止目标在伤害判定时跑远了）
            if (Owner != null)
            {
                float distance = Vector2Int.Distance(Owner.GridPosition, target.GridPosition);
                if (distance > data.AttackRange + 0.5f) // 允许0.5米缓冲
                {
                    return; // 打空了 (Miss)
                }
            }

            // 真正的数值修改
            targetCombat.TakeDamage(data.AttackPower);
        }

        // === 受击状态机逻辑 ===

        /// <summary>
        /// 受到伤害（由其他CombatComponent调用）
        /// </summary>
        public void TakeDamage(float damage)
        {
            if (!IsAlive || data == null) return;

            // 扣血
            data.CurrentHP = Mathf.Max(0f, data.CurrentHP - damage);

            // 如果还在攻击状态，被打断
            if (AttackState != AttackState.Idle && AttackState != AttackState.Reset)
            {
                CancelAttack();
            }

            // 触发受击状态机
            if (IsAlive && data.HitStunDuration > 0f)
            {
                HitState = HitState.Hit;
                hitTimer = 0f;

                // 停止移动（由MovementComponent检查硬直状态）
                if (Owner != null)
                {
                    var movementComponent = Owner.GetComponent<MovementComponent>();
                    if (movementComponent != null)
                    {
                        movementComponent.Stop();
                    }
                }

                // 通知表现层：受击开始
                OnHitStunStart?.Invoke();
            }
        }

        /// <summary>
        /// 更新受击状态机
        /// </summary>
        private void TickHitStateMachine(float deltaTime)
        {
            hitTimer += deltaTime;

            switch (HitState)
            {
                case HitState.Hit:
                    // 受击瞬间 -> 硬直
                    HitState = HitState.Stun;
                    hitTimer = 0f;
                    break;

                case HitState.Stun:
                    // 硬直阶段 -> 恢复
                    if (hitTimer >= data.HitStunDuration)
                    {
                        HitState = HitState.Recover;
                        hitTimer = 0f;
                    }
                    break;

                case HitState.Recover:
                    // 恢复阶段 -> 无受击
                    HitState = HitState.None;
                    hitTimer = 0f;
                    
                    // 通知表现层：硬直结束
                    OnHitStunEnd?.Invoke();
                    break;
            }
        }

        /// <summary>
        /// 治疗
        /// </summary>
        public void Heal(float amount)
        {
            if (!IsAlive || data == null) return;
            data.CurrentHP = Mathf.Min(data.MaxHP, data.CurrentHP + amount);
        }
    }
}
