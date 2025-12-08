using System.Collections.Generic;

namespace Pathfinding.Entity.BehaviorTree.Nodes
{
    /// <summary>
    /// 选择器节点（OR逻辑，任一子节点成功即成功）
    /// </summary>
    public class SelectorNode : IBehaviorNode
    {
        private List<IBehaviorNode> children;

        public SelectorNode(List<IBehaviorNode> children)
        {
            this.children = children ?? new List<IBehaviorNode>();
        }

        public NodeStatus Execute(Entity owner)
        {
            foreach (var child in children)
            {
                NodeStatus status = child.Execute(owner);
                if (status == NodeStatus.Success || status == NodeStatus.Running)
                {
                    return status;
                }
            }
            return NodeStatus.Failure;
        }
    }
}

