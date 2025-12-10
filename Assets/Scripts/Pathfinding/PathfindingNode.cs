using System.Collections.Generic;
using Map;
using UnityEngine;

namespace Pathfinding
{
    /// <summary>
    /// A*算法使用的节点类
    /// </summary>
    public class PathfindingNode
    {
        public Vector2Int GridPosition { get; set; }
        public Tile Tile { get; set; }
        
        // A*算法相关
        public float GCost { get; set; }  // 从起点到当前节点的实际代价
        public float HCost { get; set; }  // 从当前节点到终点的启发式代价
        public float FCost => GCost + HCost;  // 总代价
        
        public PathfindingNode Parent { get; set; }  // 用于回溯路径

        public PathfindingNode(Vector2Int position, Tile tile)
        {
            GridPosition = position;
            Tile = tile;
            GCost = 0f;
            HCost = 0f;
            Parent = null;
        }

        /// <summary>
        /// 重置节点数据
        /// </summary>
        public void Reset()
        {
            GCost = 0f;
            HCost = 0f;
            Parent = null;
        }
    }
}

