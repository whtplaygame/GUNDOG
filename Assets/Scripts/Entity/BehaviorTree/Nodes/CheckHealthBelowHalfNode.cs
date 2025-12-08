using Pathfinding.Entity;
using Pathfinding.Entity.Component;

namespace Pathfinding.Entity.BehaviorTree.Nodes
{
    /// <summary>
    /// 检查血量是否低于一半节点（符合迪米特法则）
    /// </summary>
    public class CheckHealthBelowHalfNode : IBehaviorNode
    {
        public NodeStatus Execute(Entity owner)
        {
            if (owner == null) return NodeStatus.Failure;

            var combatComponent = owner.GetComponent<CombatComponent>();
            if (combatComponent == null)
            {
                return NodeStatus.Failure;
            }

            // 检查血量是否低于一半
            if (combatComponent.MaxHealth <= 0f) return NodeStatus.Failure;
            float healthPercentage = combatComponent.CurrentHealth / combatComponent.MaxHealth;
            return healthPercentage <= 0.5f ? NodeStatus.Success : NodeStatus.Failure;
        }
    }
}

