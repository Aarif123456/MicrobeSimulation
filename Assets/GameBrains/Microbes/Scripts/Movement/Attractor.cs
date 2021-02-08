#region Copyright � ThotLab Games 2011. Licensed under the terms of the Microsoft Reciprocal Licence (Ms-RL).

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

#endregion Copyright � ThotLab Games 2011. Licensed under the terms of the Microsoft Reciprocal Licence (Ms-RL).

using System.Collections.Generic;
using System.Linq;
using GameBrains.Common.Managers;
using GameBrains.Microbes.Scripts.Entities;
using UnityEngine;

namespace GameBrains.Microbes.Scripts.Movement
{
    public class Attractor : MonoBehaviour
    {
        public float strength = 2;

        /// <summary>
        /// The radius of the attraction.
        /// </summary>
        public float radius = 400;

        protected readonly List<Microbe> oldNearbyMicrobes = new List<Microbe>();

        protected readonly List<Microbe> nearbyMicrobes = new List<Microbe>();

        protected List<Microbe> noLongerPullingMicrobes;

        protected Microbe microbe;

        /// <summary>
        /// Gets or sets the strength of the attraction.
        /// </summary>
        public float Strength
        {
            get => strength;

            set => strength = value;
        }

        /// <summary>
        /// Which types of objects get attracted.
        /// </summary>
        public MicrobeTypes AttractTypes { get; set; }

        public void Awake()
        {
            microbe = gameObject.GetComponent<Microbe>();
        }

        public void Update()
        {
            // Keep track of which microbes were nearby last tick.
            oldNearbyMicrobes.Clear();
            oldNearbyMicrobes.AddRange(nearbyMicrobes);

            nearbyMicrobes.Clear();

            // Find all microbes that match all of the attract types in a certain radius.
            foreach (Microbe existingMicrobe in EntityManager.FindAll<Microbe>())
            {
                if (microbe != existingMicrobe &&
                    (Vector3.Distance(existingMicrobe.transform.position, transform.position) <= radius) &&
                    ((existingMicrobe.MicrobeType & AttractTypes) != 0))
                {
                    nearbyMicrobes.Add(existingMicrobe);
                }
            }

            // Now tell the microbes that are within our attraction radius that we're attracting them.
            foreach (Microbe nearbyMicrobe in nearbyMicrobes)
            {
                // can't attract yourself!
                if (nearbyMicrobe.gameObject == gameObject)
                {
                    continue;
                }

                Motor motor = nearbyMicrobe.Motor;
                if (motor != null)
                {
                    // tell the object who we are, and how hard we're pulling.
                    motor.AddPull(microbe, Strength);
                }
            }

            // Remember how we kept track of which microbes we were pulling last update?
            // Well, if we're not going to pull them anymore, we need to tell them.
            noLongerPullingMicrobes
                = oldNearbyMicrobes.Where(a => !nearbyMicrobes.Contains(a)).ToList();
            foreach (Microbe oldMicrobe in noLongerPullingMicrobes)
            {
                if (oldMicrobe != null)
                {
                    Motor motor = oldMicrobe.Motor;
                    if (motor != null)
                    {
                        // it's a microbe we can affect...
                        motor.RemovePull(microbe);
                    }
                }
            }
        }
    }
}