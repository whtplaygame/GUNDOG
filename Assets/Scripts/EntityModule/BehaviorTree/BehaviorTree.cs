namespace EntityModule.BehaviorTree
{
    /// <summary>
    /// 行为树类（符合迪米特法则）
    /// </summary>
    public class BehaviorTree
    {
        private IBehaviorNode rootNode;
        private Entity owner;

        public BehaviorTree(Entity owner, IBehaviorNode rootNode)
        {
            this.owner = owner;
            this.rootNode = rootNode;
        }

        /// <summary>
        /// 更新行为树
        /// </summary>

        private string nodeName;
        public void Update()
        {
            rootNode?.Execute(owner);
        }

        /// <summary>
        /// 获取根节点（用于Mod扩展）
        /// </summary>
        public IBehaviorNode GetRootNode()
        {
            return rootNode;
        }

        /// <summary>
        /// 设置根节点（用于Mod扩展）
        /// </summary>
        public void SetRootNode(IBehaviorNode node)
        {
            rootNode = node;
        }
    }
}

