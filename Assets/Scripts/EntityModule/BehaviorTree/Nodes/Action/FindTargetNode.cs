using EntityModule.Component;

namespace EntityModule.BehaviorTree.Nodes.Action
{
    /// <summary>
    /// 查找目标节点（符合迪米特法则）
    /// </summary>
    public class FindTargetNode : IBehaviorNode
    {
        public NodeStatus Execute(Entity owner)
        {
            if (owner == null) return NodeStatus.Failure;

            var dataComponent = owner.GetComponent<DataComponent>();
            if (dataComponent == null)
            {
                return NodeStatus.Failure;
            }

            EntityManager manager = EntityManager.Instance;
            if (manager == null)
            {
                return NodeStatus.Failure;
            }

            // 查找目标类型的实体
            EntityType targetType = dataComponent.EntityType == EntityType.Chaser ? EntityType.Target : EntityType.Chaser;
            Entity targetEntity = manager.FindNearestEntity(owner, targetType);

            if (targetEntity != null)
            {
                dataComponent.SetTargetEntity(targetEntity);
                return NodeStatus.Success;
            }

            return NodeStatus.Failure;
        }
    }
}

