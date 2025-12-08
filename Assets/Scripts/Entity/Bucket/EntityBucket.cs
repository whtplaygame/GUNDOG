using System.Collections.Generic;
using UnityEngine;

namespace Pathfinding.Entity.Bucket
{
    /// <summary>
    /// 实体桶（分桶策略）
    /// 按优先级分类，避免频繁遍历所有Entity
    /// </summary>
    public class EntityBucket
    {
        private List<Entity> entities = new List<Entity>();
        private EntityPriority priority;
        private int updateFrameCount = 0;

        public EntityPriority Priority => priority;
        public int Count => entities.Count;

        public EntityBucket(EntityPriority priority)
        {
            this.priority = priority;
        }

        /// <summary>
        /// 添加实体
        /// </summary>
        public void AddEntity(Entity entity)
        {
            if (!entities.Contains(entity))
            {
                entities.Add(entity);
            }
        }

        /// <summary>
        /// 移除实体
        /// </summary>
        public void RemoveEntity(Entity entity)
        {
            entities.Remove(entity);
        }

        /// <summary>
        /// 更新桶内所有实体（由管理器调用，符合好莱坞原则）
        /// </summary>
        public void UpdateEntities(float deltaTime, Vector3 referencePosition)
        {
            updateFrameCount++;

            for (int i = entities.Count - 1; i >= 0; i--)
            {
                if (entities[i] == null)
                {
                    entities.RemoveAt(i);
                    continue;
                }

                // 根据LOD级别决定是否更新
                var lodLevel = LOD.LODSystem.CalculateLODLevel(
                    entities[i].WorldPosition, 
                    referencePosition
                );

                // 静态物体只在近距离更新
                if (priority == EntityPriority.Static && lodLevel != LOD.EntityLODLevel.High)
                {
                    continue;
                }

                // 检查是否应该更新
                if (LOD.LODSystem.ShouldUpdate(lodLevel, updateFrameCount))
                {
                    entities[i].UpdateEntity(deltaTime);
                }
            }
        }

        /// <summary>
        /// 获取所有实体（用于查询）
        /// </summary>
        public List<Entity> GetEntities()
        {
            return new List<Entity>(entities);
        }

        /// <summary>
        /// 清空桶
        /// </summary>
        public void Clear()
        {
            entities.Clear();
        }
    }
}

