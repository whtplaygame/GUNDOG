using System.Collections.Generic;
using Map;
using Pathfinding;
using UnityEngine;

namespace EntityModule.Component
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

        public override void TickLogic(float deltaTime)
        {
            base.TickLogic(deltaTime);
            
            // 检查硬直状态，硬直时停止移动
            if (Owner != null)
            {
                var combatComponent = Owner.GetComponent<CombatComponent>();
                // 如果不能移动（硬直 或 正在攻击且未进入可移动阶段），则强制停止
                if (combatComponent != null && !combatComponent.CanMove)
                {
                    if (IsMoving)
                    {
                        Stop();
                    }
                    return;
                }
            }
            
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

            // 检查硬直状态
            var combatComponent = Owner.GetComponent<CombatComponent>();
            if (combatComponent != null && !combatComponent.CanMove)
            {
                return false; // 严格检查：硬直或攻击中都不能开始移动
            }

            Vector2Int startPos = Owner.GridPosition;
            List<Vector2Int> path = pathfinding.FindPath(startPos, targetGridPos);

            if (path != null && path.Count > 1)
            {
                currentPath = path;
                currentPathIndex = 1;
                IsMoving = true;
                
                // 通知动画组件开始移动
                NotifyAnimationComponent();
                
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
            // 注意：使用距离判断而不是精确相等，避免因为浮点误差导致的跳跃
            Vector3 targetWorldPos = gridManager.GridToWorld(targetGridPos);
            float distanceToCurrentTarget = Vector3.Distance(currentWorldPos, targetWorldPos);
            
            // 计算到目标格子的距离
            float distanceToTarget = Vector3.Distance(currentWorldPos, targetWorldPos);
            
            // 如果已经到达当前目标格子，移动到下一个
            if (distanceToTarget < 0.01f) // 非常接近目标格子（1厘米内）
            {
                // 更新网格位置
                Owner.SetGridPosition(targetGridPos);
                currentPathIndex++;
                
                if (currentPathIndex >= currentPath.Count)
                {
                    // 到达最终目标，精确设置位置
                    Owner.SetWorldPosition(targetWorldPos);
                    currentPath = null;
                    currentPathIndex = 0;
                    IsMoving = false;
                    
                    // 通知动画组件停止移动
                    NotifyAnimationComponent();
                    
                    return;
                }
                
                // 切换到下一个格子
                targetGridPos = currentPath[currentPathIndex];
                targetWorldPos = gridManager.GridToWorld(targetGridPos);
                distanceToTarget = Vector3.Distance(currentWorldPos, targetWorldPos);
            }

            // 计算移动速度（考虑地形）
            Tile currentTile = gridManager.GetTile(currentGridPos);
            float terrainSpeed = currentTile != null ? currentTile.GetMovementSpeed() : 1f;
            float actualSpeed = moveSpeed * terrainSpeed;

            // 计算到目标格子的距离（使用上面计算好的targetWorldPos）
            distanceToTarget = Vector3.Distance(currentWorldPos, targetWorldPos);
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
                    
                    // 通知动画组件停止移动
                    NotifyAnimationComponent();
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
        /// <param name="updateView">是否更新视图（动画）。如果在攻击开始时调用，设为false以避免先切Idle再切Attack导致的动画延迟。</param>
        public void Stop(bool updateView = true)
        {
            currentPath = null;
            currentPathIndex = 0;
            IsMoving = false;
            
            // 将位置对齐到当前格子，防止半格残留造成“滑步”错觉
            if (Owner != null && gridManager != null)
            {
                var snappedWorld = gridManager.GridToWorld(Owner.GridPosition);
                Owner.SetWorldPosition(snappedWorld);
            }
            
            // 通知动画组件停止移动
            if (updateView)
            {
                NotifyAnimationComponent();
            }
        }

        /// <summary>
        /// 通知动画组件状态变化
        /// </summary>
        private void NotifyAnimationComponent()
        {
            if (Owner == null) return;
            
            var animComponent = Owner.GetComponent<AnimationComponent>();
            if (animComponent != null)
            {
                if (IsMoving)
                {
                    animComponent.SetState(AnimationState.Move);
                }
                else
                {
                    animComponent.SetState(AnimationState.Idle);
                }
            }
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

