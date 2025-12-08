using UnityEngine;
using Pathfinding.Entity;
using Pathfinding.Entity.Component;

namespace Pathfinding.Entity.BehaviorTree.Nodes
{
    /// <summary>
    /// 检查目标是否在范围内节点（符合迪米特法则）
    /// </summary>
    public class CheckTargetInRangeNode : IBehaviorNode
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
            if (targetEntity == null)
            {
                return NodeStatus.Failure;
            }

            float distance = Vector2Int.Distance(owner.GridPosition, targetEntity.GridPosition);
            return distance <= dataComponent.DetectionRange ? NodeStatus.Success : NodeStatus.Failure;
        }
    }
}
