# Archer 射手实体 - 实现总结

## 文件清单

### 新增文件

1. **EntityType.cs** (修改)
   - 添加 `Archer = 2` 实体类型枚举

2. **CheckDangerZoneNode.cs**
   - 路径: `Assets/Scripts/EntityModule/BehaviorTree/Nodes/Condition/`
   - 功能: 检查目标是否进入危险区域
   - 参数: `dangerRange` (危险距离)

3. **FleeFromTargetNode.cs**
   - 路径: `Assets/Scripts/EntityModule/BehaviorTree/Nodes/Action/`
   - 功能: 从目标逃跑，自动计算逃离方向
   - 参数: `fleeDistance` (逃跑距离，默认3格)
   - 特性: 智能路径验证、附近可走格子搜索

4. **FindEnemyNode.cs**
   - 路径: `Assets/Scripts/EntityModule/BehaviorTree/Nodes/Action/`
   - 功能: 通用敌人查找节点，支持不同敌对关系
   - 特性: 自动判定敌对关系（Archer vs Chaser）

5. **GameInitializer.cs** (修改)
   - 添加 `RegisterArcher()` 方法
   - 在 `RegisterBaseEntities()` 中调用

6. **EntitySystemTest.cs** (修改)
   - 添加 `archerCount` 和 `archerStartPos` 字段
   - 在 `CreateTestEntities()` 中添加Archer创建逻辑
   - 在 `EntityPrefabPaths` 字典中添加Archer映射

7. **Archer.MD**
   - 路径: `Assets/Scripts/ReadMe/`
   - 完整的Archer实体文档

### Meta文件
- CheckDangerZoneNode.cs.meta
- FleeFromTargetNode.cs.meta
- FindEnemyNode.cs.meta
- Archer.MD.meta

## 实现特性

### 行为逻辑（满足需求）

✅ **需求1**: 攻击距离a=5格，危险距离b=2格
- 在攻击范围内攻击
- 敌人进入危险距离时逃跑

✅ **需求2**: 攻击和被攻击时无法立刻执行逃跑
- 通过 `CombatComponent.CanMove` 属性控制
- `AttackState != Idle` 时返回false
- `HitState != None` 时返回false

✅ **需求3**: 优先级为逃跑>攻击
- 行为树使用 `SelectorNode` 实现优先级
- 逃跑序列在攻击序列之前

### 架构遵循（满足ReadMe要求）

✅ **组合优于继承**: Entity只是组件容器

✅ **注册表模式**: 
- 使用 `EntityDefinition` 和 `EntityFactory`
- 支持Mod扩展（PostBuildHook）

✅ **MVC分层**:
- Model: `CombatData`
- Controller: `CombatComponent`
- Command: `PerformAttackNode`, `FleeFromTargetNode`
- View: `CombatViewComponent`, `AnimationComponent`

✅ **迪米特法则**: 节点只与必要组件交互

✅ **单一职责**: 每个节点职责明确
- `CheckDangerZoneNode`: 只负责距离判定
- `FleeFromTargetNode`: 只负责逃跑逻辑
- `FindEnemyNode`: 只负责敌人查找

✅ **开闭原则**: 
- 通过注册扩展，无需修改现有代码
- 新增节点完全独立

## 行为树结构

```
SelectorNode (根节点)
├── SequenceNode (逃跑序列 - 优先级1)
│   ├── CheckIsAliveNode
│   ├── HasValidTargetNode
│   ├── CheckDangerZoneNode (危险距离 ≤ 2格)
│   └── FleeFromTargetNode (逃跑4格)
├── SequenceNode (攻击序列 - 优先级2)
│   ├── CheckIsAliveNode
│   ├── HasValidTargetNode
│   ├── CheckDistanceNode (攻击距离 ≤ 5格)
│   ├── CheckCooldownNode
│   └── PerformAttackNode
├── FindEnemyNode (寻找敌人 - 优先级3)
└── IdleNode (待机 - 优先级4)
```

## 配置参数

```csharp
EntityType: Archer
MaxHealth: 80
AttackPower: 20
AttackRange: 5.0f
AttackCooldown: 2.0f
MoveSpeed: 3.0f
DetectionRange: 15.0f
DangerRange: 2.0f
FleeDistance: 4.0f
```

## 使用方式

### 通过代码创建
```csharp
Entity archer = EntityFactory.Create("Archer", new Vector2Int(8, 8), gridManager);
```

### 通过测试场景创建
1. 调整 `EntitySystemTest.archerCount` 和 `archerStartPos`
2. 运行场景或执行 "创建测试实体" 菜单

## 编译注意事项

如果遇到编译错误提示找不到新节点类型：
1. Unity可能需要时间重新编译
2. 尝试手动重新编译（Assets > Reimport All）
3. 检查.meta文件是否正确生成
4. 重启Unity编辑器

## 测试建议

### 场景1: 风筝战术测试
- 创建1个Archer和1个Chaser
- Archer初始位置: (8, 8)
- Chaser初始位置: (2, 2)
- 观察: Archer应该在Chaser靠近到2格内时逃跑

### 场景2: 远程输出测试
- 创建1个Archer和1个Target
- 距离设置在3-5格之间
- 观察: Archer应该持续攻击，不逃跑

### 场景3: 硬直测试
- 让Chaser攻击Archer
- 观察: Archer受击硬直时不能立刻逃跑
- 观察: Archer攻击动画播放时不能逃跑

## 后续优化建议

1. **Luban配置表集成**: 将硬编码参数移到配置表
2. **专用预制体**: 创建Archer独特的视觉资源
3. **AI增强**: 
   - 预判敌人移动方向
   - 向友军方向逃跑
   - 利用地形掩体
4. **技能系统**: 添加特殊技能（爆头、穿透等）

---

**实现时间**: 2026-01-09  
**符合标准**: Framework.MD, MVC.MD, EntityNew.MD, AnimationStateMachine.MD  
**测试状态**: 待Unity编译完成后测试

