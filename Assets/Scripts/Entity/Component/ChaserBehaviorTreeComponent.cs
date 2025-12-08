using System.Collections.Generic;
using Pathfinding.Entity.BehaviorTree;
using Pathfinding.Entity.BehaviorTree.Nodes;
using Pathfinding.Entity.Component;
using BT = Pathfinding.Entity.BehaviorTree;

namespace Pathfinding.Entity.Component
{
    /// <summary>
    /// 追逐者行为树组件（符合开闭原则）
    /// </summary>
    public class ChaserBehaviorTreeComponent : BehaviorTreeComponent
    {
        protected override void BuildBehaviorTree()
        {
            var findTargetNode = new FindTargetNode();
            var checkTargetExistsNode = new CheckTargetExistsNode();
            var chaseNode = new ChaseNode();
            var idleNode = new IdleNode();

            // 追逐序列：检查目标存在 AND 追逐
            var chaseSequence = new SequenceNode(new List<IBehaviorNode>
            {
                checkTargetExistsNode,
                chaseNode
            });

            // 主选择器：追逐序列 OR 查找目标 OR 空闲
            var mainSelector = new SelectorNode(new List<IBehaviorNode>
            {
                chaseSequence,
                findTargetNode,
                idleNode
            });

            SetBehaviorTree(new BT.BehaviorTree(Owner, mainSelector));
        }
    }
}

