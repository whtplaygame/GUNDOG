using EntityModule.Component;
using UnityEngine;

namespace EntityModule.BehaviorTree.Nodes.Condition
{
    /// <summary>
    /// 检查目标是否进入危险区域节点（符合迪米特法则）
    /// 用于射手判断敌人是否过近，需要逃跑
    /// </summary>
    public class CheckDangerZoneNode : IBehaviorNode
    {
        private readonly float dangerRange;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="dangerRange">危险距离</param>
        public CheckDangerZoneNode(float dangerRange)
        {
            this.dangerRange = dangerRange;
        }

        public NodeStatus Execute(Entity owner)
        {
            if (owner == null) return NodeStatus.Failure;

            var dataComponent = owner.GetComponent<DataComponent>();
            if (dataComponent == null)
            {
                return NodeStatus.Failure;
            }

            Entity targetEntity = dataComponent.GetTargetEntity();
            if (targetEntity == null)
            {
                return NodeStatus.Failure;
            }

            // 计算距离
            float distance = Vector2Int.Distance(owner.GridPosition, targetEntity.GridPosition);

            // 如果目标在危险区域内（距离小于等于危险距离），返回Success
            return distance <= dangerRange ? NodeStatus.Success : NodeStatus.Failure;
        }
    }
}

