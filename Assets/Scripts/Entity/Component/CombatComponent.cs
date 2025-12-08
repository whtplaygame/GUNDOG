using UnityEngine;

namespace Pathfinding.Entity.Component
{
    /// <summary>
    /// 战斗组件（存储血量和攻击力，符合单一职责原则）
    /// </summary>
    public class CombatComponent : Component
    {
        [SerializeField] private float maxHealth = 100f;
        [SerializeField] private float currentHealth = 100f;
        [SerializeField] private float attackPower = 10f;
        [SerializeField] private float attackRange = 1f;
        [SerializeField] private float attackCooldown = 1f; // 攻击CD（秒）

        private float attackCooldownTimer = 0f;

        public float MaxHealth
        {
            get => maxHealth;
            set
            {
                maxHealth = Mathf.Max(0f, value);
                currentHealth = Mathf.Min(currentHealth, maxHealth);
            }
        }

        public float CurrentHealth
        {
            get => currentHealth;
            set => currentHealth = Mathf.Clamp(value, 0f, maxHealth);
        }

        public float AttackPower
        {
            get => attackPower;
            set => attackPower = Mathf.Max(0f, value);
        }

        public float AttackRange
        {
            get => attackRange;
            set => attackRange = Mathf.Max(0f, value);
        }

        public float AttackCooldown
        {
            get => attackCooldown;
            set => attackCooldown = Mathf.Max(0f, value);
        }

        public float AttackCooldownRemaining => attackCooldownTimer;
        public bool HasAttackCooldown => attackCooldownTimer > 0f;
        public bool CanAttack => attackCooldownTimer <= 0f;

        public bool IsAlive => currentHealth > 0f;

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);

            // 更新攻击CD
            if (attackCooldownTimer > 0f)
            {
                attackCooldownTimer -= deltaTime;
                if (attackCooldownTimer < 0f)
                {
                    attackCooldownTimer = 0f;
                }
            }

            // 检查血量，如果为0则销毁Entity
            if (!IsAlive && Owner != null)
            {
                var entityManager = EntityManager.Instance;
                if (entityManager != null)
                {
                    entityManager.DestroyEntity(Owner);
                }
            }
        }

        /// <summary>
        /// 受到伤害
        /// </summary>
        public void TakeDamage(float damage)
        {
            if (!IsAlive) return;
            currentHealth = Mathf.Max(0f, currentHealth - damage);
        }

        /// <summary>
        /// 治疗
        /// </summary>
        public void Heal(float amount)
        {
            if (!IsAlive) return;
            currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        }

        /// <summary>
        /// 攻击目标（带CD检查）
        /// </summary>
        public bool Attack(Entity target)
        {
            if (!IsAlive || target == null) return false;
            if (!CanAttack) return false; // CD未冷却

            var targetCombat = target.GetComponent<CombatComponent>();
            if (targetCombat == null || !targetCombat.IsAlive) return false;

            // 检查攻击范围
            if (Owner != null)
            {
                float distance = Vector2Int.Distance(Owner.GridPosition, target.GridPosition);
                if (distance > attackRange) return false;
            }

            // 执行攻击
            targetCombat.TakeDamage(attackPower);
            
            // 重置CD
            attackCooldownTimer = attackCooldown;
            
            return true;
        }
    }
}

