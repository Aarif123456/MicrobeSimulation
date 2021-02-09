using GameBrains.Common.Entities;
using GameBrains.Microbes.Scripts.Movement;
using UnityEngine;

namespace GameBrains.Microbes.Scripts.Entities{
    /* Stores all the different types of power */
    public enum PowerUpType{
        Invincible=0
    }

    public class PowerUp : MonoBehaviour
    {
        /* By default the power up type is invincibility */
        public PowerUpType type=PowerUpType.Invincible;
        public Motor Motor { get; set; }

        public void Awake()
        {
            Motor = gameObject.GetComponent<Motor>();
        }
    }
}

