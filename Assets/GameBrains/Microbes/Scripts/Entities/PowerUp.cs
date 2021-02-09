using GameBrains.Common.Entities;
using GameBrains.Microbes.Scripts.Movement;
using UnityEngine;

namespace GameBrains.Microbes.Scripts.Entities{
    public enum PowerUpType{
        Invincible=0
    }
    public class PowerUp : Entity
    {
        /* By default the power up type is invincibility */
        public PowerUpType type=PowerUpType.Invincible;
        public Motor Motor { get; set; }

        public override void Awake()
        {
            base.Awake();
            Motor = gameObject.GetComponent<Motor>();
        }
    }
}

