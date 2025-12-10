using UnityEngine;

namespace EntityModule.LOD
{
    /// <summary>
    /// LOD系统（Level of Detail System）
    /// 根据距离动态调整更新频率
    /// </summary>
    public static class LODSystem
    {
        // LOD距离阈值
        private const float HIGH_DETAIL_DISTANCE = 20f;
        private const float MEDIUM_DETAIL_DISTANCE = 50f;

        // 更新频率
        private const int MEDIUM_UPDATE_INTERVAL = 5;  // 每5帧更新一次

        /// <summary>
        /// 计算LOD级别
        /// </summary>
        public static EntityLODLevel CalculateLODLevel(Vector3 entityPos, Vector3 referencePos)
        {
            float distance = Vector3.Distance(entityPos, referencePos);

            if (distance < HIGH_DETAIL_DISTANCE)
            {
                return EntityLODLevel.High;
            }
            else if (distance < MEDIUM_DETAIL_DISTANCE)
            {
                return EntityLODLevel.Medium;
            }
            else
            {
                return EntityLODLevel.Low;
            }
        }

        /// <summary>
        /// 检查是否应该更新（根据LOD级别和帧计数）
        /// </summary>
        public static bool ShouldUpdate(EntityLODLevel lodLevel, int frameCount)
        {
            switch (lodLevel)
            {
                case EntityLODLevel.High:
                    return true;  // 每帧更新
                case EntityLODLevel.Medium:
                    return frameCount % MEDIUM_UPDATE_INTERVAL == 0;  // 每5帧更新
                case EntityLODLevel.Low:
                    return false;  // 不更新
                default:
                    return false;
            }
        }

        /// <summary>
        /// 获取参考位置（通常是玩家或摄像机位置）
        /// </summary>
        public static Vector3 GetReferencePosition()
        {
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                return mainCamera.transform.position;
            }
            return Vector3.zero;
        }
    }
}

