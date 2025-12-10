using EntityModule;
using EntityModule.Component;
using UnityEditor;

namespace Editor
{
    /// <summary>
    /// Entity的自定义Inspector编辑器（显示组件信息）
    /// </summary>
    [CustomEditor(typeof(Entity))]
    [CanEditMultipleObjects]
    public class EntityEditor : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            // 绘制默认Inspector
            DrawDefaultInspector();

            Entity entity = (Entity)target;
            if (entity == null) return;

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("组件信息", EditorStyles.boldLabel);

            // 显示战斗组件信息
            var combatComponent = entity.GetComponent<CombatComponent>();
            if (combatComponent != null)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField("战斗组件", EditorStyles.boldLabel);
                
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.FloatField("最大血量", combatComponent.MaxHealth);
                EditorGUILayout.FloatField("当前血量", combatComponent.CurrentHealth);
                EditorGUILayout.FloatField("攻击力", combatComponent.AttackPower);
                EditorGUILayout.FloatField("攻击范围", combatComponent.AttackRange);
                EditorGUILayout.Toggle("是否存活", combatComponent.IsAlive);
                
                // 显示攻击CD（如果有）
                if (combatComponent.HasAttackCooldown)
                {
                    EditorGUILayout.FloatField("攻击CD剩余", combatComponent.AttackCooldownRemaining);
                }
                EditorGUI.EndDisabledGroup();
                
                EditorGUILayout.EndVertical();
            }

            // 显示数据组件信息
            var dataComponent = entity.GetComponent<DataComponent>();
            if (dataComponent != null)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField("数据组件", EditorStyles.boldLabel);
                
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.EnumPopup("实体类型", dataComponent.EntityType);
                EditorGUILayout.FloatField("检测范围", dataComponent.DetectionRange);
                EditorGUILayout.IntField("目标ID", dataComponent.TargetEntityId);
                EditorGUI.EndDisabledGroup();
                
                EditorGUILayout.EndVertical();
            }

            // 显示移动组件信息
            var movementComponent = entity.GetComponent<MovementComponent>();
            if (movementComponent != null)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField("移动组件", EditorStyles.boldLabel);
                
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.FloatField("移动速度", movementComponent.MoveSpeed);
                EditorGUILayout.Toggle("正在移动", movementComponent.IsMoving);
                EditorGUILayout.Toggle("有路径", movementComponent.HasPath);
                EditorGUI.EndDisabledGroup();
                
                EditorGUILayout.EndVertical();
            }
        }
    }
}

