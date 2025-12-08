using UnityEngine;
using System.Collections.Generic;

namespace Pathfinding.Map
{
    /// <summary>
    /// 地图格子可视化器（可选，用于显示地形和buff）
    /// </summary>
    public class TileVisualizer : MonoBehaviour
    {
        [Header("可视化设置")]
        [SerializeField] private bool showTerrainColors = true;
        [SerializeField] private bool showBuffIcons = false;
        [SerializeField] private float tileHeight = 0.05f;

        [Header("地形颜色")]
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color grassColor = Color.green;
        [SerializeField] private Color mudColor = new Color(0.6f, 0.4f, 0.2f);
        [SerializeField] private Color waterColor = Color.blue;
        [SerializeField] private Color roadColor = new Color(0.5f, 0.5f, 0.5f);

        private GridManager gridManager;
        private Dictionary<Vector2Int, GameObject> tileObjects = new Dictionary<Vector2Int, GameObject>();

        private void Awake()
        {
            gridManager = FindObjectOfType<GridManager>();
        }

        private void Start()
        {
            if (showTerrainColors && gridManager != null)
            {
                VisualizeTiles();
            }
        }

        /// <summary>
        /// 可视化所有格子
        /// </summary>
        private void VisualizeTiles()
        {
            if (gridManager == null || gridManager.TileMap == null)
                return;

            foreach (var kvp in gridManager.TileMap)
            {
                CreateTileVisual(kvp.Key, kvp.Value);
            }
        }

        /// <summary>
        /// 创建单个格子的可视化
        /// </summary>
        private void CreateTileVisual(Vector2Int gridPos, Tile tile)
        {
            Vector3 worldPos = gridManager.GridToWorld(gridPos);
            worldPos.y = tileHeight * 0.5f;

            GameObject tileObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
            tileObj.name = $"Tile_{gridPos.x}_{gridPos.y}";
            tileObj.transform.SetParent(transform);
            tileObj.transform.position = worldPos;
            tileObj.transform.localScale = new Vector3(
                gridManager.CellSize * 0.9f,
                tileHeight,
                gridManager.CellSize * 0.9f
            );

            // 设置颜色
            Renderer renderer = tileObj.GetComponent<Renderer>();
            Material material = new Material(Shader.Find("Standard"));
            material.color = GetTerrainColor(tile.TerrainType);
            renderer.material = material;

            // 保留碰撞体用于射线检测（设置为触发器，避免物理碰撞）
            Collider collider = tileObj.GetComponent<Collider>();
            if (collider != null)
            {
                collider.isTrigger = true;  // 设置为触发器，保留碰撞体但不会阻挡物理
            }

            tileObjects[gridPos] = tileObj;
        }

        /// <summary>
        /// 获取地形颜色
        /// </summary>
        private Color GetTerrainColor(TerrainType terrain)
        {
            return terrain switch
            {
                TerrainType.Normal => normalColor,
                TerrainType.Grass => grassColor,
                TerrainType.Mud => mudColor,
                TerrainType.Water => waterColor,
                TerrainType.Road => roadColor,
                _ => normalColor
            };
        }

        /// <summary>
        /// 更新单个格子的可视化
        /// </summary>
        public void UpdateTileVisual(Vector2Int gridPos)
        {
            if (gridManager == null)
                return;

            Tile tile = gridManager.GetTile(gridPos);
            if (tile == null)
                return;

            if (tileObjects.ContainsKey(gridPos))
            {
                Destroy(tileObjects[gridPos]);
                tileObjects.Remove(gridPos);
            }

            CreateTileVisual(gridPos, tile);
        }

        private void OnDestroy()
        {
            foreach (var obj in tileObjects.Values)
            {
                if (obj != null)
                {
                    Destroy(obj);
                }
            }
            tileObjects.Clear();
        }
    }
}

