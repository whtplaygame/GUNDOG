using EntityModule.Component;

namespace EntityModule.BehaviorTree.Nodes.Action
{
    /// <summary>
    /// 执行攻击节点（符合迪米特法则）
    /// </summary>
    public class PerformAttackNode : IBehaviorNode
    {
        public NodeStatus Execute(Entity owner)
        {
            if (owner == null) return NodeStatus.Failure;

            var dataComponent = owner.GetComponent<DataComponent>();
            var combatComponent = owner.GetComponent<CombatComponent>();
            var animComponent = owner.GetComponent<AnimationComponent>();
            
            if (dataComponent == null || combatComponent == null)
            {
                return NodeStatus.Failure;
            }

            Entity targetEntity = dataComponent.GetTargetEntity();
            if (targetEntity == null)
            {
                return NodeStatus.Failure;
            }

            // 播放攻击动画
            if (animComponent != null)
            {
                animComponent.PlayAttack();
            }

            // 执行攻击
            bool attackSuccess = combatComponent.Attack(targetEntity);
            return attackSuccess ? NodeStatus.Success : NodeStatus.Failure;
        }
    }
}

