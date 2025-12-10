using UnityEngine;

namespace Map
{
    /// <summary>
    /// 地形类型枚举
    /// </summary>
    public enum TerrainType
    {
        Normal = 0,     // 普通地形，移动速度 1.0
        Grass = 1,      // 草地，移动速度 0.8
        Mud = 2,        // 泥地，移动速度 0.5
        Water = 3,      // 水域，移动速度 0.3
        Road = 4        // 道路，移动速度 1.2
    }

    /// <summary>
    /// Buff类型枚举
    /// </summary>
    public enum BuffType
    {
        None = 0,       // 无buff
        SpeedUp = 1,    // 加速buff，速度 +0.3
        SlowDown = 2,   // 减速buff，速度 -0.2
        Blocked = 3     // 阻挡，无法通过
    }

    /// <summary>
    /// 地图格子类，包含地形属性和buff属性
    /// </summary>
    [System.Serializable]
    public class Tile
    {
        [SerializeField] private TerrainType terrainType = TerrainType.Normal;
        [SerializeField] private BuffType buffType = BuffType.None;
        [SerializeField] private Vector2Int gridPosition;

        public TerrainType TerrainType => terrainType;
        public BuffType BuffType => buffType;
        public Vector2Int GridPosition => gridPosition;

        public Tile(Vector2Int position, TerrainType terrain = TerrainType.Normal, BuffType buff = BuffType.None)
        {
            gridPosition = position;
            terrainType = terrain;
            buffType = buff;
        }

        /// <summary>
        /// 获取该格子的移动速度倍数
        /// </summary>
        public float GetMovementSpeed()
        {
            // 如果被阻挡，返回0
            if (buffType == BuffType.Blocked)
                return 0f;

            // 基础地形速度
            float baseSpeed = terrainType switch
            {
                TerrainType.Normal => 1.0f,
                TerrainType.Grass => 0.8f,
                TerrainType.Mud => 0.5f,
                TerrainType.Water => 0.3f,
                TerrainType.Road => 1.2f,
                _ => 1.0f
            };

            // Buff影响
            float buffModifier = buffType switch
            {
                BuffType.SpeedUp => 0.3f,
                BuffType.SlowDown => -0.2f,
                _ => 0f
            };

            return Mathf.Max(0f, baseSpeed + buffModifier);
        }

        /// <summary>
        /// 获取通过该格子所需的时间代价（速度的倒数）
        /// </summary>
        public float GetMovementCost()
        {
            float speed = GetMovementSpeed();
            if (speed <= 0f)
                return float.MaxValue; // 无法通过
            
            return 1f / speed; // 时间代价 = 1 / 速度
        }

        /// <summary>
        /// 设置地形类型
        /// </summary>
        public void SetTerrainType(TerrainType terrain)
        {
            terrainType = terrain;
        }

        /// <summary>
        /// 设置Buff类型
        /// </summary>
        public void SetBuffType(BuffType buff)
        {
            buffType = buff;
        }

        /// <summary>
        /// 检查是否可以通行
        /// </summary>
        public bool IsWalkable()
        {
            return buffType != BuffType.Blocked && GetMovementSpeed() > 0f;
        }
    }
}

