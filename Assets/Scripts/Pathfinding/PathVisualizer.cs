using System.Collections.Generic;
using Map;
using UnityEngine;

namespace Pathfinding
{
    /// <summary>
    /// 路径可视化器
    /// </summary>
    public class PathVisualizer : MonoBehaviour
    {
        [Header("可视化设置")]
        [SerializeField] private Material pathMaterial;
        [SerializeField] private Material startMaterial;
        [SerializeField] private Material endMaterial;
        [SerializeField] private float lineHeight = 0.1f;
        [SerializeField] private float lineWidth = 0.1f;

        private GridManager gridManager;
        private List<GameObject> pathVisuals = new List<GameObject>();

        private void Awake()
        {
            gridManager = FindObjectOfType<GridManager>();
            
            // 如果没有指定材质，创建默认材质
            if (pathMaterial == null)
            {
                pathMaterial = new Material(Shader.Find("Standard"));
                pathMaterial.color = Color.yellow;
            }
            
            if (startMaterial == null)
            {
                startMaterial = new Material(Shader.Find("Standard"));
                startMaterial.color = Color.green;
            }
            
            if (endMaterial == null)
            {
                endMaterial = new Material(Shader.Find("Standard"));
                endMaterial.color = Color.red;
            }
        }

        /// <summary>
        /// 显示路径
        /// </summary>
        public void ShowPath(List<Vector2Int> path, Vector2Int start, Vector2Int end)
        {
            ClearPath();

            if (path == null || path.Count == 0)
            {
                Debug.LogWarning("路径为空，无法显示");
                return;
            }

            if (gridManager == null)
            {
                Debug.LogError("GridManager未找到");
                return;
            }

            // 显示起点
            CreatePathMarker(gridManager.GridToWorld(start), startMaterial, "Start");

            // 显示路径线段
            for (int i = 0; i < path.Count - 1; i++)
            {
                Vector3 startPos = gridManager.GridToWorld(path[i]);
                Vector3 endPos = gridManager.GridToWorld(path[i + 1]);
                startPos.y = lineHeight;
                endPos.y = lineHeight;

                CreatePathLine(startPos, endPos, pathMaterial);
            }

            // 显示终点
            CreatePathMarker(gridManager.GridToWorld(end), endMaterial, "End");
        }

        /// <summary>
        /// 创建路径线段
        /// </summary>
        private void CreatePathLine(Vector3 start, Vector3 end, Material material)
        {
            GameObject lineObj = new GameObject("PathLine");
            lineObj.transform.SetParent(transform);
            
            LineRenderer lineRenderer = lineObj.AddComponent<LineRenderer>();
            lineRenderer.material = material;
            lineRenderer.startWidth = lineWidth;
            lineRenderer.endWidth = lineWidth;
            lineRenderer.positionCount = 2;
            lineRenderer.useWorldSpace = true;
            lineRenderer.startColor = Color.yellow;
            lineRenderer.endColor = Color.yellow;
            
            lineRenderer.SetPosition(0, start);
            lineRenderer.SetPosition(1, end);

            pathVisuals.Add(lineObj);
        }

        /// <summary>
        /// 创建路径标记（起点/终点）
        /// </summary>
        private void CreatePathMarker(Vector3 position, Material material, string name)
        {
            GameObject marker = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            marker.name = name;
            marker.transform.SetParent(transform);
            marker.transform.position = position + Vector3.up * 0.5f;
            marker.transform.localScale = new Vector3(0.3f, 0.5f, 0.3f);
            
            Renderer renderer = marker.GetComponent<Renderer>();
            renderer.material = material;

            pathVisuals.Add(marker);
        }

        /// <summary>
        /// 清除路径显示
        /// </summary>
        public void ClearPath()
        {
            foreach (var visual in pathVisuals)
            {
                if (visual != null)
                {
                    Destroy(visual);
                }
            }
            pathVisuals.Clear();
        }

        private void OnDestroy()
        {
            ClearPath();
        }
    }
}

