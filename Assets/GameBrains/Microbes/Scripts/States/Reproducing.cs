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
using UnityEngine;

#endregion Copyright © ThotLab Games 2011. Licensed under the terms of the Microsoft Reciprocal Licence (Ms-RL).

namespace GameBrains.Microbes.Scripts.States
{
    /// <summary>
    /// Reproducing state. Executing in the mating state results in "contributions" from parents.
    /// This state should spawn zero, one, or more new microbes with characteristics determined by
    /// the parents. Be creative.
    /// TODO: Fill in the details.
    /// </summary>
    public class Reproducing : State<Microbe>
    {
        private static Reproducing instance;

        /// <summary>
        /// Prevents a default instance of the Reproducing class from being created.
        /// </summary>
        private Reproducing()
        {
            if (null != instance)
            {
                Debug.LogError("Singleton already created.");
                return;
            }

            instance = this;
        }

        /// <summary>
        /// Gets the accessor for the <see cref="Reproducing"/> singleton instance.
        /// </summary>
        public static Reproducing Instance
        {
            get
            {
                if (null == instance)
                {
                    new Reproducing();
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
            microbe.UpdateStateDisplay("Reproducing");
        }

        /// <summary>
        /// This is the state's normal update function.
        /// </summary>
        /// <param name="microbe">
        /// The game object associated with this state.
        /// </param>
        public override void Execute(Microbe microbe)
		{
			//// adjust for desired reproduction radius (assume sphere so scale x = scale y = scale z).
            var radius = microbe.transform.localScale.x / 2.0f;

            List<Microbe> nearbyMicrobes = new List<Microbe>();

            // Find all microbes in a certain radius that match any of the types of possible mates.
            foreach (Microbe existingMicrobe in EntityManager.FindAll<Microbe>())
            {
				if (microbe != existingMicrobe && (existingMicrobe.MicrobeType & microbe.MateTypes) != 0 && existingMicrobe.StateMachine.CurrentState == Instance)
                {
                    if (Physics.Raycast(microbe.transform.position, existingMicrobe.transform.position - microbe.transform.position, out RaycastHit hit, radius))
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
                    microbe.AttemptReproduction(nearbyMicrobe);
                }
            }
        }

        /// <summary>
        /// This will execute when the state is exited.
        /// </summary>
        /// <param name="microbe">
        /// The game object associated with this state.
        /// </param>
        public override void Exit(Microbe microbe)
        {
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