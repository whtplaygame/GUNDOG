using UnityEngine;

namespace EntityModule.Component
{
    /// <summary>
    /// 动画状态枚举
    /// </summary>
    public enum AnimationState
    {
        Idle,       // 空闲
        Move,       // 移动
        Attack,     // 攻击
        Hit,        // 受击
        Death       // 死亡
    }

    /// <summary>
    /// 动画参数名常量
    /// </summary>
    public static class AnimationParameters
    {
        public const string State = "State";           // 动画状态参数（int）
        public const string Speed = "Speed";          // 移动速度参数（float）
        public const string IsMoving = "IsMoving";    // 是否移动（bool）
        public const string AttackTrigger = "Attack"; // 攻击触发器（trigger）
        public const string HitTrigger = "Hit";       // 受击触发器（trigger）
        public const string DeathTrigger = "Death";   // 死亡触发器（trigger）
    }

    /// <summary>
    /// 动画组件（单一职责原则：只负责动画表现）
    /// 控制Unity Animator的状态切换
    /// </summary>
    public class AnimationComponent : Component
    {
        [SerializeField] private Animator animator;
        [SerializeField] private AnimationState currentState = AnimationState.Idle;
        
        private MovementComponent movementComponent;
        private CombatComponent combatComponent;
        private bool wasMoving = false;
        private bool wasAlive = true;

        public Animator Animator
        {
            get => animator;
            set => animator = value;
        }

        public AnimationState CurrentState => currentState;

        public override void Initialize()
        {
            base.Initialize();
            
            if (Owner == null) return;

            // 如果没有手动设置Animator，尝试从GameObject获取
            if (animator == null)
            {
                animator = Owner.gameObject.GetComponent<Animator>();
                
                // 如果还是没有，尝试从子对象获取
                if (animator == null)
                {
                    animator = Owner.GetComponentInChildren<Animator>();
                }
            }

            // 获取其他组件引用
            movementComponent = Owner.GetComponent<MovementComponent>();
            combatComponent = Owner.GetComponent<CombatComponent>();

            // 初始化动画状态
            lastSetState = AnimationState.Idle;
            SetState(AnimationState.Idle);
        }

        public override void UpdateView()
        {
            base.UpdateView();
            
            if (Owner == null || animator == null) return;

            // 自动检测状态变化并切换动画（View层更新）
            UpdateAnimationState();
        }

        /// <summary>
        /// 更新动画状态（根据其他组件的状态自动切换）
        /// </summary>
        private void UpdateAnimationState()
        {
            // 检查死亡状态
            if (combatComponent != null && !combatComponent.IsAlive)
            {
                if (wasAlive)
                {
                    SetState(AnimationState.Death);
                    wasAlive = false;
                }
                return; // 死亡后不再更新其他状态
            }
            else
            {
                wasAlive = true;
            }

            // 检查攻击状态（优先级最高，如果正在攻击，不处理移动状态）
            if (combatComponent != null && combatComponent.AttackState != AttackState.Idle)
            {
                // 确保动画状态与逻辑状态同步
                // 解决 MovementComponent.Stop(false) 导致的动画状态滞后问题
                if (currentState != AnimationState.Attack)
                {
                    SetState(AnimationState.Attack);
                }
                
                // 攻击/受击期间禁用根运动，避免Animator自带位移导致滑步
                if (animator != null)
                {
                    animator.applyRootMotion = false;
                }
                wasMoving = false;
                return; // 攻击状态由CombatViewComponent或自动同步管理
            }
            
            // ✅ 关键修复：当逻辑攻击状态已结束，但动画状态还在 Attack 时，重置状态
            // 场景：Attack 动画通过 Has Exit Time 自动回到 Idle，但 currentState/lastSetState 还是 Attack
            // 这样下次攻击时，SetState(Attack) 才不会因为 lastSetState == Attack 而提前返回
            if (currentState == AnimationState.Attack)
            {
                // 逻辑状态已回到 Idle，重置动画状态以便下次攻击能触发
                currentState = AnimationState.Idle;
                lastSetState = AnimationState.Idle;
                // 注意：不调用 SetState，避免触发 Animator 参数设置
            }
            
            // 检查硬直状态（硬直状态优先于移动状态）
            if (combatComponent != null && combatComponent.IsInHitStun)
            {
                // 硬直期间保持受击动画状态
                if (currentState != AnimationState.Hit)
                {
                    SetState(AnimationState.Hit);
                }
                
                if (animator != null)
                {
                    animator.applyRootMotion = false;
                }
                wasMoving = false; // 硬直期间不移动
                return; // 硬直期间不更新其他状态
            }
            
            // ✅ 同样，硬直结束后也要重置状态
            if (currentState == AnimationState.Hit)
            {
                currentState = AnimationState.Idle;
                lastSetState = AnimationState.Idle;
            }

            // 检查移动状态（只有在非攻击/受击状态下才处理）
            bool isMoving = movementComponent != null && movementComponent.IsMoving;
            
            // 强制状态同步：如果当前是 Attack/Hit/Death 但逻辑已经结束，需要切回 Idle/Move
            // 比如攻击结束瞬间，isMoving可能为false，wasMoving为false，但currentState仍为Attack
            if (currentState == AnimationState.Attack || currentState == AnimationState.Hit)
            {
                // 强制刷新
                wasMoving = !isMoving; 
            }
            
            if (isMoving != wasMoving)
            {
                wasMoving = isMoving;
                if (isMoving)
                {
                    SetState(AnimationState.Move);
                    
                    // 移动时恢复根运动由动画驱动（如果需要），但防止上个状态残留
                    if (animator != null)
                    {
                        animator.applyRootMotion = false; // 此处仍禁用，因位移由逻辑控制
                    }
                }
                else if (currentState == AnimationState.Move)
                {
                    SetState(AnimationState.Idle);
                    if (animator != null)
                    {
                        animator.applyRootMotion = false;
                    }
                }
            }
        }

