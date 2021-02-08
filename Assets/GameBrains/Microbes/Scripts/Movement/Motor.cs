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
using GameBrains.Common.Managers;
using GameBrains.Microbes.Scripts.Entities;
using UnityEngine;

namespace GameBrains.Microbes.Scripts.Movement
{
    [RequireComponent(typeof(Rigidbody))]

    public class Motor : MonoBehaviour
    {
        [SerializeField] float maximumSpeed = 50;

        [SerializeField] float maximumAcceleration = 10;

        readonly Dictionary<Microbe, float> pull = new Dictionary<Microbe, float>();

        Vector2 desiredDelta;
        Vector2 desiredVelocity = Vector2.zero;
        Vector2 netDesiredVelocity = Vector2.zero;

        public void Update()
        {
            //// TODO: This should take elapsed time and time scale into account.

            #region Try commenting this out and try to spot the problem!!!

            var pullerList = new List<Microbe>(pull.Keys);
            foreach (Microbe puller in pullerList)
            {
                //// TODO: When an entity is removed from the entity manager,
                //// it should clean up after itself. ... but it doesn't so
                //// we catch the problem here for now.
                if (EntityManager.Find<Microbe>(puller.ID) == null)
                {
                    pull.Remove(puller);
                }
            }

            #endregion Try commenting this out and try to spot the problem!!!

            netDesiredVelocity = Vector2.zero;

            foreach (Microbe puller in pull.Keys)
            {
                if (puller.transform != null)
                {
                    Vector3 distance3D = puller.transform.position - transform.position;
                    desiredVelocity = new Vector2(distance3D.x, distance3D.z);

                    if (desiredVelocity.sqrMagnitude > 0.001f)
                    {
                        desiredVelocity.Normalize();
                    }

                    // scale by the strength
                    desiredVelocity *= pull[puller];
                    netDesiredVelocity += desiredVelocity;
                }
            }

            // Cap the velocity with the max speed.
            if (netDesiredVelocity.magnitude > maximumSpeed)
            {
                netDesiredVelocity.Normalize();
                netDesiredVelocity *= maximumSpeed;
            }

            // calculate our delta
            Vector3 velocity3D = transform.GetComponent<Rigidbody>().velocity;
            desiredDelta = netDesiredVelocity - new Vector2(velocity3D.x, velocity3D.z);

            // Cap the acceleration with the max speed delta.
            if (desiredDelta.magnitude > maximumAcceleration)
            {
                desiredDelta.Normalize();
                desiredDelta *= maximumAcceleration;
            }

            transform.GetComponent<Rigidbody>().velocity += new Vector3(desiredDelta.x, 0, desiredDelta.y);
        }

        /// <summary>
        /// Add a pull from the given source with the given strength.
        /// </summary>
        /// <param name="source">The source of the pull to added.</param>
        /// <param name="strength">The strength of the added pull.</param>
        public void AddPull(Microbe source, float strength)
        {
            pull[source] = strength;
        }

        /// <summary>
        /// Remove the pull of the given source.
        /// </summary>
        /// <param name="source">The source of the pull to remove.</param>
        public void RemovePull(Microbe source)
        {
            pull.Remove(source);
        }
    }
}