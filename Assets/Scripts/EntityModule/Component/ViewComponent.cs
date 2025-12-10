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
            
            var cube = CreateCube();
            cube.transform.SetParent(Owner.transform);
            entityRenderer = cube.gameObject.GetComponent<Renderer>();
            cube.transform.localPosition = Vector3.zero;
            
            if (entityRenderer == null)
            {
                entityRenderer = Owner.gameObject.AddComponent<MeshRenderer>();
                var a=Owner.gameObject.AddComponent<MeshRenderer>();
            }

            // 创建材质
            material = new Material(Shader.Find("Standard"));
            material.color = defaultColor;
            entityRenderer.material = material;

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

        /// <summary>
        /// 更新旋转（朝向移动方向）
        /// </summary>
        public void UpdateRotation(Vector3 direction)
        {
            if (Owner != null && direction.magnitude > 0.01f)
            {
                Owner.transform.rotation = Quaternion.LookRotation(direction);
            }
        }

        private GameObject CreateCube()
        {
            GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            // 设置 Cube 的位置
            cube.transform.position = new Vector3(0, 0, 0);
            // 可以设置其他属性，例如缩放、旋转等
            cube.transform.localScale = new Vector3(1, 1, 1);   
            return cube;
        }
    }
}

