using MH.ActorControllers;
using UnityEngine;

namespace MH
{
    /// <summary>
    /// ゲームに必要な各種計算を行うクラス
    /// </summary>
    public static class Calculator
    {
        public static DamageData GetDamageData(
            int weaponStrength,
            int motionPower,
            float partRate,
            int criticalRate,
            Actor receiveActor,
            Vector3 attackPosition,
            Define.PartType partType
        )
        {
            var damage = (weaponStrength * motionPower) * partRate;
            var isCritical = (criticalRate * 0.01f) > Random.value;
            if (isCritical)
            {
                damage *= 1.2f;
            }
            var isGuardSuccess = receiveActor.GuardController.IsSuccess(attackPosition);
            if (isGuardSuccess)
            {
                damage *= 0.2f;
            }

            return new DamageData
            {
                damage = Mathf.FloorToInt(damage),
                receiveActor = receiveActor,
                partType = partType,
                isCritical = isCritical,
                isGuardSuccess = isGuardSuccess
            };
        }
    }
}
