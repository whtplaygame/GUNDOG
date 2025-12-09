namespace Pathfinding.Entity.BehaviorTree.Nodes.Action
{
    /// <summary>
    /// 空闲节点（什么都不做）
    /// </summary>
    public class IdleNode : IBehaviorNode
    {
        public NodeStatus Execute(Entity owner)
        {
            // 空闲节点总是成功
            return NodeStatus.Success;
        }
    }
}

