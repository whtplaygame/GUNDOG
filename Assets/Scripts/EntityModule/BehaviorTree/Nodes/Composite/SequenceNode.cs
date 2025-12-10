using System.Collections.Generic;

namespace EntityModule.BehaviorTree.Nodes.Composite
{
    /// <summary>
    /// 序列节点（AND逻辑，所有子节点成功才成功）
    /// </summary>
    public class SequenceNode : IBehaviorNode
    {
        private List<IBehaviorNode> children;

        public SequenceNode(List<IBehaviorNode> children)
        {
            this.children = children ?? new List<IBehaviorNode>();
        }

        public NodeStatus Execute(global::EntityModule.Entity owner)
        {
            foreach (var child in children)
            {
                NodeStatus status = child.Execute(owner);
                if (status == NodeStatus.Failure)
                {
                    return NodeStatus.Failure;
                }
                if (status == NodeStatus.Running)
                {
                    return NodeStatus.Running;
                }
            }
            return NodeStatus.Success;
        }
    }
}

