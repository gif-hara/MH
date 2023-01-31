using UnityEngine;

namespace MH
{
    /// <summary>
    ///
    /// </summary>
    public static class Calculator
    {
        public static DamageData GetDamageData(int weaponStrength, int motionPower, float partRate)
        {
            return new DamageData
            {
                damage = Mathf.FloorToInt((weaponStrength * motionPower) * partRate)
            };
        }
    }
}
