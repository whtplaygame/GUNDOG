# Chaser 目标更新说明

## 修改内容

已将 Chaser 的目标查找从 `FindTargetNode` 更改为 `FindEnemyNode`。

## 行为变化

### 修改前
- Chaser 只查找 Target 类型的实体
- 使用 `FindTargetNode`，硬编码查找逻辑

### 修改后
- Chaser 查找所有敌对实体（Target 和 Archer）
- 使用 `FindEnemyNode`，支持灵活的敌对关系配置
- **优先攻击最近的敌人**，无论是 Archer 还是 Target

## 敌对关系配置

在 `FindEnemyNode` 中定义的敌对关系：

```
Chaser 的敌人:
  - Target ✅
  - Archer ✅

Archer 的敌人:
  - Chaser ✅

Target 的敌人:
  - Chaser ✅
```

## 战术效果

### 场景示例

**场景1: Chaser vs Archer**
1. Chaser 发现 Archer（距离 < 10格探测范围）
2. Chaser 追逐 Archer
3. Archer 在 Chaser 靠近到 2格时逃跑
4. Chaser 继续追逐
5. Archer 逃到 5格左右时转身攻击

**场景2: Chaser 在 Archer 和 Target 之间**
1. Chaser 计算到 Archer 和 Target 的距离
2. 选择距离最近的敌人作为目标
3. 追逐并攻击该目标

**场景3: 多个 Chaser vs 1个 Archer**
1. 所有 Chaser 都会以 Archer 为目标
2. Archer 需要不断风筝逃跑
3. 体现 Archer 作为脆皮远程的生存压力

## 优势

1. **更灵活**: 不再硬编码目标类型
2. **更真实**: 追逐者会攻击所有敌对单位
3. **更有趣**: 战场动态更复杂，Archer 有真实威胁
4. **易扩展**: 未来添加新单位类型，只需在 `FindEnemyNode` 中配置敌对关系

## 代码位置

- 修改文件: `Assets/Scripts/EntityModule/GameInitializer.cs`
- 使用节点: `FindEnemyNode.cs`
- 第 58 行: `var findEnemyNode = new FindEnemyNode();`
- 第 86 行: 添加到行为树选择器

## 测试建议

### 推荐测试配置
```
EntitySystemTest 设置:
- chaserCount: 2
- archerCount: 1
- chaserStartPos: (2, 2)
- archerStartPos: (8, 8)
```

### 预期行为
1. 2个 Chaser 会同时发现并追逐 Archer
2. Archer 在 Chaser 靠近时逃跑
3. Archer 拉开距离后反击
4. 观察风筝战术和追逐动态

---

**更新时间**: 2026-01-09  
**影响范围**: Chaser 实体行为树  
**兼容性**: 完全向后兼容，不影响其他实体

