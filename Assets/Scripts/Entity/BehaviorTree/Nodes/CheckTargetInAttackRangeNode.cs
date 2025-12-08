using UnityEngine;
using Pathfinding.Entity;
using Pathfinding.Entity.Component;

namespace Pathfinding.Entity.BehaviorTree.Nodes
{
    /// <summary>
    /// 检查目标是否在攻击范围内节点（符合迪米特法则）
    /// </summary>
    public class CheckTargetInAttackRangeNode : IBehaviorNode
    {
        public NodeStatus Execute(Entity owner)
        {
            if (owner == null) return NodeStatus.Failure;

            var dataComponent = owner.GetComponent<DataComponent>();
            var combatComponent = owner.GetComponent<CombatComponent>();
            
            if (dataComponent == null || combatComponent == null)
            {
                return NodeStatus.Failure;
            }

            Entity targetEntity = dataComponent.GetTargetEntity();
            if (targetEntity == null)
            {
                return NodeStatus.Failure;
            }

            // 检查目标是否存活
            var targetCombat = targetEntity.GetComponent<CombatComponent>();
            if (targetCombat == null || !targetCombat.IsAlive)
            {
                return NodeStatus.Failure;
            }

            // 检查距离是否在攻击范围内
            float distance = Vector2Int.Distance(owner.GridPosition, targetEntity.GridPosition);
            return distance <= combatComponent.AttackRange ? NodeStatus.Success : NodeStatus.Failure;
        }
    }
}

