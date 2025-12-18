using EntityModule.Component;
using UnityEngine;

namespace EntityModule.BehaviorTree.Nodes.Action
{
    /// <summary>
    /// 动态移动到指定位置节点（每帧或隔几帧重新计算目标位置）
    /// 适用于逃跑等需要动态调整目标的行为
    /// </summary>
    public class DynamicMoveToPositionNode : IBehaviorNode
    {
        private readonly System.Func<global::EntityModule.Entity, Vector2Int?> positionProvider;
        private readonly float updateInterval; // 更新间隔（秒），0表示每帧更新
        private float lastUpdateTime = 0f;
        private Vector2Int? lastTargetPos;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="positionProvider">提供目标位置的函数</param>
        /// <param name="updateInterval">更新间隔（秒），0表示每帧更新，默认0.1秒</param>
        public DynamicMoveToPositionNode(System.Func<global::EntityModule.Entity, Vector2Int?> positionProvider, float updateInterval = 0.1f)
        {
            this.positionProvider = positionProvider;
            this.updateInterval = Mathf.Max(0f, updateInterval);
        }

        public NodeStatus Execute(global::EntityModule.Entity owner)
        {
            if (owner == null) return NodeStatus.Failure;

            var movementComponent = owner.GetComponent<MovementComponent>();
            if (movementComponent == null)
            {
                return NodeStatus.Failure;
            }

            // 检查硬直状态
            var combatComponent = owner.GetComponent<CombatComponent>();
            if (combatComponent != null && combatComponent.IsInHitStun)
            {
                // 硬直期间停止移动
                if (movementComponent.IsMoving)
                {
                    movementComponent.Stop();
                }
                return NodeStatus.Failure;
            }

            // 检查是否需要更新目标位置
            bool needUpdate = false;
            float currentTime = Time.time;
            
            if (updateInterval <= 0f)
            {
                // 每帧更新
                needUpdate = true;
            }
            else if (currentTime - lastUpdateTime >= updateInterval)
            {
                // 达到更新间隔
                needUpdate = true;
                lastUpdateTime = currentTime;
            }

            // 如果正在移动且不需要更新，继续移动
            if (movementComponent.IsMoving && !needUpdate)
            {
                return NodeStatus.Running;
            }

            // 获取目标位置
            Vector2Int? targetPos = positionProvider?.Invoke(owner);
            if (targetPos == null)
            {
                return NodeStatus.Failure;
            }

            // 如果目标位置没有变化且正在移动，继续移动
            if (targetPos == lastTargetPos && movementComponent.IsMoving)
            {
                return NodeStatus.Running;
            }

            lastTargetPos = targetPos;

            // 如果已经在目标位置（允许一定误差），返回成功
            Vector2Int currentPos = owner.GridPosition;
            if (currentPos == targetPos.Value)
            {
                return NodeStatus.Success;
            }

            // 尝试移动到目标位置（如果目标位置变化或还没开始移动，重新设置目标）
            if (!movementComponent.IsMoving || targetPos != lastTargetPos)
            {
                bool pathFound = movementComponent.SetTarget(targetPos.Value);
                if (pathFound)
                {
                    return NodeStatus.Running;
                }
            }
            else
            {
                // 正在移动，继续移动
                return NodeStatus.Running;
            }

            return NodeStatus.Failure;
        }
    }
}