        private AnimationState lastSetState = AnimationState.Idle; // 记录上次设置的状态

        /// <summary>
        /// 设置动画状态
        /// </summary>
        public void SetState(AnimationState newState)
        {
            if (animator == null || !animator.isInitialized) return;
            
            // ✅ 修复：只检查 lastSetState，避免 Animator 状态不同步导致的问题
            // 场景：Attack 动画通过 Has Exit Time 回到 Idle，但 currentState 还是 Attack
            // 下次调用 SetState(Attack) 时应该能触发，而不是被提前返回
            if (lastSetState == newState) return;

            // 记录旧状态（用于判断是否需要触发Trigger）
            AnimationState oldState = lastSetState;
            lastSetState = newState;
            currentState = newState;

            // 设置Animator参数
            // 设置状态枚举值（转换为int）
            // animator.SetInteger(AnimationParameters.State, (int)newState);

            if ("Chaser" == Owner.name)
                Debug.LogError($"{Owner.gameObject.name}=>{newState}");
            // 根据状态设置其他参数
            switch (newState)
            {
                case AnimationState.Idle:
                    animator.SetBool(AnimationParameters.IsMoving, false);
                    animator.SetFloat(AnimationParameters.Speed, 0f);
                    break;

                case AnimationState.Move:
                    animator.SetBool(AnimationParameters.IsMoving, true);
                    if (movementComponent != null)
                    {
                        animator.SetFloat(AnimationParameters.Speed, movementComponent.MoveSpeed);
                    }
                    break;

                case AnimationState.Attack:
                    // ✅ 关键修复：不设置 IsMoving，避免触发 Run → Idle 转换
                    // 让 Unity Animator 通过 Run → Attack 直接转换（只需 Attack trigger）
                    // 只在状态切换时触发，避免重复触发
                    if (oldState != AnimationState.Attack)
                    {
                        animator.SetTrigger(AnimationParameters.AttackTrigger);
                    }
                    break;

                case AnimationState.Hit:
                    // ✅ 关键修复：不设置 IsMoving，让 Run → Hit 直接转换
                    // 只在状态切换时触发
                    if (oldState != AnimationState.Hit)
                    {
                        animator.SetTrigger(AnimationParameters.HitTrigger);
                    }
                    break;

                case AnimationState.Death:
                    // ✅ 关键修复：不设置 IsMoving，让 Run → Death 直接转换
                    // 只在状态切换时触发
                    if (oldState != AnimationState.Death)
                    {
                        animator.SetTrigger(AnimationParameters.DeathTrigger);
                    }
                    break;
            }
        }

        /// <summary>
        /// 播放攻击动画
        /// </summary>
        public void PlayAttack()
        {
            SetState(AnimationState.Attack);
        }

        /// <summary>
        /// 播放受击动画
        /// </summary>
        public void PlayHit()
        {
            SetState(AnimationState.Hit);
        }

        /// <summary>
        /// 播放死亡动画
        /// </summary>
        public void PlayDeath()
        {
            SetState(AnimationState.Death);
        }

        /// <summary>
        /// 播放指定动作（用于技能动画等）
        /// </summary>
        public void PlayAction(string actionName)
        {
            if (animator == null || !animator.isInitialized) return;
            
            // 如果Animator中有对应的Trigger，则触发它
            if (HasParameter(actionName))
            {
                animator.SetTrigger(actionName);
            }
            else
            {
                Debug.LogWarning($"[AnimationComponent] 动画参数 '{actionName}' 不存在于Animator中");
            }
        }

        /// <summary>
        /// 检查Animator是否有指定参数
        /// </summary>
        private bool HasParameter(string paramName)
        {
            if (animator == null || !animator.isInitialized) return false;

            foreach (AnimatorControllerParameter param in animator.parameters)
            {
                if (param.name == paramName)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// 设置移动速度参数（用于Blend Tree等）
        /// </summary>
        public void SetSpeed(float speed)
        {
            if (animator != null && animator.isInitialized)
            {
                animator.SetFloat(AnimationParameters.Speed, speed);
            }
        }
    }
}

