using System.Collections.Generic;
using UnityEngine;
using Pathfinding;
using Pathfinding.Map;
using Pathfinding.Entity.Component;

namespace Pathfinding.Entity.Component
{
    /// <summary>
    /// 移动组件（单一职责原则）
    /// </summary>
    public class MovementComponent : Component
    {
        private GridManager gridManager;
        private AStarPathfinding pathfinding;
        private List<Vector2Int> currentPath;
        private int currentPathIndex = 0;
        private float moveSpeed = 2f;

        public bool HasPath => currentPath != null && currentPath.Count > 0;
        public bool IsMoving { get; private set; }

        public float MoveSpeed
        {
            get => moveSpeed;
            set => moveSpeed = Mathf.Max(0f, value);
        }

        public override void Initialize()
        {
            base.Initialize();
            
            if (Owner == null) return;

            // 获取GridManager（符合迪米特法则，通过管理器获取）
            var entityManager = EntityManager.Instance;
            if (entityManager != null)
            {
                gridManager = entityManager.GridManager;
            }

            if (gridManager != null)
            {
                pathfinding = new AStarPathfinding(gridManager.MapWidth, gridManager.MapHeight);
                pathfinding.SetNodeMap(gridManager.TileMap);
            }
        }

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);
            
            if (IsMoving && HasPath)
            {
                UpdateMovement(deltaTime);
            }
        }

        /// <summary>
        /// 设置目标位置并开始寻路
        /// </summary>
        public bool SetTarget(Vector2Int targetGridPos)
        {
            if (gridManager == null || Owner == null)
            {
                return false;
            }

            Vector2Int startPos = Owner.GridPosition;
            List<Vector2Int> path = pathfinding.FindPath(startPos, targetGridPos);

            if (path != null && path.Count > 1)
            {
                currentPath = path;
                currentPathIndex = 1;
                IsMoving = true;
                return true;
            }

            return false;
        }

        /// <summary>
        /// 更新移动
        /// </summary>
        private void UpdateMovement(float deltaTime)
        {
            if (!HasPath || Owner == null)
            {
                IsMoving = false;
                return;
            }

            Vector3 currentWorldPos = Owner.WorldPosition;
            Vector2Int currentGridPos = gridManager.WorldToGrid(currentWorldPos);
            Vector2Int targetGridPos = currentPath[currentPathIndex];

            // 如果已经到达当前目标格子，移动到下一个
            if (currentGridPos == targetGridPos)
            {
                currentPathIndex++;
                
                if (currentPathIndex >= currentPath.Count)
                {
                    // 到达最终目标
                    Owner.SetGridPosition(targetGridPos);
                    Owner.SetWorldPosition(gridManager.GridToWorld(targetGridPos));
                    currentPath = null;
                    currentPathIndex = 0;
                    IsMoving = false;
                    return;
                }
                
                targetGridPos = currentPath[currentPathIndex];
            }

            // 计算移动速度（考虑地形）
            Tile currentTile = gridManager.GetTile(currentGridPos);
            float terrainSpeed = currentTile != null ? currentTile.GetMovementSpeed() : 1f;
            float actualSpeed = moveSpeed * terrainSpeed;

            // 计算到目标格子的距离
            Vector3 targetWorldPos = gridManager.GridToWorld(targetGridPos);
            float distanceToTarget = Vector3.Distance(currentWorldPos, targetWorldPos);
            float moveDistance = actualSpeed * deltaTime;

            if (moveDistance >= distanceToTarget)
            {
                // 到达当前目标格子
                Owner.SetGridPosition(targetGridPos);
                Owner.SetWorldPosition(targetWorldPos);
                currentPathIndex++;

                if (currentPathIndex >= currentPath.Count)
                {
                    currentPath = null;
                    currentPathIndex = 0;
                    IsMoving = false;
                }
            }
            else
            {
                // 移动到目标格子的中间位置
                Vector3 direction = (targetWorldPos - currentWorldPos).normalized;
                Vector3 newWorldPos = currentWorldPos + direction * moveDistance;
                Owner.SetWorldPosition(newWorldPos);

                Vector2Int newGridPos = gridManager.WorldToGrid(newWorldPos);
                if (newGridPos != currentGridPos)
                {
                    Owner.SetGridPosition(newGridPos);
                }
            }
        }

        /// <summary>
        /// 停止移动
        /// </summary>
        public void Stop()
        {
            currentPath = null;
            currentPathIndex = 0;
            IsMoving = false;
        }

        /// <summary>
        /// 获取当前路径
        /// </summary>
        public List<Vector2Int> GetCurrentPath()
        {
            return currentPath;
        }
    }
}

