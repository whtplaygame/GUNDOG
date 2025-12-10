using System.Collections.Generic;
using EntityModule.BehaviorTree;
using EntityModule.BehaviorTree.Nodes.Action;
using EntityModule.BehaviorTree.Nodes.Composite;
using EntityModule.BehaviorTree.Nodes.Condition;
using EntityModule.Component;
using UnityEngine;

namespace EntityModule
{
    /// <summary>
    /// 游戏初始化器（注册基础实体定义）
    /// </summary>
    public static class GameInitializer
    {
        /// <summary>
        /// 注册基础实体（在游戏启动时调用）
        /// </summary>
        public static void RegisterBaseEntities()
        {
            RegisterChaser();
            RegisterTarget();
        }

        /// <summary>
        /// 注册Chaser实体定义
        /// </summary>
        private static void RegisterChaser()
        {
            var chaserDef = new EntityDefinition("Chaser");

            // 1. 配置组件
            // 注意：不再写死数值，数值应该从 Config 配置表读取
            // 为了演示方便，这里先写死，后续可以改为从Luban配置表读取
            chaserDef.ConfigureComponents = (builder) =>
            {
                builder.AddData(EntityType.Chaser, 10f)
                       .AddLocomotor(runSpeed: 2f)
                       .AddCombat(maxHealth: 100f, attackPower: 15f, attackRange: 1f, attackCooldown: 1f)
                       .AddView(Color.red)
                       // 必须确保加了行为树组件容器，但不在这里初始化树
                       .Entity.AddComponent<BehaviorTreeComponent>();
            };

            // 2. 配置行为树
            chaserDef.BuildBehaviorTree = (Entity entity) =>
            {
                // 构建原子节点
                var hasValidTargetNode = new HasValidTargetNode();
                var checkIsAliveNode = new CheckIsAliveNode();
                var checkDistanceNode = new CheckDistanceNode("AttackRange"); // 参数化
                var checkCooldownNode = new CheckCooldownNode(waitForCooldown: true);
                var performAttackNode = new PerformAttackNode();
                var moveToTargetNode = new MoveToTargetNode();
                var findTargetNode = new FindTargetNode();
                var idleNode = new IdleNode();

                // 攻击序列
                var attackSequence = new SequenceNode(new List<IBehaviorNode>
                {
                    checkIsAliveNode,
                    hasValidTargetNode,
                    checkDistanceNode,
                    checkCooldownNode,
                    performAttackNode
                });

                // 追逐序列
                var chaseSequence = new SequenceNode(new List<IBehaviorNode>
                {
                    checkIsAliveNode,
                    hasValidTargetNode,
                    moveToTargetNode
                });

                // 返回根选择器
                return new SelectorNode(new List<IBehaviorNode>
                {
                    attackSequence,
                    chaseSequence,
                    findTargetNode,
                    idleNode
                });
            };

            // 注册到工厂
            EntityFactory.Register(chaserDef);
        }

        /// <summary>
        /// 注册Target实体定义
        /// </summary>
        private static void RegisterTarget()
        {
            var targetDef = new EntityDefinition("Target");

            // 1. 配置组件
            targetDef.ConfigureComponents = (builder) =>
            {
                builder.AddData(EntityType.Target, 20f)
                       .AddLocomotor(runSpeed: 2f)
                       .AddCombat(maxHealth: 80f, attackPower: 5f, attackRange: 1f, attackCooldown: 1f)
                       .AddView(Color.blue)
                       .Entity.AddComponent<BehaviorTreeComponent>();
            };

            // 2. 配置行为树
            targetDef.BuildBehaviorTree = (Entity entity) =>
            {
                // 构建原子节点
                var checkHealthBelowHalfNode = new CheckHealthBelowHalfNode();
                var checkIsAliveNode = new CheckIsAliveNode();
                var hasValidTargetNode = new HasValidTargetNode();
                var checkDistanceNode = new CheckDistanceNode("AttackRange");
                var checkCooldownNode = new CheckCooldownNode(waitForCooldown: true);
                var performAttackNode = new PerformAttackNode();
                var moveToTargetNode = new MoveToTargetNode();
                var findChaserNode = new FindChaserNode();
                var calculateFleePositionNode = new CalculateFleePositionNode();
                var randomMoveNode = new RandomMoveNode();
                var idleNode = new IdleNode();

                // 创建逃跑位置提供函数
                System.Func<Entity, Vector2Int?> fleePositionProvider = (owner) =>
                {
                    calculateFleePositionNode.Execute(owner);
                    return calculateFleePositionNode.GetFleePosition();
                };
                var moveToFleePositionNode = new MoveToPositionNode(fleePositionProvider);

                // 战斗序列（血量低于一半时）
                var attackSequence = new SequenceNode(new List<IBehaviorNode>
                {
                    checkIsAliveNode,
                    hasValidTargetNode,
                    checkDistanceNode,
                    checkCooldownNode,
                    performAttackNode
                });

                // 追逐序列（血量低于一半时）
                var chaseSequence = new SequenceNode(new List<IBehaviorNode>
                {
                    checkIsAliveNode,
                    hasValidTargetNode,
                    moveToTargetNode
                });

                // 战斗行为（血量低于一半时）
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

                // 逃跑序列
                var fleeSequence = new SequenceNode(new List<IBehaviorNode>
                {
                    hasValidTargetNode,
                    calculateFleePositionNode,
                    moveToFleePositionNode
                });

                // 检测Chaser序列
                var detectChaserSequence = new SequenceNode(new List<IBehaviorNode>
                {
                    findChaserNode,
                    fleeSequence
                });

                // 逃跑行为（血量高于一半时）
                var fleeBehavior = new SelectorNode(new List<IBehaviorNode>
                {
                    detectChaserSequence,
                    fleeSequence
                });

                // 返回根选择器
                return new SelectorNode(new List<IBehaviorNode>
                {
                    combatConditionSequence,
                    fleeBehavior,
                    randomMoveNode,
                    idleNode
                });
            };

            // 注册到工厂
            EntityFactory.Register(targetDef);
        }
    }
}

