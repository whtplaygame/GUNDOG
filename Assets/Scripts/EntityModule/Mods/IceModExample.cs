using EntityModule.Component;
using UnityEngine;
using EntityEntity = EntityModule.Entity;

namespace EntityModule.Mods
{
    /// <summary>
    /// 冰霜Mod示例（演示如何使用生命周期钩子扩展Entity）
    /// 这个Mod会在所有Chaser创建时添加冰霜光环组件，并修改其AI行为
    /// </summary>
    public class IceModExample
    {
        /// <summary>
        /// 加载Mod（在游戏初始化后调用）
        /// </summary>
        public static void Load()
        {
            // 注册一个"后处理钩子"，专门针对 "Chaser"
            EntityFactory.RegisterPostBuildHook("Chaser", (EntityEntity entity) =>
            {
                // 场景 1：添加组件
                // 给实体动态挂载一个新组件（这里只是示例，实际需要创建IceAuraComponent）
                // var iceAura = new IceAuraComponent(radius: 3f);
                // entity.AddComponent(iceAura);

                // 场景 2：修改行为树
                ModifyChaserAI(entity);

                Debug.Log("Chaser 已被冰霜 Mod 改装！");
            });
        }

        /// <summary>
        /// 修改Chaser的AI行为
        /// </summary>
        private static void ModifyChaserAI(EntityEntity entity)
        {
            var btComp = entity.GetComponent<BehaviorTreeComponent>();
            if (btComp == null) return;

            // 获取原始的根节点
            var originalRoot = btComp.GetRootNode();
            if (originalRoot == null) return;

            // 创建一个新的节点：检查是否被冻结（这里只是示例，实际需要创建CheckFrozenNode）
            // var checkFrozenNode = new CheckFrozenNode();

            // Mod 策略：如果被冻结，就不能执行原始逻辑
            // 新逻辑：Selector( CheckFrozen(播放被冻动画), OriginalRoot )
            // 这里为了演示，我们创建一个简单的装饰器逻辑
            // 实际使用时，CheckFrozenNode会在被冻结时返回Success，否则返回Failure

            // 示例：创建一个新的根节点，包装原始逻辑
            // var newRoot = new SelectorNode(new List<IBehaviorNode>
            // {
            //     checkFrozenNode,  // 如果被冻结，Execute返回Success，不执行后面
            //     originalRoot      // 如果没冻结，执行原来的逻辑
            // });

            // 替换掉实体的大脑
            // btComp.SetRootNode(newRoot);

            // 注意：上面的代码被注释是因为CheckFrozenNode和IceAuraComponent还没有实现
            // 这里只是展示如何使用Mod系统
        }
    }
}

