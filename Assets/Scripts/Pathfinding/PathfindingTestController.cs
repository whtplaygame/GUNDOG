using UnityEngine;
using System.Collections.Generic;
using Pathfinding.Map;

namespace Pathfinding
{
    /// <summary>
    /// 寻路测试控制器
    /// </summary>
    public class PathfindingTestController : MonoBehaviour
    {
        [Header("引用")]
        [SerializeField] private GridManager gridManager;
        [SerializeField] private PathVisualizer pathVisualizer;

        [Header("测试设置")]
        [SerializeField] private Vector2Int startPosition = new Vector2Int(0, 0);
        [SerializeField] private Vector2Int endPosition = new Vector2Int(19, 19);
        [SerializeField] private KeyCode findPathKey = KeyCode.Space;
        [SerializeField] private KeyCode clearPathKey = KeyCode.C;

        private List<Vector2Int> currentPath;

        private void Awake()
        {
            if (gridManager == null)
                gridManager = FindObjectOfType<GridManager>();
            
            if (pathVisualizer == null)
                pathVisualizer = FindObjectOfType<PathVisualizer>();
        }

        private void Start()
        {
            // 自动执行一次寻路
            FindPath();
        }

        private void Update()
        {
            if (Input.GetKeyDown(findPathKey))
            {
                FindPath();
            }

            if (Input.GetKeyDown(clearPathKey))
            {
                ClearPath();
            }

            // 鼠标点击寻路
            if (Input.GetMouseButtonDown(0))
            {
                HandleMouseClick();
            }
        }

        /// <summary>
        /// 执行寻路
        /// </summary>
        public void FindPath()
        {
            if (gridManager == null)
            {
                Debug.LogError("GridManager未找到");
                return;
            }

            // 确保坐标在地图范围内
            startPosition.x = Mathf.Clamp(startPosition.x, 0, gridManager.MapWidth - 1);
            startPosition.y = Mathf.Clamp(startPosition.y, 0, gridManager.MapHeight - 1);
            endPosition.x = Mathf.Clamp(endPosition.x, 0, gridManager.MapWidth - 1);
            endPosition.y = Mathf.Clamp(endPosition.y, 0, gridManager.MapHeight - 1);

            currentPath = gridManager.FindPath(startPosition, endPosition);

            if (currentPath != null && currentPath.Count > 0)
            {
                Debug.Log($"找到路径，共 {currentPath.Count} 个节点");
                
                // 计算总时间代价
                float totalCost = CalculatePathCost(currentPath);
                Debug.Log($"路径总时间代价: {totalCost:F2}");

                if (pathVisualizer != null)
                {
                    pathVisualizer.ShowPath(currentPath, startPosition, endPosition);
                }
            }
            else
            {
                Debug.LogWarning("未找到路径");
            }
        }

        /// <summary>
        /// 清除路径显示
        /// </summary>
        public void ClearPath()
        {
            currentPath = null;
            if (pathVisualizer != null)
            {
                pathVisualizer.ClearPath();
            }
        }

        /// <summary>
        /// 处理鼠标点击
        /// </summary>
        private void HandleMouseClick()
        {
            if (gridManager == null)
            {
                Debug.LogError("GridManager未找到");
                return;
            }

            Camera cam = Camera.main;
            if (cam == null)
            {
                Debug.LogError("主相机未找到");
                return;
            }

            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            
            // 方法1: 使用物理射线检测（检测碰撞体）
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
            {
                Vector2Int gridPos = gridManager.WorldToGrid(hit.point);
                
                // 确保坐标在地图范围内
                gridPos.x = Mathf.Clamp(gridPos.x, 0, gridManager.MapWidth - 1);
                gridPos.y = Mathf.Clamp(gridPos.y, 0, gridManager.MapHeight - 1);
                
                // 第一次点击设置起点，第二次点击设置终点并寻路
                if (startPosition == endPosition || currentPath == null)
                {
                    startPosition = gridPos;
                    Debug.Log($"设置起点: {startPosition}");
                }
                else
                {
                    endPosition = gridPos;
                    Debug.Log($"设置终点: {endPosition}");
                    FindPath();
                }
                return;
            }

            // 方法2: 如果物理射线检测失败，使用平面射线检测（Y=0平面）
            Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
            if (groundPlane.Raycast(ray, out float distance))
            {
                Vector3 hitPoint = ray.GetPoint(distance);
                Vector2Int gridPos = gridManager.WorldToGrid(hitPoint);
                
                // 确保坐标在地图范围内
                gridPos.x = Mathf.Clamp(gridPos.x, 0, gridManager.MapWidth - 1);
                gridPos.y = Mathf.Clamp(gridPos.y, 0, gridManager.MapHeight - 1);
                
                // 第一次点击设置起点，第二次点击设置终点并寻路
                if (startPosition == endPosition || currentPath == null)
                {
                    startPosition = gridPos;
                    Debug.Log($"设置起点: {startPosition}");
                }
                else
                {
                    endPosition = gridPos;
                    Debug.Log($"设置终点: {endPosition}");
                    FindPath();
                }
            }
            else
            {
                Debug.LogWarning("射线检测失败，无法设置位置");
            }
        }

        /// <summary>
        /// 计算路径的总时间代价
        /// </summary>
        private float CalculatePathCost(List<Vector2Int> path)
        {
            if (path == null || path.Count < 2)
                return 0f;

            float totalCost = 0f;
            for (int i = 1; i < path.Count; i++)
            {
                Tile tile = gridManager.GetTile(path[i]);
                if (tile != null)
                {
                    float distance = Vector2Int.Distance(path[i - 1], path[i]);
                    totalCost += distance * tile.GetMovementCost();
                }
            }

            return totalCost;
        }

        /// <summary>
        /// 设置起点
        /// </summary>
        public void SetStartPosition(Vector2Int pos)
        {
            startPosition = pos;
        }

        /// <summary>
        /// 设置终点
        /// </summary>
        public void SetEndPosition(Vector2Int pos)
        {
            endPosition = pos;
        }
    }
}

