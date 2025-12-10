using System;
using System.Collections.Generic;
using EntityModule.Component;
using Map;
using UnityEngine;
using EntityEntity = EntityModule.Entity;

namespace EntityModule
{
    /// <summary>
    /// 实体工厂（注册表模式，支持Mod扩展）
    /// </summary>
    public static class EntityFactory
    {
        // 注册表：存储官方或Mod定义的实体配方
        private static Dictionary<string, EntityDefinition> definitions = new Dictionary<string, EntityDefinition>();

        // Mod 钩子：当某个ID的实体被创建后触发
        // key: EntityTypeID, value: 修改实体的回调列表
        private static Dictionary<string, List<Action<EntityEntity>>> postProcessHooks = new Dictionary<string, List<Action<EntityEntity>>>();

        /// <summary>
        /// 注册定义（官方或Mod调用）
        /// </summary>
        public static void Register(EntityDefinition def)
        {
            if (definitions.ContainsKey(def.EntityTypeID))
            {
                Debug.LogWarning($"实体定义 {def.EntityTypeID} 已存在，将被覆盖");
            }
            definitions[def.EntityTypeID] = def;
        }

        /// <summary>
        /// 注册 Mod 钩子（Mod开发者调用）
        /// </summary>
        public static void RegisterPostBuildHook(string entityTypeID, Action<EntityEntity> callback)
        {
            if (!postProcessHooks.ContainsKey(entityTypeID))
            {
                postProcessHooks[entityTypeID] = new List<Action<EntityEntity>>();
            }
            postProcessHooks[entityTypeID].Add(callback);
        }

        /// <summary>
        /// 统一创建入口（注册表模式）
        /// </summary>
        public static EntityEntity Create(string entityTypeID, Vector2Int gridPos, GridManager gridManager)
        {
            if (!definitions.TryGetValue(entityTypeID, out var def))
            {
                throw new System.Exception($"未找到实体定义: {entityTypeID}");
            }

            Vector3 worldPos = gridManager.GridToWorld(gridPos);

            // 1. 初始化 Builder
            var entity = EntityBuilder.CreateEntity(entityTypeID);
            var builder = entity.AddTransform(gridPos, worldPos, EntityPriority.Active);

            // 2. 应用基础配置（官方配方）
            def.ConfigureComponents?.Invoke(builder);

            // 3. 构建实体
            entity = builder.Build();

            // 4. 构建 AI（如果有定义）
            if (def.BuildBehaviorTree != null)
            {
                var rootNode = def.BuildBehaviorTree(entity);
                // 获取或添加行为树组件
                var btComponent = entity.GetComponent<BehaviorTreeComponent>();
                if (btComponent == null)
                {
                    // 如果配方没加BT组件，这里补救
                    btComponent = entity.AddComponent<BehaviorTreeComponent>();
                }
                // 设置行为树（如果组件已经初始化了，需要重新设置）
                var behaviorTree = new global::EntityModule.BehaviorTree.BehaviorTree(entity, rootNode);
                btComponent.SetBehaviorTree(behaviorTree);
            }

            // 5. 【关键】执行 Mod 钩子
            // 第三方开发者可以在这里修改生成好的 Entity
            if (postProcessHooks.TryGetValue(entityTypeID, out var hooks))
            {
                foreach (var hook in hooks)
                {
                    hook(entity);
                }
            }

            return entity;
        }

        // ========== 兼容性方法（保留旧的接口，内部调用新的注册表模式） ==========

        /// <summary>
        /// 创建追逐者（兼容性方法，建议使用Create("Chaser", ...)）
        /// </summary>
        [Obsolete("建议使用 EntityFactory.Create(\"Chaser\", ...) 代替")]
        public static EntityEntity CreateChaser(Vector2Int gridPos, GridManager gridManager, float runSpeed = 2f, float maxHealth = 100f, float attackPower = 15f, float attackRange = 1f, float attackCooldown = 1f)
        {
            return Create("Chaser", gridPos, gridManager);
        }

        /// <summary>
        /// 创建目标（兼容性方法，建议使用Create("Target", ...)）
        /// </summary>
        [Obsolete("建议使用 EntityFactory.Create(\"Target\", ...) 代替")]
        public static EntityEntity CreateTarget(Vector2Int gridPos, GridManager gridManager, float runSpeed = 2f, float maxHealth = 80f, float attackPower = 5f, float attackRange = 1f, float attackCooldown = 1f)
        {
            return Create("Target", gridPos, gridManager);
        }

        /// <summary>
        /// 创建静态物体（石头、树等）
        /// </summary>
        public static EntityEntity CreateStaticEntity(string name, Vector2Int gridPos, GridManager gridManager, Color color)
        {
            Vector3 worldPos = gridManager.GridToWorld(gridPos);

            return EntityBuilder.CreateEntity(name)
                .AddTransform(gridPos, worldPos, EntityPriority.Static)
                .AddView(color)
                .Build();
        }
    }
}
