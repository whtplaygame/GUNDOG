using Pathfinding.Entity.BehaviorTree;
using Pathfinding.Entity.Component;
using BT = Pathfinding.Entity.BehaviorTree;

namespace Pathfinding.Entity.Component
{
    /// <summary>
    /// 行为树组件（单一职责原则：只负责AI行为）
    /// </summary>
    public class BehaviorTreeComponent : Component
    {
        private BT.BehaviorTree behaviorTree;

        public BT.BehaviorTree BehaviorTree => behaviorTree;

        public override void Initialize()
        {
            base.Initialize();
            
            if (Owner == null) return;

            // 构建行为树（由子类或外部设置）
            BuildBehaviorTree();
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
            var idleNode = new BT.Nodes.Action.IdleNode();
            behaviorTree = new BT.BehaviorTree(Owner, idleNode);
        }

        /// <summary>
        /// 设置行为树
        /// </summary>
        public void SetBehaviorTree(BT.BehaviorTree tree)
        {
            behaviorTree = tree;
        }
    }
}

