using UnityEngine;
using Pathfinding.Entity;
using Pathfinding.Entity.Component;

namespace Pathfinding.Entity.BehaviorTree.Nodes
{
    /// <summary>
    /// 随机移动节点（符合迪米特法则）
    /// </summary>
    public class RandomMoveNode : IBehaviorNode
    {
        private float lastMoveTime = 0f;
        private float moveInterval = 2f;  // 移动间隔（秒）
        private Vector2Int? currentTarget;

        public NodeStatus Execute(Entity owner)
        {
            if (owner == null) return NodeStatus.Failure;

            var movementComponent = owner.GetComponent<MovementComponent>();
            if (movementComponent == null)
            {
                return NodeStatus.Failure;
            }

            EntityManager manager = EntityManager.Instance;
            if (manager == null || manager.GridManager == null)
            {
                return NodeStatus.Failure;
            }

            // 如果正在移动，继续移动
            if (movementComponent.IsMoving)
            {
                return NodeStatus.Running;
            }

            // 检查是否需要选择新目标
            bool needNewTarget = false;
            if (currentTarget == null)
            {
                needNewTarget = true;
            }
            else if (Time.time - lastMoveTime >= moveInterval)
            {
                needNewTarget = true;
            }

            if (needNewTarget)
            {
                // 随机选择一个附近的格子作为目标
                Vector2Int currentPos = owner.GridPosition;
                Vector2Int randomTarget = GetRandomNearbyPosition(currentPos, manager.GridManager, 3);

                // 尝试移动到目标位置
                bool pathFound = movementComponent.SetTarget(randomTarget);
                if (pathFound)
                {
                    currentTarget = randomTarget;
                    lastMoveTime = Time.time;
                    return NodeStatus.Running;
                }
                else
                {
                    currentTarget = null;
                    return NodeStatus.Failure;
                }
            }

            return NodeStatus.Success;
        }

        /// <summary>
        /// 获取随机附近位置
        /// </summary>
        private Vector2Int GetRandomNearbyPosition(Vector2Int center, Pathfinding.Map.GridManager gridManager, int range)
        {
            int attempts = 10;
            for (int i = 0; i < attempts; i++)
            {
                int offsetX = Random.Range(-range, range + 1);
                int offsetY = Random.Range(-range, range + 1);
                Vector2Int candidate = center + new Vector2Int(offsetX, offsetY);

                // 检查是否在地图范围内
                if (candidate.x >= 0 && candidate.x < gridManager.MapWidth &&
                    candidate.y >= 0 && candidate.y < gridManager.MapHeight)
                {
                    // 检查是否可通行
                    var tile = gridManager.GetTile(candidate);
                    if (tile != null && tile.IsWalkable())
                    {
                        return candidate;
                    }
                }
            }

            // 如果找不到合适的位置，返回当前位置
            return center;
        }
    }
}
