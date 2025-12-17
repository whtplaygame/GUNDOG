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
            SetState(AnimationState.Idle);
        }

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);
            
            if (Owner == null || animator == null) return;

            // 自动检测状态变化并切换动画
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

            // 检查移动状态
            bool isMoving = movementComponent != null && movementComponent.IsMoving;
            
            if (isMoving != wasMoving)
            {
                wasMoving = isMoving;
                if (isMoving && currentState != AnimationState.Attack)
                {
                    SetState(AnimationState.Move);
                }
                else if (!isMoving && currentState == AnimationState.Move)
                {
                    SetState(AnimationState.Idle);
                }
            }
        }

        /// <summary>
        /// 设置动画状态
        /// </summary>
        public void SetState(AnimationState newState)
        {
            if (animator == null || currentState == newState) return;

            currentState = newState;

            // 设置Animator参数
            if (animator.isInitialized)
            {
                // 设置状态枚举值（转换为int）
                animator.SetInteger(AnimationParameters.State, (int)newState);

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
                        animator.SetBool(AnimationParameters.IsMoving, false);
                        animator.SetTrigger(AnimationParameters.AttackTrigger);
                        break;

                    case AnimationState.Hit:
                        animator.SetTrigger(AnimationParameters.HitTrigger);
                        break;

                    case AnimationState.Death:
                        animator.SetBool(AnimationParameters.IsMoving, false);
                        animator.SetTrigger(AnimationParameters.DeathTrigger);
                        break;
                }
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

