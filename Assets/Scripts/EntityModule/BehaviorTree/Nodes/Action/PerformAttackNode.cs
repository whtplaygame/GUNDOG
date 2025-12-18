using EntityModule.Component;
using UnityEngine;

namespace EntityModule.BehaviorTree.Nodes.Action
{
    /// <summary>
    /// 执行攻击节点（Command层）
    /// 职责：判断局势，调用Component的状态机方法
    /// </summary>
    public class PerformAttackNode : IBehaviorNode
    {
        public NodeStatus Execute(Entity owner)
        {
            if (owner == null) return NodeStatus.Failure;

            var dataComponent = owner.GetComponent<DataComponent>();
            var combatComponent = owner.GetComponent<CombatComponent>();
            
            if (dataComponent == null || combatComponent == null)
            {
                return NodeStatus.Failure;
            }

            Entity targetEntity = dataComponent.GetTargetEntity();
            if (targetEntity == null)
            {
                return NodeStatus.Failure;
            }

            // 1. 如果处于空闲状态，说明还没开始打，或者刚打完
            if (combatComponent.AttackState == AttackState.Idle)
            {
                // 尝试发起攻击
                bool started = combatComponent.TryStartAttack(targetEntity);
                if (started)
                {
                    Debug.Log($"[PerformAttackNode] {owner.name} 开始攻击，状态: {combatComponent.AttackState}");
                    return NodeStatus.Running; // 刚开始，继续运行
                }
                else
                {
                    // 可能是CD没好，或者其他原因（硬直、距离不够等）
                    Debug.LogWarning($"[PerformAttackNode] {owner.name} 无法攻击 - CanAttack: {combatComponent.CanAttack}, IsAlive: {combatComponent.IsAlive}, IsInHitStun: {combatComponent.IsInHitStun}, Distance: {Vector2Int.Distance(owner.GridPosition, targetEntity.GridPosition)}");
                    return NodeStatus.Failure; 
                }
            }
            
            // 2. 如果正在进行中 (Prepare, WindUp, Impact, Recovery)
            // 注意：状态机的更新由CombatComponent.TickLogic()负责
            // 这里不需要手动调用，因为Entity.UpdateEntity()会在行为树更新后调用所有Component的TickLogic()

            // 3. 再次检查状态，如果变回 Idle 了，说明打完了
            if (combatComponent.AttackState == AttackState.Idle)
            {
                return NodeStatus.Success;
            }

            // 4. 如果被打断了（比如被攻击），返回Failure
            // 注意：CancelAttack()会在TakeDamage()中调用，状态会变回Idle
            // 但这里我们检查是否还在攻击状态，如果不在说明被打断了
            if (combatComponent.AttackState == AttackState.Idle && combatComponent.IsInHitStun)
            {
                return NodeStatus.Failure;
            }

            return NodeStatus.Running;
        }
    }
}

