using UnityEngine;

namespace EntityModule.Data
{
    /// <summary>
    /// 战斗纯数据对象（Model层）
    /// 只包含属性，不包含逻辑，用于存档和配置表
    /// </summary>
    [System.Serializable]
    public class CombatData
    {
        // === 静态属性（配置） ===
        [Header("攻击配置")]
        public float AttackRange = 1f;           // 攻击范围
        public float AttackCooldown = 1f;        // 攻击冷却时间
        public float AttackPower = 10f;          // 攻击力

        [Header("攻击时间轴配置")]
        public float PrepareTime = 0.1f;        // 准备阶段时间（可选，用于某些技能）
        public float WindUpTime = 0.3f;         // 前摇时间（准备 -> 伤害判定）
        public float ImpactTime = 0.1f;         // 伤害判定持续时间
        public float RecoveryTime = 0.4f;        // 后摇时间（伤害判定后 -> 结束）
        
        // 计算属性：伤害判定点（前摇结束点）
        public float DamagePointTime => PrepareTime + WindUpTime;
        
        // 计算属性：动作总时长
        public float TotalDuration => PrepareTime + WindUpTime + ImpactTime + RecoveryTime;

        [Header("受击配置")]
        public float HitStunDuration = 0.5f;     // 受击硬直时间

        // === 动态属性（存档） ===
        [Header("生命值")]
        public float MaxHP = 100f;
        public float CurrentHP = 100f;

        /// <summary>
        /// 是否存活
        /// </summary>
        public bool IsAlive => CurrentHP > 0f;

        /// <summary>
        /// 复制构造函数
        /// </summary>
        public CombatData Clone()
        {
            return new CombatData
            {
                AttackRange = AttackRange,
                AttackCooldown = AttackCooldown,
                AttackPower = AttackPower,
                PrepareTime = PrepareTime,
                WindUpTime = WindUpTime,
                ImpactTime = ImpactTime,
                RecoveryTime = RecoveryTime,
                HitStunDuration = HitStunDuration,
                MaxHP = MaxHP,
                CurrentHP = CurrentHP
            };
        }
    }
}

