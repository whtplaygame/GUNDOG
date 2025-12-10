namespace EntityModule.Component
{
    /// <summary>
    /// 组件基类（符合KISS原则）
    /// </summary>
    public abstract class Component : IComponent
    {
        public global::EntityModule.Entity Owner { get; set; }
        public bool Enabled { get; set; } = true;

        public virtual void Initialize()
        {
            // 子类可重写
        }

        public virtual void Update(float deltaTime)
        {
            if (!Enabled) return;
            // 子类可重写
        }
    }
}

