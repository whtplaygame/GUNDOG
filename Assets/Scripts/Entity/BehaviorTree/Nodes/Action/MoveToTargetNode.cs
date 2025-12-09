using Pathfinding.Entity;
using Pathfinding.Entity.Component;

namespace Pathfinding.Entity.BehaviorTree.Nodes.Action
{
    /// <summary>
    /// 移动到目标节点（符合迪米特法则）
    /// </summary>
    public class MoveToTargetNode : IBehaviorNode
    {
        public NodeStatus Execute(Entity owner)
        {
            if (owner == null) return NodeStatus.Failure;

            var dataComponent = owner.GetComponent<DataComponent>();
            var movementComponent = owner.GetComponent<MovementComponent>();
            
            if (dataComponent == null || movementComponent == null)
            {
                return NodeStatus.Failure;
            }

            Entity targetEntity = dataComponent.GetTargetEntity();
            if (targetEntity == null)
            {
                return NodeStatus.Failure;
            }

            // 如果已经在目标位置，返回成功
            if (owner.GridPosition == targetEntity.GridPosition)
            {
                return NodeStatus.Success;
            }

            // 尝试移动到目标
            bool pathFound = movementComponent.SetTarget(targetEntity.GridPosition);
            if (pathFound)
            {
                // 如果正在移动，返回运行中
                if (movementComponent.IsMoving)
                {
                    return NodeStatus.Running;
                }
                return NodeStatus.Success;
            }
            
            return NodeStatus.Failure;
        }
    }
}

