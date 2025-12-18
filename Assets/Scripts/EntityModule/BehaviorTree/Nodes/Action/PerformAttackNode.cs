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
                    return NodeStatus.Running; // 刚开始，继续运行
                }
                else
                {
                    // 可能是CD没好，或者其他原因（硬直、距离不够等）
                    return NodeStatus.Failure; 
                }
            }
            
            // 2. 如果正在进行中 (Prepare, WindUp, Impact, Recovery)
            // *关键点*：节点这里显式调用 Component 的逻辑更新
            // 这样就保证了 DataExecute 的时序完全由行为树控制
            combatComponent.TickLogic(Time.deltaTime);

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

