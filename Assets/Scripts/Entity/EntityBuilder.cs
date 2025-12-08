using UnityEngine;
using Pathfinding.Entity.Component;
using Pathfinding.Entity.BehaviorTree;
using Pathfinding.Map;
using BT = Pathfinding.Entity.BehaviorTree;

namespace Pathfinding.Entity
{
    /// <summary>
    /// 实体构建器（参考饥荒设计）
    /// 提供类似饥荒的Entity创建接口，任何人都可以通过这个接口创建新Entity
    /// </summary>
    public static class EntityBuilder
    {
        /// <summary>
        /// 创建一个空的实体（类似饥荒的CreateEntity）
        /// </summary>
        public static Entity CreateEntity(string name = "Entity")
        {
            GameObject entityObj = new GameObject(name);
            Entity entity = entityObj.AddComponent<Entity>();
            return entity;
        }

        /// <summary>
        /// 添加Transform组件（坐标）
        /// </summary>
        public static EntityBuilderHelper AddTransform(this Entity entity, Vector2Int gridPos, Vector3 worldPos, EntityPriority priority = EntityPriority.Active)
        {
            if (entity == null) return null;

            // 从EntityManager获取下一个ID
            int id = EntityManager.Instance != null ? EntityManager.Instance.GetNextEntityId() : 0;
            
            // 初始化实体位置
            entity.Initialize(id, priority, gridPos, worldPos);
            return new EntityBuilderHelper(entity);
        }

        /// <summary>
        /// 添加数据组件
        /// </summary>
        public static EntityBuilderHelper AddData(this EntityBuilderHelper helper, EntityType entityType, float detectionRange = 10f)
        {
            if (helper?.Entity == null) return helper;

            var dataComponent = helper.Entity.AddComponent<DataComponent>();
            dataComponent.EntityType = entityType;
            dataComponent.DetectionRange = detectionRange;
            return helper;
        }

        /// <summary>
        /// 添加移动组件（寻路能力）
        /// </summary>
        public static EntityBuilderHelper AddLocomotor(this EntityBuilderHelper helper, float runSpeed = 2f)
        {
            if (helper?.Entity == null) return helper;

            var movementComponent = helper.Entity.AddComponent<MovementComponent>();
            movementComponent.MoveSpeed = runSpeed;
            return helper;
        }

        /// <summary>
        /// 添加视图组件（表现）
        /// </summary>
        public static EntityBuilderHelper AddView(this EntityBuilderHelper helper, Color color, float viewHeight = 0.5f)
        {
            if (helper?.Entity == null) return helper;

            var viewComponent = helper.Entity.AddComponent<ViewComponent>();
            viewComponent.Color = color;
            return helper;
        }

        /// <summary>
        /// 添加战斗组件（血量和攻击力）
        /// </summary>
        public static EntityBuilderHelper AddCombat(this EntityBuilderHelper helper, float maxHealth = 100f, float attackPower = 10f, float attackRange = 1f, float attackCooldown = 1f)
        {
            if (helper?.Entity == null) return helper;

            var combatComponent = helper.Entity.AddComponent<CombatComponent>();
            combatComponent.MaxHealth = maxHealth;
            combatComponent.CurrentHealth = maxHealth;
            combatComponent.AttackPower = attackPower;
            combatComponent.AttackRange = attackRange;
            combatComponent.AttackCooldown = attackCooldown;
            return helper;
        }

        /// <summary>
        /// 设置行为树（AI大脑）
        /// </summary>
        public static EntityBuilderHelper SetBrain(this EntityBuilderHelper helper, IBehaviorNode brain)
        {
            if (helper?.Entity == null || brain == null) return helper;

            var behaviorTreeComponent = helper.Entity.AddComponent<BehaviorTreeComponent>();
            var behaviorTree = new BT.BehaviorTree(helper.Entity, brain);
            behaviorTreeComponent.SetBehaviorTree(behaviorTree);
            return helper;
        }

        /// <summary>
        /// 设置行为树组件（使用预定义的行为树组件）
        /// </summary>
        public static EntityBuilderHelper SetBrain<T>(this EntityBuilderHelper helper) where T : BehaviorTreeComponent, new()
        {
            if (helper?.Entity == null) return helper;

            helper.Entity.AddComponent<T>();
            return helper;
        }

        /// <summary>
        /// 完成构建并注册到管理器
        /// </summary>
        public static Entity Build(this EntityBuilderHelper helper)
        {
            if (helper?.Entity == null) return null;

            // 注册到管理器
            EntityManager.Instance?.RegisterEntity(helper.Entity);
            return helper.Entity;
        }
    }

    /// <summary>
    /// 实体构建器辅助类（用于链式调用）
    /// </summary>
    public class EntityBuilderHelper
    {
        public Entity Entity { get; private set; }

        public EntityBuilderHelper(Entity entity)
        {
            Entity = entity;
        }
    }
}

