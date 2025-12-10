using EntityModule.Component;

namespace EntityModule.BehaviorTree.Nodes.Condition
{
    /// <summary>
    /// 检查是否有合法目标节点（符合迪米特法则）
    /// </summary>
    public class HasValidTargetNode : IBehaviorNode
    {
        public NodeStatus Execute(global::EntityModule.Entity owner)
        {
            if (owner == null) return NodeStatus.Failure;

            var dataComponent = owner.GetComponent<DataComponent>();
            if (dataComponent == null || dataComponent.TargetEntityId < 0)
            {
                return NodeStatus.Failure;
            }

            global::EntityModule.Entity targetEntity = dataComponent.GetTargetEntity();
            if (targetEntity == null)
            {
                return NodeStatus.Failure;
            }

            // 检查目标是否存活（如果有战斗组件）
            var targetCombat = targetEntity.GetComponent<CombatComponent>();
            if (targetCombat != null && !targetCombat.IsAlive)
            {
                return NodeStatus.Failure;
            }

            return NodeStatus.Success;
        }
    }
}

