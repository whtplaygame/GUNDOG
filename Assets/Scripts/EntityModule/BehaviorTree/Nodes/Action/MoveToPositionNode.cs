using EntityModule.Component;
using UnityEngine;

namespace EntityModule.BehaviorTree.Nodes.Action
{
    /// <summary>
    /// 移动到指定位置节点（使用构造函数传参，符合开闭原则）
    /// </summary>
    public class MoveToPositionNode : IBehaviorNode
    {
        private readonly System.Func<Entity, Vector2Int?> positionProvider;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="positionProvider">提供目标位置的函数</param>
        public MoveToPositionNode(System.Func<Entity, Vector2Int?> positionProvider)
        {
            this.positionProvider = positionProvider;
        }

        public NodeStatus Execute(Entity owner)
        {
            if (owner == null) return NodeStatus.Failure;

            var movementComponent = owner.GetComponent<MovementComponent>();
            if (movementComponent == null)
            {
                return NodeStatus.Failure;
            }

            // 如果正在移动，继续移动
            if (movementComponent.IsMoving)
            {
                return NodeStatus.Running;
            }

            // 获取目标位置
            Vector2Int? targetPos = positionProvider?.Invoke(owner);
            if (targetPos == null)
            {
                return NodeStatus.Failure;
            }

            // 如果已经在目标位置，返回成功
            if (owner.GridPosition == targetPos.Value)
            {
                return NodeStatus.Success;
            }

            // 尝试移动到目标位置
            bool pathFound = movementComponent.SetTarget(targetPos.Value);
            if (pathFound)
            {
                return NodeStatus.Running;
            }

            return NodeStatus.Failure;
        }
    }
}

