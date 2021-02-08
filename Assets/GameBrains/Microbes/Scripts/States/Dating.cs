#region Copyright © ThotLab Games 2011. Licensed under the terms of the Microsoft Reciprocal Licence (Ms-RL).

// Microsoft Reciprocal License (Ms-RL)
//
// This license governs use of the accompanying software. If you use the software, you accept this
// license. If you do not accept the license, do not use the software.
//
// 1. Definitions
// The terms "reproduce," "reproduction," "derivative works," and "distribution" have the same
// meaning here as under U.S. copyright law.
// A "contribution" is the original software, or any additions or changes to the software.
// A "contributor" is any person that distributes its contribution under this license.
// "Licensed patents" are a contributor's patent claims that read directly on its contribution.
//
// 2. Grant of Rights
// (A) Copyright Grant- Subject to the terms of this license, including the license conditions and
// limitations in section 3, each contributor grants you a non-exclusive, worldwide, royalty-free
// copyright license to reproduce its contribution, prepare derivative works of its contribution,
// and distribute its contribution or any derivative works that you create.
// (B) Patent Grant- Subject to the terms of this license, including the license conditions and
// limitations in section 3, each contributor grants you a non-exclusive, worldwide, royalty-free
// license under its licensed patents to make, have made, use, sell, offer for sale, import, and/or
// otherwise dispose of its contribution in the software or derivative works of the contribution in
// the software.
//
// 3. Conditions and Limitations
// (A) Reciprocal Grants- For any file you distribute that contains code from the software (in
// source code or binary format), you must provide recipients the source code to that file along
// with a copy of this license, which license will govern that file. You may license other files
// that are entirely your own work and do not contain code from the software under any terms you
// choose.
// (B) No Trademark License- This license does not grant you rights to use any contributors' name,
// logo, or trademarks.
// (C) If you bring a patent claim against any contributor over patents that you claim are
// infringed by the software, your patent license from such contributor to the software ends
// automatically.
// (D) If you distribute any portion of the software, you must retain all copyright, patent,
// trademark, and attribution notices that are present in the software.
// (E) If you distribute any portion of the software in source code form, you may do so only under
// this license by including a complete copy of this license with your distribution. If you
// distribute any portion of the software in compiled or object code form, you may only do so under
// a license that complies with this license.
// (F) The software is licensed "as-is." You bear the risk of using it. The contributors give no
// express warranties, guarantees or conditions. You may have additional consumer rights under your
// local laws which this license cannot change. To the extent permitted under your local laws, the
// contributors exclude the implied warranties of merchantability, fitness for a particular purpose
// and non-infringement.
using System.Collections.Generic;
using GameBrains.Common.FiniteStateMachine;
using GameBrains.Common.Managers;
using GameBrains.Common.Messaging;
using GameBrains.Microbes.Scripts.Entities;
using GameBrains.Microbes.Scripts.Messaging;
using UnityEngine;

#endregion Copyright © ThotLab Games 2011. Licensed under the terms of the Microsoft Reciprocal Licence (Ms-RL).

namespace GameBrains.Microbes.Scripts.States
{
	/// <summary>
    /// When in this state, the microbe should try attracting a mate of a suitable type. When a
    /// potential mate is close, we should send a LetsMakeABaby message. If a LetsMakeABaby message
    /// is received, we should change to the Reproducing state. Or if a YouAreNotMyType message is
    /// received, either change to the Sleep state (say 50% chance) or seek mate of a different type
    /// (say 40%) or keep trying (say 10%). Note: the above is not a strict requirement, just one
    /// possibility. You might make your microbe seek partners of several types before reproducing
    /// (3 or 4 parents!). You might make the type of microbe that gets spawned depend on the type
    /// of its parents. Use your imagination. Then take a cold shower :-)
    /// TODO: Fill in the details of this state.
    /// </summary>
    public class Dating : State<Microbe>
    {
        private static Dating instance;

        /// <summary>
        /// Prevents a default instance of the Dating class from being created.
        /// </summary>
        private Dating()
        {
            if (null != instance)
            {
                Debug.LogError("Singleton already created.");
                return;
            }

            instance = this;
        }

        /// <summary>
        /// Gets the accessor for the <see cref="Dating"/> singleton instance.
        /// </summary>
        public static Dating Instance
        {
            get
            {
                if (null == instance)
                {
                    new Dating();
                }

                return instance;
            }
        }

        /// <summary>
        /// This will execute when the state is entered.
        /// </summary>
        /// <param name="microbe">
        /// The microbe associated with this state.
        /// </param>
        public override void Enter(Microbe microbe)
        {
            microbe.UpdateStateDisplay("Dating");
        }

