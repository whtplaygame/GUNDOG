using UnityEngine;

namespace EntityModule.Component
{
    /// <summary>
    /// 战斗表现组件（View层）
    /// 纯粹的数据观察者，订阅逻辑层事件，更新Animator/特效
    /// </summary>
    public class CombatViewComponent : Component
    {
        private CombatComponent logicComp;
        private AnimationComponent animComponent;

        public override void Initialize()
        {
            base.Initialize();
            
            if (Owner == null) return;

            // 获取逻辑组件和动画组件
            logicComp = Owner.GetComponent<CombatComponent>();
            animComponent = Owner.GetComponent<AnimationComponent>();

            if (logicComp == null)
            {
                Debug.LogWarning($"[CombatViewComponent] Entity {Owner.Id} 没有CombatComponent");
                return;
            }

            // 订阅逻辑层的事件（单向依赖：View 依赖 Logic）
            logicComp.OnPlayAction += OnPlayAction;
            logicComp.OnHitFeedback += OnHitFeedback;
            logicComp.OnAttackCanceled += OnAttackCanceled;
            logicComp.OnHitStunStart += OnHitStunStart;
            logicComp.OnHitStunEnd += OnHitStunEnd;
        }

        public override void UpdateView()
        {
            base.UpdateView();
            
            if (logicComp == null || animComponent == null) return;

            // 根据逻辑层的状态，同步动画状态
            // 注意：这里只同步状态，不直接操作Animator的播放
            SyncAnimationState();
        }

        /// <summary>
        /// 同步动画状态（根据逻辑层状态）
        /// </summary>
        private void SyncAnimationState()
        {
            // 优先级：硬直 > 攻击 > 移动 > 空闲

            // 1. 检查硬直状态
            if (logicComp.IsInHitStun)
            {
                if (animComponent.CurrentState != AnimationState.Hit)
                {
                    animComponent.SetState(AnimationState.Hit);
                }
                return;
            }

            // 2. 检查攻击状态
            // 注意：攻击状态包括Prepare、WindUp、Impact、Recovery，但不包括Reset
            // Reset状态会立即变回Idle，但我们需要确保动画完成
            if (logicComp.AttackState != AttackState.Idle && logicComp.AttackState != AttackState.Reset)
            {
                // 确保攻击动画正在播放
                if (animComponent.CurrentState != AnimationState.Attack)
                {
                    animComponent.SetState(AnimationState.Attack);
                }
                return;
            }

            // 3. 攻击刚结束（Reset或刚变回Idle），不立即切换动画
            // 让AnimationComponent根据移动状态自然切换
            // 这样可以避免动画被强制打断
        }

        // === 事件处理 ===

        /// <summary>
        /// 播放动作事件处理
        /// </summary>
        private void OnPlayAction(string actionName)
        {
            if (animComponent == null) return;
            
            // 播放攻击动画（使用现有的AnimationState.Attack）
            animComponent.PlayAttack();
        }

        /// <summary>
        /// 受击反馈事件处理
        /// </summary>
        private void OnHitFeedback(Entity target)
        {
            // 可以在这里播放受击特效、飘血等
            // 例如：VFXManager.Play("BloodSplatter", target.WorldPosition);
        }

        /// <summary>
        /// 攻击被打断事件处理
        /// </summary>
        private void OnAttackCanceled()
        {
            // 攻击被打断，动画会由AnimationComponent根据状态自动切换
            // 这里可以播放打断特效等
        }

        /// <summary>
        /// 硬直开始事件处理
        /// </summary>
        private void OnHitStunStart()
        {
            if (animComponent == null) return;
            
            // 播放受击动画
            animComponent.PlayHit();
            animComponent.SetState(AnimationState.Hit);
        }

        /// <summary>
        /// 硬直结束事件处理
        /// </summary>
        private void OnHitStunEnd()
        {
            // 硬直结束，动画会由AnimationComponent根据状态自动切换
            // 这里可以播放恢复特效等
        }

        /// <summary>
        /// 清理事件订阅
        /// </summary>
        private void OnDestroy()
        {
            if (logicComp != null)
            {
                logicComp.OnPlayAction -= OnPlayAction;
                logicComp.OnHitFeedback -= OnHitFeedback;
                logicComp.OnAttackCanceled -= OnAttackCanceled;
                logicComp.OnHitStunStart -= OnHitStunStart;
                logicComp.OnHitStunEnd -= OnHitStunEnd;
            }
        }
    }
}

