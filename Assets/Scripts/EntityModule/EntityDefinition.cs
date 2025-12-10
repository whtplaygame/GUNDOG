using System;
using EntityModule.BehaviorTree;
using EntityEntity = EntityModule.Entity;

namespace EntityModule
{
    /// <summary>
    /// 实体定义：描述一个实体该如何被构建（符合开闭原则）
    /// </summary>
    public class EntityDefinition
    {
        public string EntityTypeID { get; private set; }
        
        /// <summary>
        /// 组件配置委托：负责往Builder里塞组件
        /// </summary>
        public Action<EntityBuilderHelper> ConfigureComponents { get; set; }

        /// <summary>
        /// 行为树构建委托：负责返回根节点
        /// </summary>
        public Func<EntityEntity, IBehaviorNode> BuildBehaviorTree { get; set; }

        public EntityDefinition(string id)
        {
            EntityTypeID = id;
        }
    }
}

