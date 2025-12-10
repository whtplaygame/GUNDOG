using System.Collections.Generic;
using Pathfinding;
using UnityEngine;

namespace Map
{
    /// <summary>
    /// 地图网格管理器
    /// </summary>
    public class GridManager : MonoBehaviour
    {
        [Header("地图设置")]
        [SerializeField] private int mapWidth = 20;
        [SerializeField] private int mapHeight = 20;
        [SerializeField] private float cellSize = 1f;
        [SerializeField] private Vector3 gridOrigin = Vector3.zero;
        [SerializeField] private bool autoCenterGrid = true;  // 自动居中棋盘
        [SerializeField] private bool autoAdjustCamera = true;  // 自动调整相机位置

        [Header("地形生成")]
        [SerializeField] private bool generateRandomTerrain = true;
        [SerializeField] private float terrainRandomSeed = 0f;

        private Dictionary<Vector2Int, Tile> tileMap;
        private AStarPathfinding pathfinding;
        private GameObject[,] tileVisuals;
        private GameObject groundPlane;  // 地面平面引用

        public Dictionary<Vector2Int, Tile> TileMap => tileMap;
        public int MapWidth => mapWidth;
        public int MapHeight => mapHeight;
        public float CellSize => cellSize;
        public Vector3 GridOrigin => gridOrigin;

        private void Awake()
        {
            tileMap = new Dictionary<Vector2Int, Tile>();
            pathfinding = new AStarPathfinding(mapWidth, mapHeight);
            tileVisuals = new GameObject[mapWidth, mapHeight];
            
            // 自动居中棋盘
            if (autoCenterGrid)
            {
                CenterGrid();
            }
            
            GenerateMap();
            pathfinding.SetNodeMap(tileMap);
            CreateGroundPlane();  // 创建地面平面用于射线检测
            
            // 自动调整相机位置
            if (autoAdjustCamera)
            {
                AdjustCameraPosition();
            }
        }

        /// <summary>
        /// 自动居中棋盘
        /// </summary>
        public void CenterGrid()
        {
            // 计算棋盘中心位置
            float totalWidth = mapWidth * cellSize;
            float totalHeight = mapHeight * cellSize;
            gridOrigin = new Vector3(-totalWidth * 0.5f, 0f, -totalHeight * 0.5f);
            
            // 更新地面平面位置（如果已创建）
            if (groundPlane != null)
            {
                groundPlane.transform.position = gridOrigin + new Vector3(totalWidth * 0.5f, 0f, totalHeight * 0.5f);
                groundPlane.transform.localScale = new Vector3(totalWidth / 10f, 1f, totalHeight / 10f);
            }
            
            // 如果启用了自动调整相机，更新相机位置
            if (autoAdjustCamera)
            {
                AdjustCameraPosition();
            }
        }

