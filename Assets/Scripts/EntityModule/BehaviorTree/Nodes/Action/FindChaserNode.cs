using EntityModule.Component;
using UnityEngine;

namespace EntityModule.BehaviorTree.Nodes.Action
{
    /// <summary>
    /// 查找追逐者节点（用于Target检测附近的Chaser，符合迪米特法则）
    /// </summary>
    public class FindChaserNode : IBehaviorNode
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

            // 查找最近的Chaser
            Entity chaserEntity = manager.FindNearestEntity(owner, EntityType.Chaser);

            if (chaserEntity != null)
            {
                // 检查距离是否在检测范围内
                float distance = Vector2Int.Distance(owner.GridPosition, chaserEntity.GridPosition);
                if (distance > dataComponent.DetectionRange)
                {
                    return NodeStatus.Failure;
                }

                // 检查Chaser是否存活
                var chaserCombat = chaserEntity.GetComponent<CombatComponent>();
                if (chaserCombat != null && !chaserCombat.IsAlive)
                {
                    return NodeStatus.Failure;
                }

                dataComponent.SetTargetEntity(chaserEntity);
                return NodeStatus.Success;
            }

            return NodeStatus.Failure;
        }
    }
}

