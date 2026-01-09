# 🏹 Archer 射手 - 快速参考

## 核心概念
远程攻击单位，保持距离输出，敌人靠近时逃跑

## 关键参数
| 参数 | 值 | 说明 |
|------|-----|------|
| 攻击距离 (a) | 5格 | 远程攻击范围 |
| 危险距离 (b) | 2格 | 触发逃跑的距离 |
| 逃跑距离 | 4格 | 每次逃跑移动距离 |
| 生命值 | 80 | 脆皮单位 |
| 攻击力 | 20 | 高伤害输出 |
| 攻击CD | 2秒 | 中等攻速 |
| 移动速度 | 3.0 | 中等速度 |

## 行为优先级
```
1️⃣ 逃跑 > 2️⃣ 攻击 > 3️⃣ 寻敌 > 4️⃣ 待机
```

## 核心限制
- ❌ 攻击时不能逃跑
- ❌ 受击硬直时不能逃跑
- ✅ 只有完全空闲时才能逃跑

## 新增节点
1. **CheckDangerZoneNode** - 检查危险区域
2. **FleeFromTargetNode** - 执行逃跑
3. **FindEnemyNode** - 查找敌人

## 创建代码
```csharp
Entity archer = EntityFactory.Create("Archer", new Vector2Int(8, 8), gridManager);
```

## 战术特点
- 🎯 风筝战术：边撤边打
- 🛡️ 需要保护：血量低，怕近战
- 💥 高输出：远程高伤害
- 🏃 灵活机动：保持安全距离

## 文件位置
- 节点: `EntityModule/BehaviorTree/Nodes/`
- 注册: `EntityModule/GameInitializer.cs`
- 文档: `Assets/Scripts/ReadMe/Archer.MD`

## 架构合规 ✅
- ✅ MVC分层
- ✅ 组合优于继承
- ✅ 注册表模式
- ✅ 迪米特法则
- ✅ 单一职责
- ✅ 开闭原则

