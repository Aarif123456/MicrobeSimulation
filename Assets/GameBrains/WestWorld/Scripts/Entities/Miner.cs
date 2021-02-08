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

using GameBrains.Common.Entities;
using GameBrains.Common.FiniteStateMachine;
using GameBrains.Common.Managers;
using GameBrains.Common.Messaging;
using GameBrains.Common.Timers;
using GameBrains.WestWorld.Scripts.States;
using UnityEngine;

namespace GameBrains.WestWorld.Scripts.Entities
{
    public sealed class Miner : Entity
    {
        [SerializeField] float updatesPerSecond = 0.25f;

        /// <summary>
        /// The amount of gold a miner must have before he feels comfortable.
        /// </summary>
        [SerializeField] int comfortLevel = 5;

        /// <summary>
        /// The amount of nuggets a miner can carry.
        /// </summary>
        [SerializeField] int maximumNuggets = 3;

        /// <summary>
        /// Above this value a miner is thirsty.
        /// </summary>
        [SerializeField] int thirstLevel = 5;

        /// <summary>
        /// Above this value a miner is sleepy.
        /// </summary>
        [SerializeField] int tirednessThreshold = 5;

        public override void Awake()
        {
            base.Awake();

            Location = Locations.Shack;
            GoldCarried = 0;
            MoneyInBank = 0;
            Thirst = 0;
            Fatigue = 0;

            StateMachine = new StateMachine<Miner>(this);

            // NOTE, A GLOBAL STATE HAS NOT BEEN IMPLEMENTED FOR THE MINER
            StateMachine.SetCurrentState(GoHomeAndSleepTilRested.Instance);

            SimpleRegulator = new SimpleRegulator(updatesPerSecond); // control updates per second
        }

        /// <summary>
        /// Gets a value indicating whether the miner's pockets are full of gold.
        /// </summary>
        public bool ArePocketsFull => GoldCarried >= maximumNuggets;

        /// <summary>
        /// Gets or sets the fatigue level.
        /// </summary>
        public int Fatigue { get; set; }

        /// <summary>
        /// Gets or sets the amount of gold carried.
        /// </summary>
        public int GoldCarried { get; set; }

        /// <summary>
        /// Gets a value indicating whether the miner is tired.
        /// </summary>
        public bool IsFatigued => Fatigue >= tirednessThreshold;

        /// <summary>
        /// Gets a value indicating whether the miner is thirsty.
        /// </summary>
        public bool IsThirsty => Thirst >= thirstLevel;

        /// <summary>
        /// Gets or sets the miner's location.
        /// </summary>
        public Locations Location { get; set; }

        /// <summary>
        /// Gets or sets the amount of gold in the bank.
        /// </summary>
        public int MoneyInBank { get; set; }

        /// <summary>
        /// Gets the regulator.
        /// </summary>
        public SimpleRegulator SimpleRegulator { get; private set; }

        /// <summary>
        /// Gets the state machine.
        /// </summary>
        public StateMachine<Miner> StateMachine { get; private set; }

        /// <summary>
        /// Gets or sets the level of thirst.
        /// </summary>
        public int Thirst { get; set; }

        /// <summary>
        /// The amount of gold a miner must have before he feels comfortable.
        /// </summary>
        public int ComfortLevel
        {
            get => comfortLevel;
            set => comfortLevel = value;
        }

        /// <summary>
        /// Add to the amount of gold carried.
        /// </summary>
        /// <param name="amount">
        /// The amount to add.
        /// </param>
        public void AddToGoldCarried(int amount)
        {
            GoldCarried += amount;
            if (GoldCarried < 0)
            {
                GoldCarried = 0;
            }
        }

        /// <summary>
        /// Add to the gold in the bank.
        /// </summary>
        /// <param name="amount">
        /// The amount to add.
        /// </param>
        public void AddToMoneyInBank(int amount)
        {
            MoneyInBank += amount;
            if (MoneyInBank < 0)
            {
                MoneyInBank = 0;
            }
        }

        /// <summary>
        /// Buy and drink whiskey. Quenches this but costs 2 gold.
        /// </summary>
        public void BuyAndDrinkAWhiskey()
        {
            Thirst = 0;
            MoneyInBank -= 2;
        }

        /// <summary>
        /// Decrease the fatigue level.
        /// </summary>
        public void DecreaseFatigue()
        {
            Fatigue -= 1;
        }

        /// <summary>
        /// Messages to the entity are forwarded to its FSM.
        /// </summary>
        /// <param name="msg">
        /// The message.
        /// </param>
        /// <returns>
        /// True if the message was handled. Otherwise, false.
        /// </returns>
        public override bool HandleMessage(Telegram msg)
        {
            return StateMachine.HandleMessage(msg);
        }

        /// <summary>
        /// Increase the fatigue level.
        /// </summary>
        public void IncreaseFatigue()
        {
            Fatigue += 1;
        }

        public override void Update()
        {
            if (SimpleRegulator.IsReady)
            {
                Thirst += 1;
                StateMachine.Update();
            }

            base.Update();
        }
    }
}