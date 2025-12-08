namespace Pathfinding.Entity.LOD
{
    /// <summary>
    /// 实体LOD级别
    /// </summary>
    public enum EntityLODLevel
    {
        High = 0,      // 高细节：每帧更新（<20米）
        Medium = 1,    // 中细节：每5帧更新（20-50米）
        Low = 2        // 低细节：停止更新或极简模拟（>50米）
    }
}

