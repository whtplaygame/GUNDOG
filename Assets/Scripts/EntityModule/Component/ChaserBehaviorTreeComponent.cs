using System.Collections.Generic;
using EntityModule.BehaviorTree;
using EntityModule.BehaviorTree.Nodes.Action;
using EntityModule.BehaviorTree.Nodes.Composite;
using EntityModule.BehaviorTree.Nodes.Condition;

namespace EntityModule.Component
{
    /// <summary>
    /// 追逐者行为树组件（符合开闭原则）
    /// </summary>
    public class ChaserBehaviorTreeComponent : BehaviorTreeComponent
    {
        protected override void BuildBehaviorTree()
        {
            var findTargetNode = new FindTargetNode();
            var hasValidTargetNode = new HasValidTargetNode();
            var moveToTargetNode = new MoveToTargetNode();
            var idleNode = new IdleNode();

            // 追逐序列：有合法目标 AND 移动到目标
            var chaseSequence = new SequenceNode(new List<IBehaviorNode>
            {
                hasValidTargetNode,
                moveToTargetNode
            });

            // 主选择器：追逐序列 OR 查找目标 OR 空闲
            var mainSelector = new SelectorNode(new List<IBehaviorNode>
            {
                chaseSequence,
                findTargetNode,
                idleNode
            });

            SetBehaviorTree(new BehaviorTree.BehaviorTree((global::EntityModule.Entity)Owner, mainSelector));
        }
    }
}

