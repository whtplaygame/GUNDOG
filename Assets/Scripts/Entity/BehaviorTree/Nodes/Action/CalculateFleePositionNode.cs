using UnityEngine;
using Pathfinding.Entity;
using Pathfinding.Entity.Component;
using Pathfinding.Map;

namespace Pathfinding.Entity.BehaviorTree.Nodes.Action
{
    /// <summary>
    /// 计算逃跑位置节点（符合迪米特法则）
    /// </summary>
    public class CalculateFleePositionNode : IBehaviorNode
    {
        private Vector2Int? calculatedFleePosition;

        public NodeStatus Execute(Entity owner)
        {
            if (owner == null) return NodeStatus.Failure;

            var dataComponent = owner.GetComponent<DataComponent>();
            if (dataComponent == null)
            {
                return NodeStatus.Failure;
            }

            Entity chaserEntity = dataComponent.GetTargetEntity();
            if (chaserEntity == null)
            {
                calculatedFleePosition = null;
                return NodeStatus.Failure;
            }

            EntityManager manager = EntityManager.Instance;
            if (manager == null || manager.GridManager == null)
            {
                return NodeStatus.Failure;
            }

            GridManager gridManager = manager.GridManager;
            Vector2Int currentPos = owner.GridPosition;
            Vector2Int chaserPos = chaserEntity.GridPosition;
            
            // 计算逃跑方向（远离Chaser）
            Vector2Int direction = currentPos - chaserPos;

            // 如果方向为零向量，随机选择一个方向
            if (direction.x == 0 && direction.y == 0)
            {
                direction = new Vector2Int(Random.Range(-1, 2), Random.Range(-1, 2));
            }

            // 归一化方向（取符号）
            if (direction.x != 0) direction.x = direction.x > 0 ? 1 : -1;
            if (direction.y != 0) direction.y = direction.y > 0 ? 1 : -1;

            // 计算逃跑目标位置（远离Chaser的方向，距离为检测范围的1.5倍）
            int fleeDistance = Mathf.CeilToInt(dataComponent.DetectionRange * 1.5f);
            Vector2Int targetPos = currentPos + direction * fleeDistance;

            // 确保目标位置在地图范围内
            targetPos.x = Mathf.Clamp(targetPos.x, 0, gridManager.MapWidth - 1);
            targetPos.y = Mathf.Clamp(targetPos.y, 0, gridManager.MapHeight - 1);

            // 如果目标位置不可通行，尝试找到附近的可通行位置
            Tile targetTile = gridManager.GetTile(targetPos);
            if (targetTile == null || !targetTile.IsWalkable())
            {
                targetPos = FindNearestWalkablePosition(targetPos, gridManager, 3);
            }

            calculatedFleePosition = targetPos;
            return NodeStatus.Success;
        }

        /// <summary>
        /// 获取计算出的逃跑位置
        /// </summary>
        public Vector2Int? GetFleePosition()
        {
            return calculatedFleePosition;
        }

        /// <summary>
        /// 查找最近的可通行位置
        /// </summary>
        private Vector2Int FindNearestWalkablePosition(Vector2Int center, GridManager gridManager, int searchRange)
        {
            for (int range = 1; range <= searchRange; range++)
            {
                for (int x = -range; x <= range; x++)
                {
                    for (int y = -range; y <= range; y++)
                    {
                        Vector2Int candidate = center + new Vector2Int(x, y);
                        
                        if (candidate.x >= 0 && candidate.x < gridManager.MapWidth &&
                            candidate.y >= 0 && candidate.y < gridManager.MapHeight)
                        {
                            Tile tile = gridManager.GetTile(candidate);
                            if (tile != null && tile.IsWalkable())
                            {
                                return candidate;
                            }
                        }
                    }
                }
            }

            return center;
        }
    }
}

