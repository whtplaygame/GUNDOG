namespace Pathfinding.Entity.BehaviorTree.Nodes
{
    /// <summary>
    /// 空闲节点
    /// </summary>
    public class IdleNode : IBehaviorNode
    {
        public NodeStatus Execute(Entity owner)
        {
            // 空闲状态，什么都不做
            return NodeStatus.Success;
        }
    }
}
