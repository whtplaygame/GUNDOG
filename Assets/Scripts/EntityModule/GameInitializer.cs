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
            // RegisterTarget();
            RegisterArcher();
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
                       .AddLocomotor(runSpeed: 4f)
                       .AddCombat(maxHealth: 100f, attackPower: 15f, attackRange: 1f, attackCooldown: 3f)
                       .AddView(Color.red)
                       .AddAnimation() // 添加动画组件，会自动从GameObject获取Animator
                       // CombatViewComponent已在AddCombat中自动添加
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
                // 关键修正：确保 CheckCooldownNode 不会卡住行为树
                // 如果在硬直中，CheckCooldownNode 应该返回 Failure，从而跳出序列，去执行 Idle 或其他
                var attackSequence = new SequenceNode(new List<IBehaviorNode>
                {
                    checkIsAliveNode,
                    hasValidTargetNode,
                    checkDistanceNode,
                    checkCooldownNode, // 如果在硬直，返回Failure
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
                       .AddCombat(maxHealth: 800f, attackPower: 5f, attackRange: 1f, attackCooldown: 1f)
                       .AddView(Color.blue)
                       .AddAnimation() // 添加动画组件，会自动从GameObject获取Animator
                       // CombatViewComponent已在AddCombat中自动添加
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

                // 创建逃跑位置提供函数（动态更新，每0.1秒重新计算）
                System.Func<Entity, Vector2Int?> fleePositionProvider = (owner) =>
                {
                    calculateFleePositionNode.Execute(owner);
                    return calculateFleePositionNode.GetFleePosition();
                };
                var moveToFleePositionNode = new DynamicMoveToPositionNode(fleePositionProvider, updateInterval: 0.1f);

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

        /// <summary>
        /// 注册Archer实体定义
        /// </summary>
        private static void RegisterArcher()
        {
            var archerDef = new EntityDefinition("Archer");

            // 1. 配置组件
            // 射手：远程攻击，攻击距离长，但危险距离内会逃跑
            archerDef.ConfigureComponents = (builder) =>
            {
                builder.AddData(EntityType.Archer, 15f) // 探测范围15格
                       .AddLocomotor(runSpeed: 3f) // 移动速度中等
                       .AddCombat(
                           maxHealth: 80f,      // 血量较低
                           attackPower: 20f,    // 攻击力高
                           attackRange: 5f,     // 攻击距离5格（远程）
                           attackCooldown: 2f   // 攻击冷却2秒
                       )
                       .AddView(UnityEngine.Color.green) // 绿色显示
                       .AddAnimation() // 添加动画组件
                       .Entity.AddComponent<BehaviorTreeComponent>();
            };

            // 2. 配置行为树
            archerDef.BuildBehaviorTree = (Entity entity) =>
            {
                // 射手行为逻辑：
                // 优先级：逃跑 > 攻击 > 寻找敌人 > 待机
                // 危险距离：2格（当敌人靠近到2格内时逃跑）
                // 攻击距离：5格
                // 攻击和被攻击时无法立刻执行逃跑行为（由CombatComponent.CanMove控制）

                float dangerRange = 2f;  // 危险距离b
                float fleeDistance = 4f; // 逃跑距离

                // 构建原子节点
                var checkIsAliveNode = new CheckIsAliveNode();
                var hasValidTargetNode = new HasValidTargetNode();
                var checkDangerZoneNode = new CheckDangerZoneNode(dangerRange);
                var fleeFromTargetNode = new FleeFromTargetNode(fleeDistance);
                var checkAttackRangeNode = new CheckDistanceNode("AttackRange"); // 攻击距离a
                var checkCooldownNode = new CheckCooldownNode(waitForCooldown: true);
                var performAttackNode = new PerformAttackNode();
                var findEnemyNode = new FindEnemyNode();
                var idleNode = new IdleNode();

                // 逃跑序列（最高优先级）
                // 条件：存活 + 有目标 + 目标在危险区域内
                var fleeSequence = new SequenceNode(new List<IBehaviorNode>
                {
                    checkIsAliveNode,
                    hasValidTargetNode,
                    checkDangerZoneNode,  // 检查目标是否进入危险区域
                    fleeFromTargetNode    // 执行逃跑
                });

                // 攻击序列（次优先级）
                // 条件：存活 + 有目标 + 在攻击范围内 + CD冷却完毕
                var attackSequence = new SequenceNode(new List<IBehaviorNode>
                {
                    checkIsAliveNode,
                    hasValidTargetNode,
                    checkAttackRangeNode,  // 检查是否在攻击范围内
                    checkCooldownNode,
                    performAttackNode
                });

                // 返回根选择器
                // 优先级：逃跑 > 攻击 > 寻找敌人 > 待机
                return new SelectorNode(new List<IBehaviorNode>
                {
                    fleeSequence,    // 1. 如果敌人太近，逃跑
                    attackSequence,  // 2. 如果敌人在攻击范围内，攻击
                    findEnemyNode,   // 3. 如果没有目标，寻找敌人
                    idleNode         // 4. 否则待机
                });
            };

            // 注册到工厂
            EntityFactory.Register(archerDef);
        }
    }
}

