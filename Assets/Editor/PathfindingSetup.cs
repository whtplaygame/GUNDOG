using Map;
using UnityEngine;
using UnityEditor;
using Pathfinding;

namespace PathfindingEditor
{
    /// <summary>
    /// 寻路系统快速设置工具
    /// </summary>
    public class PathfindingSetup : EditorWindow
    {
        [MenuItem("Tools/寻路系统/快速设置场景")]
        public static void ShowWindow()
        {
            GetWindow<PathfindingSetup>("寻路系统设置");
        }

        private void OnGUI()
        {
            GUILayout.Label("寻路系统快速设置", EditorStyles.boldLabel);
            GUILayout.Space(10);

            if (GUILayout.Button("创建完整寻路系统", GUILayout.Height(30)))
            {
                SetupPathfindingSystem();
            }

            GUILayout.Space(10);
            GUILayout.Label("说明：", EditorStyles.boldLabel);
            GUILayout.Label("1. 点击按钮将在当前场景创建完整的寻路系统");
            GUILayout.Label("2. 包括：GridManager、PathVisualizer、PathfindingTestController");
            GUILayout.Label("3. 运行场景后按空格键执行寻路");
            GUILayout.Label("4. 按C键清除路径");
            GUILayout.Label("5. 点击鼠标左键设置起点和终点");
        }

        private void SetupPathfindingSystem()
        {
            // 创建主管理器对象
            GameObject managerObj = new GameObject("PathfindingManager");
            
            // 添加GridManager
            GridManager gridManager = managerObj.AddComponent<GridManager>();
            
            // 添加PathVisualizer
            PathVisualizer pathVisualizer = managerObj.AddComponent<PathVisualizer>();
            
            // 添加PathfindingTestController
            PathfindingTestController testController = managerObj.AddComponent<PathfindingTestController>();
            
            // 添加TileVisualizer（可选）
            TileVisualizer tileVisualizer = managerObj.AddComponent<TileVisualizer>();

            // 设置相机位置（如果存在主相机）
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                // 根据地图大小自动调整相机位置
                int mapWidth = gridManager.MapWidth;
                int mapHeight = gridManager.MapHeight;
                float cellSize = gridManager.CellSize;
                
                float mapSize = Mathf.Max(mapWidth, mapHeight) * cellSize;
                float cameraHeight = mapSize * 1.5f;  // 相机高度为地图大小的1.5倍
                float cameraDistance = mapSize * 0.8f;  // 相机距离
                
                Vector3 centerPos = gridManager.GridOrigin + new Vector3(mapWidth * cellSize * 0.5f, 0f, mapHeight * cellSize * 0.5f);
                mainCamera.transform.position = centerPos + new Vector3(cameraDistance, cameraHeight, cameraDistance);
                mainCamera.transform.LookAt(centerPos);
            }

            Debug.Log("寻路系统设置完成！");
            Selection.activeGameObject = managerObj;
        }
    }
}

