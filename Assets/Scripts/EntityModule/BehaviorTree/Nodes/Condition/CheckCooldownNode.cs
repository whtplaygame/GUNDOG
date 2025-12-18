using EntityModule.Component;

namespace EntityModule.BehaviorTree.Nodes.Condition
{
    /// <summary>
    /// 检查攻击CD是否冷却节点（符合迪米特法则）
    /// </summary>
    public class CheckCooldownNode : IBehaviorNode
    {
        /// <summary>
        /// 是否等待CD冷却（true: CD未冷却时返回Running，false: CD未冷却时返回Failure）
        /// </summary>
        private readonly bool waitForCooldown;

        public CheckCooldownNode(bool waitForCooldown = true)
        {
            this.waitForCooldown = waitForCooldown;
        }

        public NodeStatus Execute(Entity owner)
        {
            if (owner == null) return NodeStatus.Failure;

            var combatComponent = owner.GetComponent<CombatComponent>();
            if (combatComponent == null)
            {
                return NodeStatus.Failure;
            }

            if (combatComponent.CanAttack)
            {
                return NodeStatus.Success;
            }

            // CD未冷却
            return waitForCooldown ? NodeStatus.Running : NodeStatus.Failure;
        }
    }
}

