using Map;
using UnityEngine;

namespace EntityModule
{
    /// <summary>
    /// 实体系统测试脚本
    /// </summary>
    public class EntitySystemTest : MonoBehaviour
    {
        [Header("测试设置")]
        [SerializeField] private bool createOnStart = true;
        [SerializeField] private int chaserCount = 1;
        [SerializeField] private int targetCount = 1;
        [SerializeField] private Vector2Int chaserStartPos = new Vector2Int(2, 2);
        [SerializeField] private Vector2Int targetStartPos = new Vector2Int(15, 15);

        private EntityManager entityManager;
        private GridManager gridManager;

        private void Start()
        {
            entityManager = EntityManager.Instance;
            gridManager = FindObjectOfType<GridManager>();

            if (entityManager == null)
            {
                Debug.LogError("EntityManager未找到！");
                return;
            }

            if (gridManager == null)
            {
                Debug.LogError("GridManager未找到！");
                return;
            }

            // 初始化：注册基础实体定义
            GameInitializer.RegisterBaseEntities();

            if (createOnStart)
            {
                CreateTestEntities();
            }
        }

        /// <summary>
        /// 创建测试实体
        /// </summary>
        [ContextMenu("创建测试实体")]
        public void CreateTestEntities()
        {
            if (entityManager == null || gridManager == null)
            {
                Debug.LogError("管理器未初始化！");
                return;
            }

            // 创建目标（使用新的注册表模式）
            for (int i = 0; i < targetCount; i++)
            {
                Vector2Int pos = targetStartPos + new Vector2Int(i % 5, i / 5);
                if (pos.x < gridManager.MapWidth && pos.y < gridManager.MapHeight)
                {
                    Entity target = EntityFactory.Create("Target", pos, gridManager);
                    Debug.Log($"创建目标实体: ID={target?.Id}, 位置={pos}");
                }
            }

            // 创建追逐者（使用新的注册表模式）
            for (int i = 0; i < chaserCount; i++)
            {
                Vector2Int pos = chaserStartPos + new Vector2Int(i % 5, i / 5);
                if (pos.x < gridManager.MapWidth && pos.y < gridManager.MapHeight)
                {
                    Entity chaser = EntityFactory.Create("Chaser", pos, gridManager);
                    Debug.Log($"创建追逐者实体: ID={chaser?.Id}, 位置={pos}");
                }
            }
        }

        /// <summary>
        /// 清除所有实体
        /// </summary>
        [ContextMenu("清除所有实体")]
        public void ClearAllEntities()
        {
            if (entityManager == null) return;

            var allEntities = entityManager.GetAllEntities();
            foreach (var entity in allEntities)
            {
                if (entity != null)
                {
                    entityManager.DestroyEntity(entity.Id);
                }
            }

            Debug.Log("已清除所有实体");
        }
    }
}

