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

#region Derived from Mat Buckland's Programming Game AI by Example
// Please see Buckland's book for the original C++ code and examples.
#endregion

using GameBrains.Common.FiniteStateMachine;
using GameBrains.Common.Managers;
using GameBrains.Common.Messaging;
using GameBrains.WestWorld.Scripts.Entities;
using GameBrains.WestWorld.Scripts.Messaging;
using UnityEngine;

namespace GameBrains.WestWorld.Scripts.States
{
    /// <summary>
    /// </summary>
    public class GoHomeAndSleepTilRested : State<Miner>
    {
        /// <summary>
        /// </summary>
        private static GoHomeAndSleepTilRested instance;

        /// <summary>
        /// Prevents a default instance of the GoHomeAndSleepTilRested class from being created.
        /// </summary>
        private GoHomeAndSleepTilRested()
        {
            if (null != instance)
            {
                Debug.LogError("Singleton already created.");
                return;
            }

            instance = this;
        }

        /// <summary>
        /// Gets the accessor for the <see cref="GoHomeAndSleepTilRested"/> singleton
        /// instance.
        /// </summary>
        public static GoHomeAndSleepTilRested Instance
        {
            get
            {
                if (null == instance)
                {
                    new GoHomeAndSleepTilRested();
                }

                if (null == instance)
                {
                    Debug.LogError("Singleton instance not set by constructor.");
                    return default(GoHomeAndSleepTilRested);
                }

                return instance;
            }
        }

        /// <summary>
        /// This will execute when the state is entered.
        /// </summary>
        /// <param name="miner">
        /// The game object associated with this state.
        /// </param>
        public override void Enter(Miner miner)
        {
            if (miner.Location != Locations.Shack)
            {
                miner.DisplayMessage("Walkin' home");

                miner.Location = Locations.Shack;

                // let the wife know I'm home
                MessageDispatcher.Instance.DispatchMsg(
                    MessageDispatcher.SendMsgImmediately, // time delay
                    miner.ID, // ID of sender
                    EntityManager.Find<MinersWife>("Elsa").ID, // ID of recipient
                    (MessageTypes)WestWorldMessageTypes.HiHoneyImHome, // the message
                    MessageDispatcher.NoAdditionalInfo);
            }
        }

        /// <summary>
        /// This is the state's normal update function.
        /// </summary>
        /// <param name="miner">
        /// The game object associated with this state.
        /// </param>
        public override void Execute(Miner miner)
        {
            // if miner is not fatigued start to dig for nuggets again.
            if (!miner.IsFatigued)
            {
                miner.DisplayMessage("All mah fatigue has drained away. Time to find more gold!");

                miner.StateMachine.ChangeState(EnterMineAndDigForNugget.Instance);
            }
            else
            {
                // sleep
                miner.DecreaseFatigue();

                miner.DisplayMessage("ZZZZ... ");
            }
        }

        /// <summary>
        /// This will execute when the state is exited.
        /// </summary>
        /// <param name="miner">
        /// The game object associated with this state.
        /// </param>
        public override void Exit(Miner miner)
        {
        }

        /// <summary>
        /// This executes if the agent receives a message from the message dispatcher.
        /// </summary>
        /// <param name="miner">
        /// The game object associated with this state.
        /// </param>
        /// <param name="msg">
        /// The message.
        /// </param>
        /// <returns>
        /// True if the message was handled. Otherwise, false.
        /// </returns>
        public override bool OnMessage(Miner miner, Telegram msg)
        {
            switch ((WestWorldMessageTypes)msg.Msg)
            {
                case WestWorldMessageTypes.StewReady:

                    //Debug.Log("Message handled by " + miner.name + " at time: " + Time.time);

                    miner.DisplayMessage("Okay Hun, ahm a comin'!");

                    miner.StateMachine.ChangeState(EatStew.Instance);

                    return true;
            }

            // send msg to global message handler
            return false;
        }
    }
}