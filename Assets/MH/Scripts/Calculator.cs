using UnityEngine;

namespace MH
{
    /// <summary>
    ///
    /// </summary>
    public static class Calculator
    {
        public static int GetDamage(int weaponStrength, int motionPower, float partRate)
        {
            return Mathf.FloorToInt((weaponStrength * motionPower) * partRate);
        }
    }
}
