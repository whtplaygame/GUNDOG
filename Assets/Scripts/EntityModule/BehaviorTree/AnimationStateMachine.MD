**“基于时间轴的动作状态机”（Timeline-based Action State Machine）**。

*   **核心理念**：动作不再是一个瞬间的函数调用，而是一个**随着时间流逝的流程（Process）**。
*   **默认流程**：`Start` -> `WindUp (前摇)` -> `Impact (生效)` -> `Recovery (后摇)` -> `Finish`。
*   **打断机制**：在特定的时间段（Phase）内，如果有特定的外部信号（如移动输入、死亡、眩晕），流程会强制跳转到 `Failure` 或 `Success`。

下面我将为你设计并实现这个模块。

---

### 1. 数据层：定义时间轴配置

我们需要在配置中明确描述这三个阶段的时间点。

```csharp
namespace Pathfinding.Entity.Data
{
    [System.Serializable]
    public class AbilityConfig
    {
        public string AbilityID;      // 技能ID
        public string AnimActionName; // 对应的动画指令名 (View用)
        
        // --- 时间轴配置 (核心) ---
        public float HitTime;         // 伤害生效时间点 (前摇结束点)
        public float TotalDuration;   // 动作总时长
        
        // --- 行为参数 ---
        public bool CanCancelWindUp;  // 前摇期间是否允许被移动打断？
        public bool CanCancelRecovery;// 后摇期间是否允许被移动打断 (走A)？
        public float AttackRange;     // 攻击距离
    }
}
```

---

### 2. 逻辑层：动作执行节点 (核心实现)

这是整个方案的心脏。我们将 `PerformAttackNode` 设计为一个**微型状态机**。

为了清晰，我在类内部定义了一个 `Phase` 枚举来管理当前走到哪一步了。

```csharp
using UnityEngine;
using Pathfinding.Entity.Component;
using Pathfinding.Entity.Data;

namespace Pathfinding.Entity.BehaviorTree.Nodes
{
    public class PerformAttackNode : IBehaviorNode
    {
        // 内部状态定义
        private enum AttackPhase
        {
            None,       // 未开始
            WindUp,     // 前摇中 (动作开始 -> 伤害判定前)
            Recovery,   // 后摇中 (伤害判定后 -> 动作结束)
            Finished    // 已完成
        }

        // 运行时状态
        private AttackPhase currentPhase = AttackPhase.None;
        private float timer = 0f;
        private AbilityConfig config;
        
        // 缓存组件引用 (避免每帧 GetComponent)
        private CombatComponent combatComp;
        private InputComponent inputComp;
        private AnimationComponent animComp;
        private DataComponent dataComp;

        public NodeStatus Execute(Entity owner)
        {
            // --- 1. 初始化阶段 (Enter) ---
            if (currentPhase == AttackPhase.None)
            {
                // 获取组件
                combatComp = owner.GetComponent<CombatComponent>();
                inputComp = owner.GetComponent<InputComponent>();
                animComp = owner.GetComponent<AnimationComponent>();
                dataComp = owner.GetComponent<DataComponent>();

                // 获取配置 (假设默认普攻)
                config = combatComp.GetAbilityConfig("NormalAttack");

                // 开始状态
                timer = 0f;
                currentPhase = AttackPhase.WindUp;
                
                // 播放动画 (表现层)
                animComp?.PlayAction(config.AnimActionName);
                
                // 返回 Running 继续下一帧
                return NodeStatus.Running;
            }

            // --- 2. 逻辑更新阶段 (Update) ---
            timer += Time.deltaTime;

            // 检查全局打断条件 (比如被晕了，或者自己死了)
            if (!combatComp.IsAlive || combatComp.IsStunned)
            {
                Reset();
                return NodeStatus.Failure;
            }

            // 根据当前阶段执行不同逻辑
            switch (currentPhase)
            {
                case AttackPhase.WindUp:
                    return HandleWindUpState(owner);
                    
                case AttackPhase.Recovery:
                    return HandleRecoveryState(owner);
                    
                case AttackPhase.Finished:
                    Reset();
                    return NodeStatus.Success;
            }

            return NodeStatus.Running;
        }

        // --- 子状态：前摇处理 ---
        private NodeStatus HandleWindUpState(Entity owner)
        {
            // A. 检查打断 (取消攻击)
            if (config.CanCancelWindUp && inputComp.HasMoveInput())
            {
                // 玩家输入了移动，打断前摇
                Reset();
                return NodeStatus.Failure; // 返回失败，让行为树切去 MoveNode
            }

            // B. 检查时间点：是否到达伤害生效点？
            if (timer >= config.HitTime)
            {
                // 执行伤害逻辑 (核心判定)
                bool hitSuccess = TryApplyDamage(owner);
                
                // 切换到后摇阶段
                currentPhase = AttackPhase.Recovery;
                
                // 如果是服务器逻辑，这里可以额外广播 "AttackImpact" 事件
            }

            return NodeStatus.Running;
        }

        // --- 子状态：后摇处理 ---
        private NodeStatus HandleRecoveryState(Entity owner)
        {
            // A. 检查打断 (走A机制)
            // 如果配置允许取消后摇，且玩家有移动输入 -> 视为攻击成功并提前结束
            if (config.CanCancelRecovery && inputComp.HasMoveInput())
            {
                Reset();
                return NodeStatus.Success; 
            }

            // B. 检查动作是否自然结束
            if (timer >= config.TotalDuration)
            {
                currentPhase = AttackPhase.Finished;
                // 下一帧会返回 Success
            }

            return NodeStatus.Running;
        }

        // --- 核心功能：伤害判定 (二次校验) ---
        private bool TryApplyDamage(Entity owner)
        {
            var target = dataComp.GetTargetEntity();

            // 1. 目标校验：存在且存活
            if (target == null) return false;
            var targetCombat = target.GetComponent<CombatComponent>();
            if (targetCombat == null || !targetCombat.IsAlive) return false;

            // 2. 距离校验：防止攻击发出那一刻目标瞬移走了
            // 允许一个小的缓冲距离 (Buffer)，比如 0.5米
            float dist = Vector3.Distance(owner.Position, target.Position);
            if (dist > config.AttackRange + 0.5f)
            {
                return false; // 打空了 (Miss)
            }

            // 3. 真正扣血
            combatComp.ExecuteDamage(target, config);
            return true;
        }

        // --- 工具方法：重置 ---
        private void Reset()
        {
            currentPhase = AttackPhase.None;
            timer = 0f;
        }
    }
}
```

