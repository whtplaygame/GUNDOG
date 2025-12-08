namespace Pathfinding.Entity.BehaviorTree
{
    /// <summary>
    /// 行为节点执行结果
    /// </summary>
    public enum NodeStatus
    {
        Success,   // 成功
        Failure,   // 失败
        Running    // 运行中
    }

    /// <summary>
    /// 行为节点接口（符合迪米特法则）
    /// </summary>
    public interface IBehaviorNode
    {
        /// <summary>
        /// 执行节点
        /// </summary>
        NodeStatus Execute(Entity owner);
    }
}

