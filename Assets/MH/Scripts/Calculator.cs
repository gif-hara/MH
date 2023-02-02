using UnityEngine;

namespace MH
{
    /// <summary>
    /// ゲームに必要な各種計算を行うクラス
    /// </summary>
    public static class Calculator
    {
        public static int GetDamageData(int weaponStrength, int motionPower, float partRate, int criticalRate)
        {
            var damage = (weaponStrength * motionPower) * partRate;
            var isCritical = (criticalRate * 0.01f) > Random.value;
            if(isCritical)
            {
                damage *= 1.2f;
            }
            
            return Mathf.FloorToInt(damage);
        }
    }
}