---

### 3. 组件层：支持逻辑

你需要确保你的组件能提供上述节点所需的数据。

**InputComponent (输入组件)**
这个组件不仅用于玩家，也可以用于 AI（AI 的寻路意图也是一种 Input）。

```csharp
public class InputComponent : Component
{
    private Vector2 moveDirection;
    
    // 玩家控制器调用此方法，或者 AI Pathfinding 调用此方法
    public void SetMoveInput(Vector2 dir) { moveDirection = dir; }
    
    public bool HasMoveInput()
    {
        return moveDirection.sqrMagnitude > 0.01f;
    }
}
```

**CombatComponent (战斗组件)**
负责存储配置和执行最终的数值修改。

```csharp
public class CombatComponent : Component
{
    public bool IsAlive { get; private set; } = true;
    public bool IsStunned { get; private set; } = false;

    // 存储技能配置
    private Dictionary<string, AbilityConfig> abilities;

    public AbilityConfig GetAbilityConfig(string id)
    {
        return abilities.ContainsKey(id) ? abilities[id] : null;
    }

    public void ExecuteDamage(Entity target, AbilityConfig ability)
    {
        // 这里进行最终的数值计算：(攻击 - 防御) 等
        target.GetComponent<CombatComponent>().TakeDamage(10);
        
        Debug.Log($"[逻辑层] {Owner.ID} 对 {target.ID} 造成了伤害");
    }

    public void TakeDamage(float amount)
    {
        // 扣血逻辑...
    }
}
```

---

### 4. 方案运行流程演示

假设配置：`HitTime = 0.5s`, `TotalDuration = 1.0s`, `CanCancelRecovery = true`。

#### 情景一：正常攻击流程
1.  **T=0.0s**: 节点进入。`State = WindUp`。动画开始播放。
2.  **T=0.2s**: 节点 Update。玩家没操作。继续 WindUp。
3.  **T=0.5s**: 节点 Update。`timer >= HitTime`。
    *   执行 `TryApplyDamage` -> 扣除目标 HP。
    *   `State` 变为 `Recovery`。
4.  **T=0.7s**: 节点 Update。玩家没操作。继续 Recovery。
5.  **T=1.0s**: 节点 Update。`timer >= TotalDuration`。
    *   `State` 变为 `Finished`。
6.  **T=1.0s+**: 节点返回 `Success`。行为树继续执行下一个节点。

#### 情景二：前摇被取消 (假动作)
1.  **T=0.0s**: 开始攻击。
2.  **T=0.3s**: 玩家按下了移动键 (`InputComp.HasMoveInput()` 为真)。
3.  **节点逻辑**:
    *   检测到 `WindUp` 状态下有输入。
    *   返回 `NodeStatus.Failure`。
    *   重置 Timer。
4.  **行为树**: 收到 Failure，攻击节点结束。下一帧执行 `MoveNode`。
    *   *结果*：伤害没有发生，动画被打断（切到移动动画）。

#### 情景三：走A (取消后摇)
1.  **T=0.0s**: 开始攻击。
2.  **T=0.5s**: 伤害触发。`State` 变为 `Recovery`。
3.  **T=0.6s**: 玩家按下移动键。
4.  **节点逻辑**:
    *   检测到 `Recovery` 状态下有输入，且 `CanCancelRecovery` 为真。
    *   返回 `NodeStatus.Success` (判定为攻击已完成)。
5.  **行为树**: 立即切到 `MoveNode`。
    *   *结果*：伤害打出来了，但省去了 0.4秒 的发呆时间。

### 总结

这套设计的优点在于：
1.  **完全解耦表现**：不依赖 Animator 的状态，服务器也能跑。
2.  **确定性**：时间轴是配置写死的，不会因为卡顿导致多扣血或少扣血。
3.  **灵活性**：通过修改配置里的 `CanCancel` 布尔值，你可以轻松配出“手感极好”的战士（可以走A），或者“笨重”的巨人（必须等动作播完）。