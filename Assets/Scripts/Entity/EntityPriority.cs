namespace Pathfinding.Entity
{
    /// <summary>
    /// 实体优先级（活跃度分类）
    /// 用于分桶策略
    /// </summary>
    public enum EntityPriority
    {
        Static = 0,      // 静态物体（石头、草等）- 桶1
        Active = 1       // 活跃物体（怪物、玩家等）- 桶2
    }
}

