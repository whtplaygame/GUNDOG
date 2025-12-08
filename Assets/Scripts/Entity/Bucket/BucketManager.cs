using System.Collections.Generic;
using UnityEngine;
using Pathfinding.Entity.LOD;

namespace Pathfinding.Entity.Bucket
{
    /// <summary>
    /// 分桶管理器
    /// 负责管理不同优先级的实体桶
    /// </summary>
    public class BucketManager
    {
        private Dictionary<EntityPriority, EntityBucket> buckets = new Dictionary<EntityPriority, EntityBucket>();
        private int frameCount = 0;

        public BucketManager()
        {
            // 初始化所有优先级的桶
            foreach (EntityPriority priority in System.Enum.GetValues(typeof(EntityPriority)))
            {
                buckets[priority] = new EntityBucket(priority);
            }
        }

        /// <summary>
        /// 添加实体到对应的桶
        /// </summary>
        public void AddEntity(Entity entity)
        {
            if (entity == null) return;

            EntityPriority priority = entity.Priority;
            if (buckets.ContainsKey(priority))
            {
                buckets[priority].AddEntity(entity);
            }
        }

        /// <summary>
        /// 从桶中移除实体
        /// </summary>
        public void RemoveEntity(Entity entity)
        {
            if (entity == null) return;

            EntityPriority priority = entity.Priority;
            if (buckets.ContainsKey(priority))
            {
                buckets[priority].RemoveEntity(entity);
            }
        }

        /// <summary>
        /// 更新所有桶（由EntityManager调用，符合好莱坞原则）
        /// </summary>
        public void UpdateBuckets(float deltaTime)
        {
            frameCount++;
            Vector3 referencePosition = LODSystem.GetReferencePosition();

            foreach (var bucket in buckets.Values)
            {
                bucket.UpdateEntities(deltaTime, referencePosition);
            }
        }

        /// <summary>
        /// 获取指定优先级的桶
        /// </summary>
        public EntityBucket GetBucket(EntityPriority priority)
        {
            return buckets.ContainsKey(priority) ? buckets[priority] : null;
        }

        /// <summary>
        /// 获取所有实体（用于查询）
        /// </summary>
        public List<Entity> GetAllEntities()
        {
            List<Entity> allEntities = new List<Entity>();
            foreach (var bucket in buckets.Values)
            {
                allEntities.AddRange(bucket.GetEntities());
            }
            return allEntities;
        }

        /// <summary>
        /// 获取指定优先级的实体
        /// </summary>
        public List<Entity> GetEntitiesByPriority(EntityPriority priority)
        {
            var bucket = GetBucket(priority);
            return bucket != null ? bucket.GetEntities() : new List<Entity>();
        }

        /// <summary>
        /// 清空所有桶
        /// </summary>
        public void Clear()
        {
            foreach (var bucket in buckets.Values)
            {
                bucket.Clear();
            }
        }
    }
}

