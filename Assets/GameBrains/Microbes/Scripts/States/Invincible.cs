using GameBrains.Common.FiniteStateMachine;
using GameBrains.Common.Messaging;
using GameBrains.Microbes.Scripts.Entities;
using GameBrains.Microbes.Scripts.Messaging;
using UnityEngine;

namespace GameBrains.Microbes.Scripts.States
{
    /// <summary>
    /// Invincible state. Executing in the invincibility state that makes it impossible to kill the microbe for a set time
    /// </summary>
    public class Invincible : State<Microbe>
    {
        private static Invincible instance;

        /// <summary>
        /// Prevents a default instance of the Invincible class from being created.
        /// </summary>
        private Invincible()
        {
            if (null != instance)
            {
                Debug.LogError("Singleton already created.");
                return;
            }

            instance = this;
        }

        /// <summary>
        /// Gets the accessors for the <see cref="Invincible"/> singleton instance.
        /// </summary>
        public static Invincible Instance
        {
            get
            {
                if (null == instance)
                {
                    new Invincible();
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
            microbe.UpdateStateDisplay("Invincible");
        }

        /// <summary>
        /// This is the state's normal update function.
        /// </summary>
        /// <param name="microbe">
        /// The game object associated with this state.
        /// </param>
        public override void Execute(Microbe microbe)
        {
            microbe.UpdateInvincibilityTimer();
        }

        /// <summary>
        /// This will execute when the state is exited.
        /// </summary>
        /// <param name="microbe">
        /// The game object associated with this state.
        /// </param>
        public override void Exit(Microbe microbe)
        {
            microbe.LoseInvincibility();
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