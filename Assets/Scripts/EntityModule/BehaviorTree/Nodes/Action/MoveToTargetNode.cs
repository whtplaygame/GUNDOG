using EntityModule.Component;
using UnityEngine;

namespace EntityModule.BehaviorTree.Nodes.Action
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
            var combatComponent = owner.GetComponent<CombatComponent>();
            
            if (dataComponent == null || movementComponent == null)
            {
                return NodeStatus.Failure;
            }

            // 检查硬直状态
            if (combatComponent != null && combatComponent.IsInHitStun)
            {
                // 硬直期间停止移动
                if (movementComponent.IsMoving)
                {
                    movementComponent.Stop();
                }
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

            // 动态更新目标位置（目标可能移动）
            Vector2Int targetPos = targetEntity.GridPosition;
            
            // 如果目标位置变化或还没开始移动，重新设置目标
            if (!movementComponent.IsMoving)
            {
                bool pathFound = movementComponent.SetTarget(targetPos);
                if (pathFound)
                {
                    return NodeStatus.Running;
                }
                return NodeStatus.Failure;
            }
            else
            {
                // 正在移动，检查是否需要更新路径（如果目标移动了）
                // 这里可以优化：只在目标移动一定距离后才重新寻路，避免每帧寻路
                // 暂时保持简单：如果正在移动就继续移动
                return NodeStatus.Running;
            }
        }
    }
}

