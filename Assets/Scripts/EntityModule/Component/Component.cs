namespace EntityModule.Component
{
    /// <summary>
    /// 组件基类（符合KISS原则）
    /// 支持MVC架构：Logic和View分离
    /// </summary>
    public abstract class Component : IComponent
    {
        public Entity Owner { get; set; }
        public bool Enabled { get; set; } = true;

        public virtual void Initialize()
        {
            // 子类可重写
        }

        /// <summary>
        /// 逻辑更新（Data Execute阶段）
        /// 负责状态机推进、数值计算等逻辑
        /// </summary>
        public virtual void TickLogic(float deltaTime)
        {
            if (!Enabled) return;
            // 子类可重写
        }

        /// <summary>
        /// 表现更新（View Execute阶段）
        /// 负责同步Animator、特效等表现
        /// </summary>
        public virtual void UpdateView()
        {
            if (!Enabled) return;
            // 子类可重写
        }

        /// <summary>
        /// 兼容旧接口（已废弃，保留用于向后兼容）
        /// </summary>
        [System.Obsolete("Use TickLogic() instead")]
        public virtual void Update(float deltaTime)
        {
            TickLogic(deltaTime);
        }
    }
}

