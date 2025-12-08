using Pathfinding.Entity;
using Pathfinding.Entity.Component;

namespace Pathfinding.Entity.BehaviorTree.Nodes
{
    /// <summary>
    /// 检查目标是否存在节点（符合迪米特法则）
    /// </summary>
    public class CheckTargetExistsNode : IBehaviorNode
    {
        public NodeStatus Execute(Entity owner)
        {
            if (owner == null) return NodeStatus.Failure;

            var dataComponent = owner.GetComponent<DataComponent>();
            if (dataComponent == null || dataComponent.TargetEntityId < 0)
            {
                return NodeStatus.Failure;
            }

            Entity targetEntity = dataComponent.GetTargetEntity();
            return targetEntity != null ? NodeStatus.Success : NodeStatus.Failure;
        }
    }
}
