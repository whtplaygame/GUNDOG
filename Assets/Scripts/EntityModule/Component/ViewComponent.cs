using Unity.VisualScripting;
using UnityEngine;

namespace EntityModule.Component
{
    /// <summary>
    /// 视图组件（单一职责原则：只负责视觉表现）
    /// </summary>
    public class ViewComponent : Component
    {
        [SerializeField] private Color defaultColor = Color.white;
        [SerializeField] private float viewHeight = 0.5f;

        private Renderer entityRenderer;
        private Material material;

        public Color Color
        {
            get => material != null ? material.color : defaultColor;
            set
            {
                if (material != null)
                {
                    material.color = value;
                }
            }
        }

        public override void Initialize()
        {
            base.Initialize();
            
            if (Owner == null) return;

            var f = Resources.Load<GameObject>("Fighter");
            var f_go= GameObject.Instantiate(f);
            f_go.transform.SetParent(Owner.transform);
            f_go.transform.SetLocalPositionAndRotation(Vector3.zero, Quaternion.Euler(90, 0, 0));

            // 更新位置
            UpdatePosition();
        }

        public override void Update(float deltaTime)
        {
            base.Update(deltaTime);
            
            if (Owner != null)
            {
                UpdatePosition();
            }
        }

        /// <summary>
        /// 更新位置
        /// </summary>
        private void UpdatePosition()
        {
            if (Owner != null)
            {
                Vector3 pos = Owner.WorldPosition;
                Owner.transform.position = pos + Vector3.up * viewHeight;
            }
        }
    }
}

