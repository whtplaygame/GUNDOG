using EntityModule.BehaviorTree;
using EntityModule.BehaviorTree.Nodes.Action;
using EntityModule.BehaviorTree.Nodes.Composite;

namespace EntityModule.Component
{
    /// <summary>
    /// 目标行为树组件（符合开闭原则）
    /// </summary>
    public class TargetBehaviorTreeComponent : BehaviorTreeComponent
    {
        protected override void BuildBehaviorTree()
        {
            var randomMoveNode = new RandomMoveNode();
            var idleNode = new IdleNode();

            // 主选择器：随机移动 OR 空闲
            var mainSelector = new SelectorNode(new System.Collections.Generic.List<IBehaviorNode>
            {
                randomMoveNode,
                idleNode
            });

            SetBehaviorTree(new BehaviorTree.BehaviorTree((Entity)Owner, mainSelector));
        }
    }
}