        /// <summary>
        /// 自动调整相机位置，使相机对准棋盘中心
        /// </summary>
        public void AdjustCameraPosition()
        {
            Camera mainCamera = Camera.main;
            if (mainCamera == null)
            {
                // 如果没有主相机，尝试查找场景中的第一个相机
                mainCamera = FindObjectOfType<Camera>();
                if (mainCamera == null)
                {
                    Debug.LogWarning("未找到相机，无法自动调整相机位置");
                    return;
                }
            }

            // 计算棋盘中心位置
            float totalWidth = mapWidth * cellSize;
            float totalHeight = mapHeight * cellSize;
            Vector3 centerPos = gridOrigin + new Vector3(totalWidth * 0.5f, 0f, totalHeight * 0.5f);

            // 计算相机位置
            // 根据棋盘大小和屏幕宽高比计算合适的相机距离
            float mapSize = Mathf.Max(totalWidth, totalHeight);
            
            // 获取屏幕宽高比
            // 优先使用相机的aspect属性，这是最可靠的方法
            float screenAspect = 16f / 9f;  // 默认16:9
            
            // 尝试从相机获取宽高比（最可靠）
            if (mainCamera != null && mainCamera.aspect > 0)
            {
                screenAspect = mainCamera.aspect;
            }
            // 如果相机aspect不可用，尝试使用Screen.currentResolution（实际屏幕分辨率）
            else if (Screen.currentResolution.width > 0 && Screen.currentResolution.height > 0)
            {
                screenAspect = (float)Screen.currentResolution.width / Screen.currentResolution.height;
            }
            // 最后使用Screen.width/height作为后备（可能不准确）
            else if (Screen.width > 0 && Screen.height > 0)
            {
                screenAspect = (float)Screen.width / Screen.height;
            }
            
            // 计算相机高度，确保棋盘在视野内
            // 使用正交投影或透视投影的视野计算
            float cameraHeight;
            float cameraDistance;
            
            if (mainCamera.orthographic)
            {
                // 正交相机：根据棋盘大小设置orthographicSize
                // 考虑宽高比，确保棋盘完全在视野内
                float orthoSizeForWidth = (totalWidth * 0.5f) / screenAspect;
                float orthoSizeForHeight = totalHeight * 0.5f;
                float orthoSize = Mathf.Max(orthoSizeForWidth, orthoSizeForHeight) * 1.1f;// 增加10%边距
                mainCamera.orthographicSize = orthoSize;
                cameraHeight = mapSize * 1.5f;  // 正交相机高度
                cameraDistance = 0f;  // 正交相机不需要距离，直接垂直向下
            }
            else
            {
                // 透视相机：计算合适的距离和高度
                float fov = mainCamera.fieldOfView;
                // 计算宽度和高度所需的距离
                float distanceForWidth = (totalWidth * 0.5f) / (Mathf.Tan(fov * 0.5f * Mathf.Deg2Rad) * screenAspect);
                float distanceForHeight = (totalHeight * 0.5f) / Mathf.Tan(fov * 0.5f * Mathf.Deg2Rad);
                float requiredDistance = Mathf.Max(distanceForWidth, distanceForHeight) * 1.2f;  // 增加20%边距
                
                cameraHeight = requiredDistance * 0.8f;  // 相机高度为距离的80%
                cameraDistance = requiredDistance * 0.4f;  // 相机水平距离
            }

            // 设置相机位置（从斜上方俯视）
            mainCamera.transform.position = centerPos + new Vector3(cameraDistance, cameraHeight, cameraDistance);
            mainCamera.transform.LookAt(centerPos);
        }

        /// <summary>
        /// 创建地面平面用于射线检测
        /// </summary>
        private void CreateGroundPlane()
        {
            // 如果已存在地面平面，先销毁
            if (groundPlane != null)
            {
                if (Application.isPlaying)
                    Destroy(groundPlane);
                else
                    DestroyImmediate(groundPlane);
            }

            // 创建地面平面
            groundPlane = GameObject.CreatePrimitive(PrimitiveType.Plane);
            groundPlane.name = "GroundPlane";
            groundPlane.transform.SetParent(transform);
            
            // 设置位置和大小
            float totalWidth = mapWidth * cellSize;
            float totalHeight = mapHeight * cellSize;
            groundPlane.transform.position = gridOrigin + new Vector3(totalWidth * 0.5f, 0f, totalHeight * 0.5f);
            groundPlane.transform.localScale = new Vector3(totalWidth / 10f, 1f, totalHeight / 10f);  // Plane默认是10x10
            
            // 设置材质（半透明，便于看到格子）
            Renderer renderer = groundPlane.GetComponent<Renderer>();
            Material material = new Material(Shader.Find("Standard"));
            material.color = new Color(0.5f, 0.5f, 0.5f, 0.3f);
            material.SetFloat("_Mode", 3);  // 设置为透明模式
            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            material.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            material.SetInt("_ZWrite", 0);
            material.DisableKeyword("_ALPHATEST_ON");
            material.EnableKeyword("_ALPHABLEND_ON");
            material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            material.renderQueue = 3000;
            renderer.material = material;
        }

