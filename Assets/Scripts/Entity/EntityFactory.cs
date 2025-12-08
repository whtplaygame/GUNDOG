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
        public static Entity CreateChaser(Vector2Int gridPos, GridManager gridManager, float runSpeed = 2f, float maxHealth = 100f, float attackPower = 15f, float attackRange = 1f, float attackCooldown = 1f)
        {
            Vector3 worldPos = gridManager.GridToWorld(gridPos);

            // 构建行为树节点
            var findTargetNode = new FindTargetNode();
            var checkTargetExistsNode = new CheckTargetExistsNode();
            var checkTargetInAttackRangeNode = new CheckTargetInAttackRangeNode();
            var attackNode = new AttackNode();
            var chaseNode = new ChaseNode();
            var idleNode = new IdleNode();

            // 攻击序列：目标存在 AND 在攻击范围内 AND 攻击
            var attackSequence = new SequenceNode(new List<IBehaviorNode>
            {
                checkTargetExistsNode,
                checkTargetInAttackRangeNode,
                attackNode
            });

            // 追逐序列：目标存在 AND 不在攻击范围内 AND 追逐
            var chaseSequence = new SequenceNode(new List<IBehaviorNode>
            {
                checkTargetExistsNode,
                chaseNode
            });

            // 主选择器：攻击 OR 追逐 OR 查找目标 OR 空闲
            var mainSelector = new SelectorNode(new List<IBehaviorNode>
            {
                attackSequence,
                chaseSequence,
                findTargetNode,
                idleNode
            });

            // 使用Builder创建
            var entity = EntityBuilder.CreateEntity("Chaser")
                .AddTransform(gridPos, worldPos, EntityPriority.Active)
                .AddData(EntityType.Chaser, 10f)
                .AddLocomotor(runSpeed)
                .AddCombat(maxHealth, attackPower, attackRange, attackCooldown)
                .AddView(Color.red)
                .SetBrain(mainSelector)
                .Build();
            
            return entity;
        }

        /// <summary>
        /// 创建目标
        /// </summary>
        public static Entity CreateTarget(Vector2Int gridPos, GridManager gridManager, float runSpeed = 2f, float maxHealth = 80f, float attackPower = 5f, float attackRange = 1f, float attackCooldown = 1f)
        {
            Vector3 worldPos = gridManager.GridToWorld(gridPos);

            // 构建行为树节点
            var checkHealthBelowHalfNode = new CheckHealthBelowHalfNode();
            var findChaserNode = new FindChaserNode();
            var checkChaserExistsNode = new CheckTargetExistsNode();
            var checkTargetInAttackRangeNode = new CheckTargetInAttackRangeNode();
            var attackNode = new AttackNode();
            var chaseNode = new ChaseNode();
            var fleeNode = new FleeNode();
            var randomMoveNode = new RandomMoveNode();
            var idleNode = new IdleNode();

            // 战斗序列（血量低于一半时）：目标存在 AND 在攻击范围内 AND 攻击
            var attackSequence = new SequenceNode(new List<IBehaviorNode>
            {
                checkChaserExistsNode,
                checkTargetInAttackRangeNode,
                attackNode
            });

            // 追逐序列（血量低于一半时）：目标存在 AND 不在攻击范围内 AND 追逐
            var chaseSequence = new SequenceNode(new List<IBehaviorNode>
            {
                checkChaserExistsNode,
                chaseNode
            });

            // 战斗行为（血量低于一半时）：攻击 OR 追逐 OR 查找目标
            var combatBehavior = new SelectorNode(new List<IBehaviorNode>
            {
                attackSequence,
                chaseSequence,
                findChaserNode
            });

            // 战斗条件序列：血量低于一半 AND 战斗行为
            var combatConditionSequence = new SequenceNode(new List<IBehaviorNode>
            {
                checkHealthBelowHalfNode,
                combatBehavior
            });

            // 逃跑序列：检测到Chaser AND 逃跑
            var fleeSequence = new SequenceNode(new List<IBehaviorNode>
            {
                checkChaserExistsNode,
                fleeNode
            });

            // 检测Chaser序列：查找Chaser OR 逃跑序列
            var detectChaserSequence = new SequenceNode(new List<IBehaviorNode>
            {
                findChaserNode,
                fleeSequence
            });

            // 逃跑行为（血量高于一半时）：检测并逃跑 OR 逃跑
            var fleeBehavior = new SelectorNode(new List<IBehaviorNode>
            {
                detectChaserSequence,
                fleeSequence
            });

            // 主选择器：战斗行为（血量低于一半） OR 逃跑行为（血量高于一半） OR 随机移动 OR 空闲
            var mainSelector = new SelectorNode(new List<IBehaviorNode>
            {
                combatConditionSequence,
                fleeBehavior,
                randomMoveNode,
                idleNode
            });

            // 使用Builder创建
            return EntityBuilder.CreateEntity("Target")
                .AddTransform(gridPos, worldPos, EntityPriority.Active)
                .AddData(EntityType.Target, 10f)
                .AddLocomotor(runSpeed)
                .AddCombat(maxHealth, attackPower, attackRange, attackCooldown)
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

