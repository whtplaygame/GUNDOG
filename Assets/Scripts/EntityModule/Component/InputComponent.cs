using UnityEngine;

namespace EntityModule.Component
{
    /// <summary>
    /// 输入组件
    /// 负责接收玩家或AI的移动/操作意图
    /// </summary>
    public class InputComponent : Component
    {
        private Vector2 moveDirection;
        private bool isAttackPressed;

        public Vector2 MoveDirection => moveDirection;
        public bool IsAttackPressed => isAttackPressed;

        public void SetMoveInput(Vector2 dir)
        {
            moveDirection = dir;
        }

        public void SetAttackInput(bool pressed)
        {
            isAttackPressed = pressed;
        }

        public bool HasMoveInput()
        {
            return moveDirection.sqrMagnitude > 0.01f;
        }

        public override void TickLogic(float deltaTime)
        {
            // 输入组件通常由外部系统（PlayerController或AI）每帧更新
            // 这里只需要重置瞬时状态（如果有的话）
        }
    }
}

