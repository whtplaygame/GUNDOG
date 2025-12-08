using UnityEngine;
using Pathfinding.Entity.Component;

namespace Pathfinding.Entity.Component
{
    /// <summary>
    /// 数据组件（存储实体数据，符合单一职责原则）
    /// </summary>
    public class DataComponent : Component
    {
        [SerializeField] private int targetEntityId = -1;
        [SerializeField] private float detectionRange = 10f;
        [SerializeField] private EntityType entityType;

        public int TargetEntityId
        {
            get => targetEntityId;
            set => targetEntityId = value;
        }

        public float DetectionRange
        {
            get => detectionRange;
            set => detectionRange = value;
        }

        public EntityType EntityType
        {
            get => entityType;
            set => entityType = value;
        }

        public void SetTargetEntity(Entity target)
        {
            targetEntityId = target != null ? target.Id : -1;
        }

        public Entity GetTargetEntity()
        {
            if (targetEntityId < 0) return null;
            return EntityManager.Instance?.GetEntity(targetEntityId);
        }
    }
}

