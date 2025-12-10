using System.Collections.Generic;
using System.Linq;
using Map;
using UnityEngine;

namespace Pathfinding
{
    /// <summary>
    /// A*寻路算法实现
    /// </summary>
    public class AStarPathfinding
    {
        private Dictionary<Vector2Int, PathfindingNode> nodeMap;
        private int mapWidth;
        private int mapHeight;

        public AStarPathfinding(int width, int height)
        {
            mapWidth = width;
            mapHeight = height;
            nodeMap = new Dictionary<Vector2Int, PathfindingNode>();
        }

        /// <summary>
        /// 设置节点地图
        /// </summary>
        public void SetNodeMap(Dictionary<Vector2Int, Tile> tileMap)
        {
            nodeMap.Clear();
            foreach (var kvp in tileMap)
            {
                nodeMap[kvp.Key] = new PathfindingNode(kvp.Key, kvp.Value);
            }
        }

        /// <summary>
        /// 执行A*寻路算法
        /// </summary>
        /// <param name="startPos">起点</param>
        /// <param name="endPos">终点</param>
        /// <returns>路径节点列表，如果找不到路径则返回null</returns>
        public List<Vector2Int> FindPath(Vector2Int startPos, Vector2Int endPos)
        {
            // 重置所有节点
            foreach (var node in nodeMap.Values)
            {
                node.Reset();
            }

            // 检查起点和终点是否有效
            if (!nodeMap.ContainsKey(startPos) || !nodeMap.ContainsKey(endPos))
            {
                Debug.LogWarning($"起点或终点不在地图范围内: Start={startPos}, End={endPos}");
                return null;
            }

            var startNode = nodeMap[startPos];
            var endNode = nodeMap[endPos];

            // 检查起点和终点是否可通行
            if (!startNode.Tile.IsWalkable() || !endNode.Tile.IsWalkable())
            {
                Debug.LogWarning("起点或终点不可通行");
                return null;
            }

            // 开放列表和关闭列表
            var openSet = new List<PathfindingNode> { startNode };
            var closedSet = new HashSet<Vector2Int>();

            // 初始化起点
            startNode.GCost = 0f;
            startNode.HCost = GetHeuristicCost(startPos, endPos);

            while (openSet.Count > 0)
            {
                // 从开放列表中选择F值最小的节点
                var currentNode = openSet.OrderBy(n => n.FCost).ThenBy(n => n.HCost).First();
                openSet.Remove(currentNode);
                closedSet.Add(currentNode.GridPosition);

                // 如果到达终点，回溯路径
                if (currentNode.GridPosition == endPos)
                {
                    return RetracePath(startNode, endNode);
                }

                // 检查相邻节点
                var neighbors = GetNeighbors(currentNode.GridPosition);
                foreach (var neighborPos in neighbors)
                {
                    if (!nodeMap.ContainsKey(neighborPos))
                        continue;

                    var neighborNode = nodeMap[neighborPos];

                    // 跳过不可通行的节点或已在关闭列表中的节点
                    if (!neighborNode.Tile.IsWalkable() || closedSet.Contains(neighborPos))
                        continue;

                    // 计算从起点到相邻节点的代价
                    float movementCost = currentNode.GCost + GetMovementCost(currentNode, neighborNode);

                    // 如果新路径更优，或者相邻节点不在开放列表中
                    if (movementCost < neighborNode.GCost || !openSet.Contains(neighborNode))
                    {
                        neighborNode.GCost = movementCost;
                        neighborNode.HCost = GetHeuristicCost(neighborPos, endPos);
                        neighborNode.Parent = currentNode;

                        if (!openSet.Contains(neighborNode))
                        {
                            openSet.Add(neighborNode);
                        }
                    }
                }
            }

            // 找不到路径
            Debug.LogWarning($"无法找到从 {startPos} 到 {endPos} 的路径");
            return null;
        }

        /// <summary>
        /// 获取相邻节点（8方向）
        /// </summary>
        private List<Vector2Int> GetNeighbors(Vector2Int pos)
        {
            var neighbors = new List<Vector2Int>();
            
            // 8方向移动
            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    if (x == 0 && y == 0)
                        continue;

                    var neighborPos = new Vector2Int(pos.x + x, pos.y + y);
                    
                    // 检查是否在地图范围内
                    if (neighborPos.x >= 0 && neighborPos.x < mapWidth &&
                        neighborPos.y >= 0 && neighborPos.y < mapHeight)
                    {
                        neighbors.Add(neighborPos);
                    }
                }
            }

            return neighbors;
        }

        /// <summary>
        /// 计算启发式代价（使用欧几里得距离）
        /// </summary>
        private float GetHeuristicCost(Vector2Int from, Vector2Int to)
        {
            float dx = to.x - from.x;
            float dy = to.y - from.y;
            return Mathf.Sqrt(dx * dx + dy * dy);
        }

        /// <summary>
        /// 计算移动代价（考虑地形和buff）
        /// </summary>
        private float GetMovementCost(PathfindingNode from, PathfindingNode to)
        {
            // 基础移动代价（距离）
            float distance = GetHeuristicCost(from.GridPosition, to.GridPosition);
            
            // 目标格子的移动代价（时间）
            float tileCost = to.Tile.GetMovementCost();
            
            // 总代价 = 距离 * 时间代价
            return distance * tileCost;
        }

        /// <summary>
        /// 回溯路径
        /// </summary>
        private List<Vector2Int> RetracePath(PathfindingNode startNode, PathfindingNode endNode)
        {
            var path = new List<Vector2Int>();
            var currentNode = endNode;

            while (currentNode != null)
            {
                path.Add(currentNode.GridPosition);
                currentNode = currentNode.Parent;
            }

            path.Reverse();
            return path;
        }
    }
}

