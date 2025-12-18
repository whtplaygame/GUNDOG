using EntityModule.Component;

namespace EntityModule.BehaviorTree.Nodes.Condition
{
    /// <summary>
    /// 检查Entity是否存活节点（符合迪米特法则）
    /// </summary>
    public class CheckIsAliveNode : IBehaviorNode
    {
        public NodeStatus Execute(Entity owner)
        {
            if (owner == null) return NodeStatus.Failure;

            var combatComponent = owner.GetComponent<CombatComponent>();
            if (combatComponent == null)
            {
                return NodeStatus.Failure;
            }

            return combatComponent.IsAlive ? NodeStatus.Success : NodeStatus.Failure;
        }
    }
}

