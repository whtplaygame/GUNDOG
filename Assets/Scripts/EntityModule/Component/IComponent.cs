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
        Entity Owner { get; set; }

        /// <summary>
        /// 初始化组件
        /// </summary>
        void Initialize();

        /// <summary>
        /// 逻辑更新（Data Execute阶段）
        /// 负责状态机推进、数值计算等逻辑
        /// </summary>
        void TickLogic(float deltaTime);

        /// <summary>
        /// 表现更新（View Execute阶段）
        /// 负责同步Animator、特效等表现
        /// </summary>
        void UpdateView();

        /// <summary>
        /// 组件是否启用
        /// </summary>
        bool Enabled { get; set; }
    }
}

