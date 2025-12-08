using System.Collections.Generic;

namespace Pathfinding.Entity.BehaviorTree
{
    /// <summary>
    /// 行为树类（符合迪米特法则）
    /// </summary>
    public class BehaviorTree
    {
        private IBehaviorNode rootNode;
        private Entity owner;

        public BehaviorTree(Entity owner, IBehaviorNode rootNode)
        {
            this.owner = owner;
            this.rootNode = rootNode;
        }

        /// <summary>
        /// 更新行为树
        /// </summary>
        public void Update()
        {
            rootNode?.Execute(owner);
        }

        /// <summary>
        /// 设置根节点
        /// </summary>
        public void SetRootNode(IBehaviorNode node)
        {
            rootNode = node;
        }
    }
}