        /// <summary>
        /// 生成地图
        /// </summary>
        private void GenerateMap()
        {
            tileMap.Clear();

            for (int x = 0; x < mapWidth; x++)
            {
                for (int y = 0; y < mapHeight; y++)
                {
                    var gridPos = new Vector2Int(x, y);
                    TerrainType terrain = TerrainType.Normal;
                    BuffType buff = BuffType.None;

                    if (generateRandomTerrain)
                    {
                        // 随机生成地形
                        float noise = Mathf.PerlinNoise((x + terrainRandomSeed) * 0.1f, (y + terrainRandomSeed) * 0.1f);
                        if (noise < 0.2f)
                            terrain = TerrainType.Water;
                        else if (noise < 0.4f)
                            terrain = TerrainType.Mud;
                        else if (noise < 0.6f)
                            terrain = TerrainType.Grass;
                        else if (noise > 0.85f)
                            terrain = TerrainType.Road;

                        // 随机生成buff
                        float buffChance = Random.Range(0f, 1f);
                        if (buffChance < 0.05f)
                            buff = BuffType.SpeedUp;
                        else if (buffChance < 0.1f)
                            buff = BuffType.SlowDown;
                    }

                    var tile = new Tile(gridPos, terrain, buff);
                    tileMap[gridPos] = tile;
                }
            }
        }

        /// <summary>
        /// 世界坐标转网格坐标
        /// </summary>
        public Vector2Int WorldToGrid(Vector3 worldPos)
        {
            Vector3 localPos = worldPos - gridOrigin;
            int x = Mathf.FloorToInt(localPos.x / cellSize);
            int y = Mathf.FloorToInt(localPos.z / cellSize);
            return new Vector2Int(x, y);
        }

        /// <summary>
        /// 网格坐标转世界坐标
        /// </summary>
        public Vector3 GridToWorld(Vector2Int gridPos)
        {
            float x = gridPos.x * cellSize + cellSize * 0.5f;
            float z = gridPos.y * cellSize + cellSize * 0.5f;
            return gridOrigin + new Vector3(x, 0f, z);
        }

        /// <summary>
        /// 获取指定位置的Tile
        /// </summary>
        public Tile GetTile(Vector2Int gridPos)
        {
            return tileMap.ContainsKey(gridPos) ? tileMap[gridPos] : null;
        }

        /// <summary>
        /// 执行寻路
        /// </summary>
        public List<Vector2Int> FindPath(Vector2Int start, Vector2Int end)
        {
            if (pathfinding == null)
            {
                pathfinding = new AStarPathfinding(mapWidth, mapHeight);
                pathfinding.SetNodeMap(tileMap);
            }
            return pathfinding.FindPath(start, end);
        }

        /// <summary>
        /// 设置Tile的地形类型
        /// </summary>
        public void SetTileTerrain(Vector2Int gridPos, TerrainType terrain)
        {
            if (tileMap.ContainsKey(gridPos))
            {
                tileMap[gridPos].SetTerrainType(terrain);
            }
        }

        /// <summary>
        /// 设置Tile的Buff类型
        /// </summary>
        public void SetTileBuff(Vector2Int gridPos, BuffType buff)
        {
            if (tileMap.ContainsKey(gridPos))
            {
                tileMap[gridPos].SetBuffType(buff);
            }
        }

        private void OnDrawGizmos()
        {
            // 绘制网格
            Gizmos.color = Color.white;
            for (int x = 0; x <= mapWidth; x++)
            {
                Vector3 start = gridOrigin + new Vector3(x * cellSize, 0f, 0f);
                Vector3 end = gridOrigin + new Vector3(x * cellSize, 0f, mapHeight * cellSize);
                Gizmos.DrawLine(start, end);
            }

            for (int y = 0; y <= mapHeight; y++)
            {
                Vector3 start = gridOrigin + new Vector3(0f, 0f, y * cellSize);
                Vector3 end = gridOrigin + new Vector3(mapWidth * cellSize, 0f, y * cellSize);
                Gizmos.DrawLine(start, end);
            }
        }
    }
}