        /// <summary>
        /// This is the state's normal update function.
        /// </summary>
        /// <param name="microbe">
        /// The microbe associated with this state.
        /// </param>
        public override void Execute(Microbe microbe)
		{
	        // Seeking food should be a priority for the agent.
	        if(microbe.IsHungry) {
	        	microbe.StateMachine.ChangeState(SeekingFood.Instance);
	        	return;
	        }

			// With some probability, the agent should give up and go back to sleep because it can't find a mate
			// Removed. Between hunger, and the random chance of going back to sleep, this happens too often.
//	        if(Random.value < 0.01) {
//	        	microbe.StateMachine.ChangeState(Sleeping.Instance);
//	        	return;
//	        }


			//// adjust for desired mating radius (assume sphere so scale x = scale y = scale z).
            var radius = microbe.transform.localScale.x / 2.0f;

            List<Microbe> nearbyMicrobes = new List<Microbe>();

            RaycastHit hit;

            // Find all microbes in a certain radius that match any of the types of possible mates.
            foreach (Microbe existingMicrobe in EntityManager.FindAll<Microbe>())
            {
                if (microbe != existingMicrobe && (existingMicrobe.MicrobeType & microbe.MateTypes) != 0)
                {
                    if (Physics.Raycast(microbe.transform.position, existingMicrobe.transform.position - microbe.transform.position, out hit, radius))
                    {
                        if (hit.transform == existingMicrobe.transform)
                        {
                            nearbyMicrobes.Add(existingMicrobe);
                        }
                    }
                }
            }

            if (nearbyMicrobes.Count > 0)
            {
                foreach (Microbe nearbyMicrobe in nearbyMicrobes)
                {
                    // Ask nearby microbes if they want to mate
                    MessageDispatcher.Instance.DispatchMsg(
                        MessageDispatcher.SendMsgImmediately,
                        microbe.ID,
                        nearbyMicrobe.ID,
                        (MessageTypes)MicrobeMessageTypes.LetsMakeABaby,
                        MessageDispatcher.NoAdditionalInfo);
                }
            }

            // adjust attraction radius based one hunger
	        microbe.Attractor.Strength = 20000; // could also adjust strength
	        microbe.Attractor.radius = 500 * microbe.LifeSpan.Age; // the microbe gets more desparate to mate as it ages
	        microbe.Attractor.AttractTypes = microbe.MateTypes; // could change food type


        }

        /// <summary>
        /// This will execute when the state is exited.
        /// </summary>
        /// <param name="microbe">
        /// The microbe associated with this state.
        /// </param>
        public override void Exit(Microbe microbe)
        {
        }

        /// <summary>
        /// This executes if the microbe receives a message from the message dispatcher.
        /// </summary>
        /// <param name="microbe">
        /// The microbe associated with this state.
        /// </param>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <returns>
        /// True if the message was handled. Otherwise, false.
        /// </returns>
        public override bool OnMessage(Microbe microbe, Telegram message)
        {
			switch ((MicrobeMessageTypes)message.Msg)
            {
                case MicrobeMessageTypes.LetsMakeABaby:
                	// Make sure that the microbe that sent the message is a suitable mate
                	// Not sure if "as Microbe" is necessary, I think the <Microbe> accounts for the inheritance
                	Microbe sender = EntityManager.Find<Microbe>(message.Sender);

                	if(sender) {
                		// Make sure it's an eligible mate type
                		if((sender.MicrobeType & microbe.MateTypes) != 0) {
							// Change to the reproduction state
                			//
                			// Note that this approach may lead to the following state change pattern:
                			//		* -> Reproducing -> Reproducing
                			// This would happen if valid mate microbes M1 and M2 both messaged M3 at the same time.
                			// M3 would change M1 and M3 to the reproducing state,
                			// M3 would then change M2 and M3 to the reproducing state.
                			// This causes no issues in the current form
							microbe.StateMachine.ChangeState(Reproducing.Instance);
							sender.StateMachine.ChangeState(Reproducing.Instance);
                		} else {
                			// Tell the other microbe that this won't work out
							MessageDispatcher.Instance.DispatchMsg(
		                        MessageDispatcher.SendMsgImmediately,
		                        microbe.ID,
		                        sender.ID,
		                        (MessageTypes)MicrobeMessageTypes.YouAreNotMyType,
		                        MessageDispatcher.NoAdditionalInfo);
                		}
			            return true;
                	}

                	return false;
            }

            // send msg to global message handler
            return false;
        }
    }
}