using EntityModule.Component;
using UnityEngine;

namespace EntityModule.BehaviorTree.Nodes.Action
{
    /// <summary>
    /// 从目标逃跑节点（符合迪米特法则）
    /// 计算逃离方向并移动一小段距离
    /// </summary>
    public class FleeFromTargetNode : IBehaviorNode
    {
        private readonly float fleeDistance; // 逃跑距离
        private Vector2Int? fleePosition = null; // 缓存逃跑目标位置

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="fleeDistance">逃跑距离（格子单位）</param>
        public FleeFromTargetNode(float fleeDistance = 3f)
        {
            this.fleeDistance = fleeDistance;
        }

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

            // 检查能否移动：攻击/硬直时不能逃跑
            if (combatComponent != null)
            {
                // 关键：攻击中或受击硬直中都不能立刻执行逃跑
                if (!combatComponent.CanMove || combatComponent.AttackState != AttackState.Idle)
                {
                    // 如果正在移动，停止
                    if (movementComponent.IsMoving)
                    {
                        movementComponent.Stop();
                    }
                    // 清除缓存的逃跑位置，下次重新计算
                    fleePosition = null;
                    return NodeStatus.Failure;
                }
            }

            Entity targetEntity = dataComponent.GetTargetEntity();
            if (targetEntity == null)
            {
                fleePosition = null;
                return NodeStatus.Failure;
            }

            // 如果没有缓存逃跑位置，或者已经到达逃跑位置，重新计算
            if (fleePosition == null || owner.GridPosition == fleePosition.Value)
            {
                // 计算逃离方向
                Vector2Int ownerPos = owner.GridPosition;
                Vector2Int targetPos = targetEntity.GridPosition;
                Vector2Int fleeDirection = ownerPos - targetPos; // 远离目标的方向

                // 如果和目标在同一位置，随机选择一个方向
                if (fleeDirection == Vector2Int.zero)
                {
                    fleeDirection = new Vector2Int(Random.Range(-1, 2), Random.Range(-1, 2));
                    if (fleeDirection == Vector2Int.zero)
                    {
                        fleeDirection = Vector2Int.right;
                    }
                }

                // 归一化方向并计算逃跑目标位置
                float distance = Mathf.Max(1f, fleeDirection.magnitude);
                Vector2 normalizedDirection = new Vector2(fleeDirection.x / distance, fleeDirection.y / distance);
                Vector2 fleeOffset = normalizedDirection * fleeDistance;
                
                fleePosition = new Vector2Int(
                    Mathf.RoundToInt(ownerPos.x + fleeOffset.x),
                    Mathf.RoundToInt(ownerPos.y + fleeOffset.y)
                );

                // 验证逃跑位置是否合法
                var entityManager = EntityManager.Instance;
                if (entityManager != null && entityManager.GridManager != null)
                {
                    var gridManager = entityManager.GridManager;
                    
                    // 检查是否在地图范围内
                    if (fleePosition.Value.x < 0 || fleePosition.Value.x >= gridManager.MapWidth ||
                        fleePosition.Value.y < 0 || fleePosition.Value.y >= gridManager.MapHeight)
                    {
                        // 如果超出边界，尝试调整到边界内
                        fleePosition = new Vector2Int(
                            Mathf.Clamp(fleePosition.Value.x, 0, gridManager.MapWidth - 1),
                            Mathf.Clamp(fleePosition.Value.y, 0, gridManager.MapHeight - 1)
                        );
                    }

                    // 检查目标格子是否可走
                    var tile = gridManager.GetTile(fleePosition.Value);
                    if (tile == null || !tile.IsWalkable())
                    {
                        // 如果不可走，尝试找附近可走的格子
                        Vector2Int? nearbyWalkable = FindNearbyWalkablePosition(ownerPos, fleePosition.Value, gridManager);
                        if (nearbyWalkable.HasValue)
                        {
                            fleePosition = nearbyWalkable.Value;
                        }
                        else
                        {
                            // 找不到可走的位置，逃跑失败
                            fleePosition = null;
                            return NodeStatus.Failure;
                        }
                    }
                }

                // 开始移动到逃跑位置
                bool pathFound = movementComponent.SetTarget(fleePosition.Value);
                if (pathFound)
                {
                    return NodeStatus.Running;
                }
                else
                {
                    fleePosition = null;
                    return NodeStatus.Failure;
                }
            }

            // 如果已经在移动中
            if (movementComponent.IsMoving)
            {
                return NodeStatus.Running;
            }
            else
            {
                // 移动完成，清除缓存位置
                fleePosition = null;
                return NodeStatus.Success;
            }
        }

        /// <summary>
        /// 查找附近可走的位置
        /// </summary>
        private Vector2Int? FindNearbyWalkablePosition(Vector2Int start, Vector2Int target, Map.GridManager gridManager)
        {
            // 搜索半径
            int searchRadius = 3;

            for (int radius = 1; radius <= searchRadius; radius++)
            {
                for (int dx = -radius; dx <= radius; dx++)
                {
                    for (int dy = -radius; dy <= radius; dy++)
                    {
                        if (Mathf.Abs(dx) != radius && Mathf.Abs(dy) != radius)
                            continue; // 只检查圆周上的点

                        Vector2Int checkPos = new Vector2Int(target.x + dx, target.y + dy);

                        // 检查是否在地图范围内
                        if (checkPos.x < 0 || checkPos.x >= gridManager.MapWidth ||
                            checkPos.y < 0 || checkPos.y >= gridManager.MapHeight)
                            continue;

                        var tile = gridManager.GetTile(checkPos);
                        if (tile != null && tile.IsWalkable())
                        {
                            return checkPos;
                        }
                    }
                }
            }

            return null;
        }
    }
}

