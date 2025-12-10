using System.Collections.Generic;
using EntityModule.Bucket;
using EntityModule.Component;
using Map;
using UnityEngine;

namespace EntityModule
{
    /// <summary>
    /// 实体管理器（符合好莱坞原则：实体不主动调用管理器，由管理器调用实体）
    /// </summary>
    public class EntityManager : MonoBehaviour
    {
        private static EntityManager instance;
        public static EntityManager Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<EntityManager>();
                    if (instance == null)
                    {
                        GameObject go = new GameObject("EntityManager");
                        instance = go.AddComponent<EntityManager>();
                    }
                }
                return instance;
            }
        }

        [Header("管理器设置")]
        [SerializeField] private GridManager gridManager;

        private Dictionary<int, Entity> entityMap = new Dictionary<int, Entity>();
        private BucketManager bucketManager;
        private int nextEntityId = 0;

        public GridManager GridManager => gridManager;
        public BucketManager BucketManager => bucketManager;

        /// <summary>
        /// 获取下一个实体ID（内部使用）
        /// </summary>
        internal int GetNextEntityId()
        {
            return nextEntityId++;
        }

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;

            entityMap = new Dictionary<int, Entity>();
            bucketManager = new BucketManager();

            if (gridManager == null)
            {
                gridManager = FindObjectOfType<GridManager>();
            }
        }

        private void Update()
        {
            // 更新所有桶（符合好莱坞原则：管理器主动更新实体）
            bucketManager?.UpdateBuckets(Time.deltaTime);
        }

        /// <summary>
        /// 注册实体（只负责管理，不负责创建）
        /// </summary>
        public void RegisterEntity(Entity entity)
        {
            if (entity == null) return;

            if (!entityMap.ContainsKey(entity.Id))
            {
                entityMap[entity.Id] = entity;
                bucketManager.AddEntity(entity);
            }
        }

        /// <summary>
        /// 注销实体
        /// </summary>
        public void UnregisterEntity(Entity entity)
        {
            if (entity == null) return;

            if (entityMap.ContainsKey(entity.Id))
            {
                bucketManager.RemoveEntity(entity);
                entityMap.Remove(entity.Id);
            }
        }

        /// <summary>
        /// 获取实体
        /// </summary>
        public Entity GetEntity(int id)
        {
            return entityMap.ContainsKey(id) ? entityMap[id] : null;
        }

        /// <summary>
        /// 获取所有实体
        /// </summary>
        public List<Entity> GetAllEntities()
        {
            return bucketManager.GetAllEntities();
        }

        /// <summary>
        /// 获取指定优先级的实体
        /// </summary>
        public List<Entity> GetEntitiesByPriority(EntityPriority priority)
        {
            return bucketManager.GetEntitiesByPriority(priority);
        }

        /// <summary>
        /// 查找最近的指定类型实体
        /// </summary>
        public Entity FindNearestEntity(Entity fromEntity, EntityType targetType)
        {
            if (fromEntity == null) return null;

            var dataComponent = fromEntity.GetComponent<DataComponent>();
            if (dataComponent == null) return null;

            float minDistance = float.MaxValue;
            Entity nearestEntity = null;

            foreach (var entity in entityMap.Values)
            {
                if (entity == fromEntity) continue;

                var entityData = entity.GetComponent<DataComponent>();
                if (entityData == null || entityData.EntityType != targetType) continue;

                float distance = Vector2Int.Distance(fromEntity.GridPosition, entity.GridPosition);
                if (distance < minDistance)
                {
                    minDistance = distance;
                    nearestEntity = entity;
                }
            }

            return nearestEntity;
        }

        /// <summary>
        /// 销毁实体
        /// </summary>
        public void DestroyEntity(int id)
        {
            if (entityMap.ContainsKey(id))
            {
                Entity entity = entityMap[id];
                UnregisterEntity(entity);
                
                if (entity != null)
                {
                    Destroy(entity.gameObject);
                }
            }
        }

        /// <summary>
        /// 销毁实体（通过Entity对象）
        /// </summary>
        public void DestroyEntity(Entity entity)
        {
            if (entity != null)
            {
                DestroyEntity(entity.Id);
            }
        }
    }
}
