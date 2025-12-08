using UnityEngine;
using Pathfinding.Entity.BehaviorTree;
using Pathfinding.Entity.BehaviorTree.Nodes;
using System.Collections.Generic;
using Pathfinding.Map;

namespace Pathfinding.Entity
{
    /// <summary>
    /// 实体工厂（提供预定义的Entity创建方法）
    /// </summary>
    public static class EntityFactory
    {
        /// <summary>
        /// 创建追逐者
        /// </summary>
        public static Entity CreateChaser(Vector2Int gridPos, GridManager gridManager, float runSpeed = 2f)
        {
            Vector3 worldPos = gridManager.GridToWorld(gridPos);

            // 构建行为树
            var findTargetNode = new FindTargetNode();
            var checkTargetExistsNode = new CheckTargetExistsNode();
            var chaseNode = new ChaseNode();
            var idleNode = new IdleNode();

            var chaseSequence = new SequenceNode(new List<IBehaviorNode>
            {
                checkTargetExistsNode,
                chaseNode
            });

            var mainSelector = new SelectorNode(new List<IBehaviorNode>
            {
                chaseSequence,
                findTargetNode,
                idleNode
            });

            // 使用Builder创建
            return EntityBuilder.CreateEntity("Chaser")
                .AddTransform(gridPos, worldPos, EntityPriority.Active)
                .AddData(EntityType.Chaser, 10f)
                .AddLocomotor(runSpeed)
                .AddView(Color.red)
                .SetBrain(mainSelector)
                .Build();
        }

        /// <summary>
        /// 创建目标
        /// </summary>
        public static Entity CreateTarget(Vector2Int gridPos, GridManager gridManager, float runSpeed = 2f)
        {
            Vector3 worldPos = gridManager.GridToWorld(gridPos);

            // 构建行为树
            var randomMoveNode = new RandomMoveNode();
            var idleNode = new IdleNode();

            var mainSelector = new SelectorNode(new List<IBehaviorNode>
            {
                randomMoveNode,
                idleNode
            });

            // 使用Builder创建
            return EntityBuilder.CreateEntity("Target")
                .AddTransform(gridPos, worldPos, EntityPriority.Active)
                .AddData(EntityType.Target, 10f)
                .AddLocomotor(runSpeed)
                .AddView(Color.blue)
                .SetBrain(mainSelector)
                .Build();
        }

        /// <summary>
        /// 创建静态物体（石头、树等）
        /// </summary>
        public static Entity CreateStaticEntity(string name, Vector2Int gridPos, GridManager gridManager, Color color)
        {
            Vector3 worldPos = gridManager.GridToWorld(gridPos);

            return EntityBuilder.CreateEntity(name)
                .AddTransform(gridPos, worldPos, EntityPriority.Static)
                .AddView(color)
                .Build();
        }
    }
}

