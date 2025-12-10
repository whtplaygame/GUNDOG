using System.Collections.Generic;
using EntityModule.Component;
using UnityEngine;
using EntityComponent = EntityModule.Component.Component;

namespace EntityModule
{
    /// <summary>
    /// 实体类（组合优于继承，空壳设计）
    /// 可以是任意东西：人、树、石头等，取决于其持有的组件
    /// </summary>
    public class Entity : MonoBehaviour
    {
        [SerializeField] private int id;
        [SerializeField] private EntityPriority priority = EntityPriority.Static;
        [SerializeField] private Vector2Int gridPosition;
        [SerializeField] private Vector3 worldPosition;

        private Dictionary<System.Type, IComponent> components = new Dictionary<System.Type, IComponent>();

        public int Id => id;
        public EntityPriority Priority => priority;
        public Vector2Int GridPosition => gridPosition;
        public Vector3 WorldPosition => worldPosition;

        /// <summary>
        /// 初始化实体
        /// </summary>
        public void Initialize(int entityId, EntityPriority entityPriority, Vector2Int gridPos, Vector3 worldPos)
        {
            id = entityId;
            priority = entityPriority;
            gridPosition = gridPos;
            worldPosition = worldPos;
            transform.position = worldPos;

            // 初始化所有组件
            foreach (var component in components.Values)
            {
                component.Initialize();
            }
        }

        /// <summary>
        /// 添加组件（符合开闭原则）
        /// </summary>
        public T AddComponent<T>() where T : EntityComponent, new()
        {
            var type = typeof(T);
            if (components.ContainsKey(type))
            {
                return (T)components[type];
            }

            var component = new T();
            component.Owner = this;
            components[type] = component;
            component.Initialize();
            return component;
        }

        /// <summary>
        /// 获取组件（符合迪米特法则）
        /// </summary>
        public new T GetComponent<T>() where T : class, IComponent
        {
            var type = typeof(T);
            return components.ContainsKey(type) ? (T)components[type] : null;
        }

        /// <summary>
        /// 移除组件
        /// </summary>
        public void RemoveComponent<T>() where T : IComponent
        {
            var type = typeof(T);
            if (components.ContainsKey(type))
            {
                components.Remove(type);
            }
        }

        /// <summary>
        /// 是否有组件
        /// </summary>
        public bool HasComponent<T>() where T : IComponent
        {
            return components.ContainsKey(typeof(T));
        }

        /// <summary>
        /// 设置网格位置
        /// </summary>
        public void SetGridPosition(Vector2Int pos)
        {
            gridPosition = pos;
        }

        /// <summary>
        /// 设置世界位置
        /// </summary>
        public void SetWorldPosition(Vector3 pos)
        {
            worldPosition = pos;
            transform.position = pos;
        }

        /// <summary>
        /// 更新实体（由管理器调用，符合好莱坞原则）
        /// </summary>
        public void UpdateEntity(float deltaTime)
        {
            foreach (var component in components.Values)
            {
                if (component.Enabled)
                {
                    component.Update(deltaTime);
                }
            }
        }
    }
}

