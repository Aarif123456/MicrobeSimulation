using GameBrains.Common.FiniteStateMachine;
using GameBrains.Common.Managers;
using GameBrains.Common.Messaging;
using GameBrains.Microbes.Scripts.Entities;
using GameBrains.Microbes.Scripts.Messaging;
using GameBrains.Microbes.Scripts.Collectible;
using GameBrains.Microbes.Scripts.Movement;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GameBrains.Microbes.Scripts.States
{
    /// <summary>
    /// SeekingPowerUp state. Executing in this state causes the microbe to chase a power-up 
    /// </summary>
    public class SeekingPowerUp : State<Microbe>
    {
        /* Singleton state object */
        private static SeekingPowerUp instance;
        /* strength microbe can pull on power-up 
        * NOTE: It might make sense to change to depend on microbe
        */
        public float strength = 2;
        /* Used to control which power up we pull on */
        protected readonly List<PowerUp> nearbyPowerUps = new List<PowerUp>();

        /// <summary>
        /// Gets or sets the strength of the attraction.
        /// </summary>
        public float Strength
        {
            get => strength;

            set => strength = value;
        }
        /// <summary>
        /// Prevents a default instance of the SeekingPowerUp class from being created
        /// </summary>
        private SeekingPowerUp()
        {
            if (null != instance)
            {
                Debug.LogError("Singleton already created.");
                return;
            }

            instance = this;
        }

        /// <summary>
        /// Gets the accessors for the <see cref="SeekingPowerUp"/> singleton instance.
        /// </summary>
        public static SeekingPowerUp Instance
        {
            get
            {
                if (null == instance)
                {
                    new SeekingPowerUp();
                }

                return instance;
            }
        }

        /// <summary>
        /// This will execute when the state is entered.
        /// </summary>
        /// <param name="microbe">
        /// The game object associated with this state.
        /// </param>
        public override void Enter(Microbe microbe)
        {
            microbe.UpdateStateDisplay("SeekingPowerUp");
        }

        private void StopPullingPowerUp(Microbe microbe, 
                                        List<PowerUp> noLongerPullingPowerUps){
            foreach (PowerUp oldPowerUp in noLongerPullingPowerUps)
            {
                if (oldPowerUp != null)
                {
                    Motor motor = oldPowerUp.Motor;
                    if (motor != null)
                    {
                        motor.RemovePull(microbe);
                    }
                }
            }
        }
        /// <summary>
        /// This is the state's normal update function.
        /// </summary>
        /// <param name="microbe">
        /// The game object associated with this state.
        /// </param>
        public override void Execute(Microbe microbe)
        {
            // Whens a young microbe has to eat then he needs to eat ...
            if (microbe.IsHungry)
            {
                microbe.StateMachine.ChangeState(SeekingFood.Instance);
                return;
            }

            /* Store what power up we were already pulling on */
            List<PowerUp> oldNearbyPowerUps = new List<PowerUp>();
            oldNearbyPowerUps.AddRange(nearbyPowerUps);

            nearbyPowerUps.Clear();

            //// adjust for desired seeking area radius (assume sphere so scale x = scale y = scale z).
            var radius = microbe.transform.localScale.x *10;

            RaycastHit hit;

            // Find all microbes in a certain radius that match any of the food types we eat.
            foreach (GameObject powerObject in GameObject.FindGameObjectsWithTag("collectible"))
            {
                PowerUp existingPowerUp = powerObject.GetComponent(typeof(PowerUp)) as PowerUp;
                if (Physics.Raycast(microbe.transform.position, powerObject.transform.position - microbe.transform.position, out hit, radius))
                {
                    if (hit.transform == powerObject.transform)
                    {
                        nearbyPowerUps.Add(existingPowerUp);
                    }
                }
                
            }

            if (nearbyPowerUps.Count > 0)
            {
                foreach (PowerUp nearbyPowerUp in nearbyPowerUps)
                {

                   Motor motor = nearbyPowerUp.Motor;
                   if (motor != null)
                   {
                       // tell the object who we are, and how hard we're pulling.
                       motor.AddPull(microbe, Strength);
                   }
                }
            }
            /* If no power up nearby then go into sleeping */
            else{
                microbe.StateMachine.ChangeState(Sleeping.Instance);
            }

            List<PowerUp> noLongerPullingPowerUps =  oldNearbyPowerUps.Where(a => !nearbyPowerUps.Contains(a)).ToList();
            StopPullingPowerUp(microbe, noLongerPullingPowerUps);
        }

        /// <summary>
        /// This will execute when the state is exited.
        /// </summary>
        /// <param name="microbe">
        /// The game object associated with this state.
        /// </param>
        public override void Exit(Microbe microbe)
        {
            StopPullingPowerUp(microbe, nearbyPowerUps);
        }

        /// <summary>
        /// This executes if the agent receives a message from the message dispatcher.
        /// </summary>
        /// <param name="microbe">
        /// The game object associated with this state.
        /// </param>
        /// <param name="msg">
        /// The message.
        /// </param>
        /// <returns>
        /// True if the message was handled. Otherwise, false.
        /// </returns>
        public override bool OnMessage(Microbe microbe, Telegram msg)
        {
            // send msg to global message handler
            return false;
        }
    }
}