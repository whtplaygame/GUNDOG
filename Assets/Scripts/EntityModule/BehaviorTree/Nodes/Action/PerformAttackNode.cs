using EntityModule.Component;

namespace EntityModule.BehaviorTree.Nodes.Action
{
    /// <summary>
    /// 执行攻击节点（符合迪米特法则）
    /// </summary>
    public class PerformAttackNode : IBehaviorNode
    {
        public NodeStatus Execute(global::EntityModule.Entity owner)
        {
            if (owner == null) return NodeStatus.Failure;

            var dataComponent = owner.GetComponent<DataComponent>();
            var combatComponent = owner.GetComponent<CombatComponent>();
            
            if (dataComponent == null || combatComponent == null)
            {
                return NodeStatus.Failure;
            }

            global::EntityModule.Entity targetEntity = dataComponent.GetTargetEntity();
            if (targetEntity == null)
            {
                return NodeStatus.Failure;
            }

            // 执行攻击
            bool attackSuccess = combatComponent.Attack(targetEntity);
            return attackSuccess ? NodeStatus.Success : NodeStatus.Failure;
        }
    }
}

