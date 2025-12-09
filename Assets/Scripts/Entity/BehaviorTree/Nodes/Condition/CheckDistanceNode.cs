using UnityEngine;
using Pathfinding.Entity;
using Pathfinding.Entity.Component;

namespace Pathfinding.Entity.BehaviorTree.Nodes.Condition
{
    /// <summary>
    /// 通用的距离检查节点（使用构造函数传参，符合开闭原则）
    /// </summary>
    public class CheckDistanceNode : IBehaviorNode
    {
        private readonly string rangePropertyKey; // 例如 "AttackRange" 或 "DetectionRange"
        private readonly bool checkTarget; // true: 检查到目标的距离, false: 检查到其他Entity的距离

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="rangePropertyKey">范围属性键名，用于从CombatComponent或DataComponent获取范围值</param>
        /// <param name="checkTarget">是否检查目标距离，true检查目标，false检查其他</param>
        public CheckDistanceNode(string rangePropertyKey, bool checkTarget = true)
        {
            this.rangePropertyKey = rangePropertyKey;
            this.checkTarget = checkTarget;
        }

        public NodeStatus Execute(Entity owner)
        {
            if (owner == null) return NodeStatus.Failure;

            var dataComponent = owner.GetComponent<DataComponent>();
            if (dataComponent == null)
            {
                return NodeStatus.Failure;
            }

            Entity targetEntity = null;
            if (checkTarget)
            {
                targetEntity = dataComponent.GetTargetEntity();
                if (targetEntity == null)
                {
                    return NodeStatus.Failure;
                }
            }

            // 根据key获取范围值
            float range = GetRangeValue(owner, rangePropertyKey);
            if (range <= 0f)
            {
                return NodeStatus.Failure;
            }

            // 计算距离
            float distance = 0f;
            if (checkTarget && targetEntity != null)
            {
                distance = Vector2Int.Distance(owner.GridPosition, targetEntity.GridPosition);
            }

            return distance <= range ? NodeStatus.Success : NodeStatus.Failure;
        }

        /// <summary>
        /// 根据key从组件获取范围值
        /// </summary>
        private float GetRangeValue(Entity owner, string key)
        {
            switch (key)
            {
                case "AttackRange":
                    var combatComponent = owner.GetComponent<CombatComponent>();
                    return combatComponent != null ? combatComponent.AttackRange : 0f;
                
                case "DetectionRange":
                    var dataComponent = owner.GetComponent<DataComponent>();
                    return dataComponent != null ? dataComponent.DetectionRange : 0f;
                
                default:
                    return 0f;
            }
        }
    }
}

