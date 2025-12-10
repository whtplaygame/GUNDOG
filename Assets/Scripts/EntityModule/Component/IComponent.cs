namespace EntityModule.Component
{
    /// <summary>
    /// 组件接口（符合开闭原则，单一职责原则）
    /// </summary>
    public interface IComponent
    {
        /// <summary>
        /// 组件所属的实体
        /// </summary>
        global::EntityModule.Entity Owner { get; set; }

        /// <summary>
        /// 初始化组件
        /// </summary>
        void Initialize();

        /// <summary>
        /// 更新组件
        /// </summary>
        void Update(float deltaTime);

        /// <summary>
        /// 组件是否启用
        /// </summary>
        bool Enabled { get; set; }
    }
}

