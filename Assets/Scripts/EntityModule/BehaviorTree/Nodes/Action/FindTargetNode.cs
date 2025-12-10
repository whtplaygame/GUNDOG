using EntityModule.Component;

namespace EntityModule.BehaviorTree.Nodes.Action
{
    /// <summary>
    /// 查找目标节点（符合迪米特法则）
    /// </summary>
    public class FindTargetNode : IBehaviorNode
    {
        public NodeStatus Execute(global::EntityModule.Entity owner)
        {
            if (owner == null) return NodeStatus.Failure;

            var dataComponent = owner.GetComponent<DataComponent>();
            if (dataComponent == null)
            {
                return NodeStatus.Failure;
            }

            global::EntityModule.EntityManager manager = global::EntityModule.EntityManager.Instance;
            if (manager == null)
            {
                return NodeStatus.Failure;
            }

            // 查找目标类型的实体
            global::EntityModule.EntityType targetType = dataComponent.EntityType == global::EntityModule.EntityType.Chaser ? global::EntityModule.EntityType.Target : global::EntityModule.EntityType.Chaser;
            global::EntityModule.Entity targetEntity = manager.FindNearestEntity(owner, targetType);

            if (targetEntity != null)
            {
                dataComponent.SetTargetEntity(targetEntity);
                return NodeStatus.Success;
            }

            return NodeStatus.Failure;
        }
    }
}

