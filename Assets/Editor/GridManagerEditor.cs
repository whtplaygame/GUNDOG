using Map;
using UnityEngine;
using UnityEditor;

namespace Editor
{
    /// <summary>
    /// GridManager的自定义编辑器，用于实时更新棋盘中心
    /// </summary>
    [CustomEditor(typeof(GridManager))]
    public class GridManagerEditor : UnityEditor.Editor
    {
        private GridManager gridManager;
        private int lastMapWidth;
        private int lastMapHeight;
        private float lastCellSize;
        private bool lastAutoCenter;

        private void OnEnable()
        {
            gridManager = (GridManager)target;
            lastMapWidth = gridManager.MapWidth;
            lastMapHeight = gridManager.MapHeight;
            lastCellSize = gridManager.CellSize;
            lastAutoCenter = GetAutoCenterValue();
        }

        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();

            // 检查地图参数是否改变
            bool autoCenter = GetAutoCenterValue();
            int currentWidth = GetMapWidthValue();
            int currentHeight = GetMapHeightValue();
            float currentCellSize = GetCellSizeValue();

            if (autoCenter && (currentWidth != lastMapWidth || currentHeight != lastMapHeight || currentCellSize != lastCellSize || autoCenter != lastAutoCenter))
            {
                // 参数改变且启用了自动居中，更新中心位置
                gridManager.CenterGrid();
                EditorUtility.SetDirty(gridManager);
                
                lastMapWidth = currentWidth;
                lastMapHeight = currentHeight;
                lastCellSize = currentCellSize;
                lastAutoCenter = autoCenter;
            }

            // 添加手动居中按钮
            EditorGUILayout.Space();
            if (GUILayout.Button("手动居中棋盘"))
            {
                gridManager.CenterGrid();
                EditorUtility.SetDirty(gridManager);
            }
            
            // 添加调整相机按钮
            EditorGUILayout.Space();
            if (GUILayout.Button("调整相机位置"))
            {
                gridManager.AdjustCameraPosition();
            }
        }

        private bool GetAutoCenterValue()
        {
            SerializedProperty autoCenterProp = serializedObject.FindProperty("autoCenterGrid");
            return autoCenterProp != null && autoCenterProp.boolValue;
        }

        private int GetMapWidthValue()
        {
            SerializedProperty prop = serializedObject.FindProperty("mapWidth");
            return prop != null ? prop.intValue : 20;
        }

        private int GetMapHeightValue()
        {
            SerializedProperty prop = serializedObject.FindProperty("mapHeight");
            return prop != null ? prop.intValue : 20;
        }

        private float GetCellSizeValue()
        {
            SerializedProperty prop = serializedObject.FindProperty("cellSize");
            return prop != null ? prop.floatValue : 1f;
        }
    }
}

