using EntityModule.Component;

namespace EntityModule.BehaviorTree.Nodes.Action
{
    /// <summary>
    /// 查找敌人节点（通用查找节点，符合迪米特法则）
    /// 查找距离最近的敌对实体
    /// </summary>
    public class FindEnemyNode : IBehaviorNode
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

            // 获取当前实体类型
            EntityType currentType = dataComponent.EntityType;
            
            // 查找所有敌对类型的实体
            // Archer的敌人是Chaser
            // Chaser的敌人是Target和Archer
            // Target的敌人是Chaser
            Entity nearestEnemy = null;
            float nearestDistance = float.MaxValue;

            // 遍历所有实体，查找最近的敌人
            var allEntities = manager.GetAllEntities();
            foreach (var entity in allEntities)
            {
                if (entity == owner) continue;

                var enemyDataComp = entity.GetComponent<DataComponent>();
                if (enemyDataComp == null) continue;

                EntityType enemyType = enemyDataComp.EntityType;
                
                // 判断是否是敌对关系
                bool isEnemy = false;
                switch (currentType)
                {
                    case EntityType.Archer:
                        // 射手的敌人是追逐者
                        isEnemy = (enemyType == EntityType.Chaser);
                        break;
                    case EntityType.Chaser:
                        // 追逐者的敌人是目标和射手
                        isEnemy = (enemyType == EntityType.Target || enemyType == EntityType.Archer);
                        break;
                    case EntityType.Target:
                        // 目标的敌人是追逐者
                        isEnemy = (enemyType == EntityType.Chaser);
                        break;
                }

                if (!isEnemy) continue;

                // 检查敌人是否存活
                var enemyCombat = entity.GetComponent<CombatComponent>();
                if (enemyCombat == null || !enemyCombat.IsAlive) continue;

                // 计算距离
                float distance = UnityEngine.Vector2Int.Distance(owner.GridPosition, entity.GridPosition);
                
                // 检查是否在探测范围内
                if (distance <= dataComponent.DetectionRange && distance < nearestDistance)
                {
                    nearestDistance = distance;
                    nearestEnemy = entity;
                }
            }

            if (nearestEnemy != null)
            {
                dataComponent.SetTargetEntity(nearestEnemy);
                return NodeStatus.Success;
            }

            return NodeStatus.Failure;
        }
    }
}

