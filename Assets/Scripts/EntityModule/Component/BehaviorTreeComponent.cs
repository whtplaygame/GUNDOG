using EntityModule.BehaviorTree;
using EntityModule.BehaviorTree.Nodes.Action;

namespace EntityModule.Component
{
    /// <summary>
    /// 行为树组件（单一职责原则：只负责AI行为）
    /// </summary>
    public class BehaviorTreeComponent : Component
    {
        private BehaviorTree.BehaviorTree behaviorTree;

        public BehaviorTree.BehaviorTree BehaviorTree => behaviorTree;

        public override void Initialize()
        {
            base.Initialize();
            
            if (Owner == null) return;

            // 如果行为树还没有被设置，才构建默认行为树
            // 这样可以避免覆盖通过SetBehaviorTree设置的行为树
            if (behaviorTree == null)
            {
                BuildBehaviorTree();
            }
        }

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);
            
            // 更新行为树
            behaviorTree?.Update();
        }

        /// <summary>
        /// 构建行为树（符合开闭原则，子类可重写）
        /// </summary>
        protected virtual void BuildBehaviorTree()
        {
            // 默认行为树为空
            var idleNode = new IdleNode();
            behaviorTree = new BehaviorTree.BehaviorTree((global::EntityModule.Entity)Owner, idleNode);
        }

        /// <summary>
        /// 设置行为树
        /// </summary>
        public void SetBehaviorTree(BehaviorTree.BehaviorTree tree)
        {
            behaviorTree = tree;
        }

        /// <summary>
        /// 获取根节点（用于Mod扩展）
        /// </summary>
        public IBehaviorNode GetRootNode()
        {
            return behaviorTree?.GetRootNode();
        }

        /// <summary>
        /// 设置根节点（用于Mod扩展）
        /// </summary>
        public void SetRootNode(IBehaviorNode node)
        {
            if (behaviorTree != null)
            {
                behaviorTree.SetRootNode(node);
            }
        }
    }
}

