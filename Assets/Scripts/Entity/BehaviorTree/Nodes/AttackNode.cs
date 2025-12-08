using UnityEngine;
using Pathfinding.Entity;
using Pathfinding.Entity.Component;

namespace Pathfinding.Entity.BehaviorTree.Nodes
{
    /// <summary>
    /// 攻击节点（符合迪米特法则：只通过组件访问）
    /// </summary>
    public class AttackNode : IBehaviorNode
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

            // 检查是否存活
            if (!combatComponent.IsAlive)
            {
                return NodeStatus.Failure;
            }

            // 检查是否可以攻击（CD是否冷却）
            if (!combatComponent.CanAttack)
            {
                return NodeStatus.Running; // CD未冷却，继续等待
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

            // 检查是否在攻击范围内
            float distance = Vector2Int.Distance(owner.GridPosition, targetEntity.GridPosition);
            if (distance > combatComponent.AttackRange)
            {
                return NodeStatus.Failure; // 不在攻击范围内，需要先靠近
            }

            // 执行攻击
            bool attackSuccess = combatComponent.Attack(targetEntity);
            if (attackSuccess)
            {
                return NodeStatus.Success;
            }

            return NodeStatus.Failure;
        }
    }
}

