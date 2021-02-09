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
using System.Linq;
using GameBrains.Common.Entities;
using GameBrains.Common.FiniteStateMachine;
using GameBrains.Common.Managers;
using GameBrains.Common.Messaging;
using GameBrains.Common.Timers;
using GameBrains.Microbes.Scripts.Movement;
using GameBrains.Microbes.Scripts.PopulationControl;
using GameBrains.Microbes.Scripts.States;
using UnityEngine;

#endregion Copyright � ThotLab Games 2011. Licensed under the terms of the Microsoft Reciprocal Licence (Ms-RL).

////[RequireComponent(typeof(Motor))]
////[RequireComponent(typeof(Attractor))]
////[RequireComponent(typeof(Repeller))]
////[RequireComponent(typeof(LifeSpan))]
////[RequireComponent(typeof(BoundsChecker))]
////[RequireComponent(typeof(AudioSource))]

namespace GameBrains.Microbes.Scripts.Entities
{
    public sealed class Microbe : Entity
    {
        [SerializeField] GameObject prefab;

        [SerializeField] Spawner spawner;

        [SerializeField] AudioClip microbeBirthSound;
        [SerializeField] AudioClip microbeDeathSound;

        [SerializeField] float updatesPerSecond = 10f;
        [SerializeField] float maxInvinciblityTimer=10f;
        [SerializeField] int hungerThreshold = 120;
        [SerializeField] int cannibalismThreshold = 240;
        [SerializeField] float invinciblityTimer = 0.0f;

        // The type of microbe we are.
        [SerializeField] MicrobeTypes microbeType;

        static readonly MicrobeTypes[] MicrobeTypeArray =
            {MicrobeTypes.Blue, MicrobeTypes.Red, MicrobeTypes.Green, MicrobeTypes.Yellow};

        bool isDead, invincible;

        // Text display showing what state the microbe is in
        TextMesh stateDisplay;

        public Motor Motor { get; set; }

        public Attractor Attractor { get; set; }

        public Repeller Repeller { get; set; }

        public LifeSpan LifeSpan { get; set; }

        public BoundsChecker BoundsChecker { get; set; }

        public AudioSource AudioSource { get; set; }

        // The type of microbes we eat.
        public MicrobeTypes FoodTypes { get; set; }

        // The types of microbes we can mate with.
        public MicrobeTypes MateTypes { get; set; }

        // The type of microbes we avoid.
        public MicrobeTypes AvoidTypes { get; set; }

        public override void Awake()
        {
            base.Awake();

            Hunger = 0;

            Motor = gameObject.GetComponent<Motor>();

            // make sure we get the Attractor, not the Repeller which is also an Attractor!
            Attractor = gameObject.GetComponents<Attractor>().First(a => !(a is Repeller));

            Repeller = gameObject.GetComponent<Repeller>();
            LifeSpan = gameObject.GetComponent<LifeSpan>();
            BoundsChecker = gameObject.GetComponent<BoundsChecker>();
            AudioSource = gameObject.GetComponent<AudioSource>();

            StateMachine = new StateMachine<Microbe>(this);
            StateMachine.SetCurrentState(Sleeping.Instance);
            StateMachine.SetGlobalState(MicrobeGlobalState.Instance);
            
            // control updates per second
            SimpleRegulator = new SimpleRegulator(updatesPerSecond); 
            // Making sure the things that get set when the microbe dies are turned on.
            // This seems to be an issue with reproducing microbes that have been recently killed.
            gameObject.GetComponent<Collider>().isTrigger = false;
            gameObject.GetComponent<Renderer>().enabled = true;

            // Initializing the state display that shows what state the microbe is in
            // Pretty sure sleeping is the default state.:w
            stateDisplay = gameObject.GetComponentInChildren<TextMesh>();
            if(stateDisplay != null) UpdateStateDisplay("Sleeping");
        }

        /// <summary>
        /// Gets the regulator.
        /// </summary>
        public SimpleRegulator SimpleRegulator { get; private set; }

        /// <summary>
        /// Gets the state machine.
        /// </summary>
        public StateMachine<Microbe> StateMachine { get; private set; }

        /// <summary>
        /// Gets or sets the microbe's hunger level.
        /// </summary>
        public int Hunger { get; set; }

        /// <summary>
        /// Gets a value indicating whether the microbe is hungry.
        /// </summary>
        public bool IsHungry => Hunger >= hungerThreshold;

        // Gets a value indicating whether the microbe has resorted to cannibalism (ie: Eating its own microbe type)
        public bool IsCannibalistic => Hunger >= cannibalismThreshold;

        public MicrobeTypes MicrobeType
        {
            get => microbeType;
            set => microbeType = value;
        }

        public override void Start()
        {
            base.Start();
            StateMachine.CurrentState.Enter(this);
            AudioSource.PlayOneShot(microbeBirthSound);
        }

        public override void Update()
        {
            if (SimpleRegulator.IsReady)
            {

                Hunger++;
                StateMachine.Update();
            }

            base.Update();
        }

        /// <summary>
        /// Messages to the microbe are forwarded to its FSM.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        /// <returns>
        /// True if the message was handled. Otherwise, false.
        /// </returns>
        public override bool HandleMessage(Telegram message)
        {
            return StateMachine.HandleMessage(message);
        }

        /* Microbes can eat each other */
        public void Kill(){
            if(!invincible){
                Die();
            }
        }

        /* Some cases require death - without regard for invincibility */
        public void Die()
        {
            if (!isDead)
            {
                UpdateStateDisplay("");

                isDead = true;
                AudioSource.PlayOneShot(microbeDeathSound);
                EntityManager.Remove(this);
                gameObject.GetComponent<Collider>().isTrigger = true;
                gameObject.GetComponent<Renderer>().enabled = false;
                foreach (Transform child in transform)
                {
                    child.gameObject.SetActive(false);
                }
                Destroy(gameObject, microbeDeathSound.length);
            }
        }

        public static Microbe SpawnRandomTypeAt(GameObject microbePrefab, Vector2 spawnPoint)
        {
            int microbeTypeIndex = Random.Range(0, MicrobeTypeArray.Length);

            return Spawn(microbePrefab, MicrobeTypeArray[microbeTypeIndex], spawnPoint);
        }

        public void UpdateStateDisplay(string display)
        {
            if (stateDisplay != null) {
                stateDisplay.text = display;
            }
        }

        /// <param name="microbePrefab"></param>
        /// <param name="microbeType">
        /// The type of microbe to create.
        /// </param>
        /// <param name="position">
        /// The position to spawn at.
        /// </param>
        public static Microbe Spawn(GameObject microbePrefab, MicrobeTypes microbeType, Vector2 position)
        {
            GameObject microbeObject = Instantiate(microbePrefab);
            microbeObject.SetActive(true);
            microbeObject.transform.position = new Vector3(position.x, 0.5f, position.y);
            microbeObject.transform.parent = GameObject.Find("Agents").transform;
            microbeObject.GetComponent<Renderer>().material.color = GetColor(microbeType);

            Microbe microbe = microbeObject.GetComponent<Microbe>();
            microbe.MicrobeType = microbeType;
            microbe.FoodTypes = GetFoodTypes(microbeType);
            microbe.MateTypes = GetMateTypes(microbeType);
            microbe.AvoidTypes = GetAvoidTypes(microbeType);

            // You may want to change these in certain states.
            microbe.Attractor.AttractTypes = microbe.FoodTypes;
            microbe.Repeller.AttractTypes = microbe.AvoidTypes;

            switch (microbeType)
            {
                case MicrobeTypes.Blue:
                    microbeObject.name = "BlueMicrobe" + microbe.ID;
                    break;
                case MicrobeTypes.Red:
                    microbeObject.name = "RedMicrobe" + microbe.ID;
                    break;
                case MicrobeTypes.Green:
                    microbeObject.name = "GreenMicrobe" + microbe.ID;
                    break;
                case MicrobeTypes.Yellow:
                    microbeObject.name = "YellowMicrobe" + microbe.ID;
                    break;
                default:
                    microbeObject.name = "Microbe" + microbe.ID;
                    break;
            }

            return microbe;
        }

        private MicrobeTypes GenerateChildMicrobeType(MicrobeTypes otherParentType)
        {
            MicrobeTypes childType;
            float rng = Random.value;

            // RNGsus decides if the child will be of type A, B, or of a random mutation type
            if (rng > 0.3)
            {
                childType = MicrobeType;
            }
            else if (rng > 0.6)
            {
                childType = otherParentType;
            }
            else
            {
                rng = Random.value;
                if (rng < 0.25)
                    childType = MicrobeTypes.Red;
                else if (rng < 0.5)
                    childType = MicrobeTypes.Blue;
                else if (rng < 0.75)
                    childType = MicrobeTypes.Green;
                else
                    childType = MicrobeTypes.Yellow;
            }

            return childType;
        }


        /* Attempt to reproduce with the other microbe. If it's successful, a new
        *  microbe will spawn between the two of them.
        *  Usually, both microbes will undergo this procedure. This is why there's a 
        *  probability; Both microbes act as the "mother".
        *
        *  There is also a probability that the microbe will stay in 
        *  the reproduction state after mating. 
        */
        public bool AttemptReproduction(Microbe otherMicrobe)
        {
            // Test if reproduction failed.
            if (Random.value < 0.5)
            {
                return false;
            }

            // Generating the spawn pos, and the child's microbe type
            // Causes big explosions microbes when a bunch are birthed in the same area. Looks hilarious.
            // Vector2 spawnPos = (transform.position + otherMicrobe.transform.position) / 2;
            MicrobeTypes childType = GenerateChildMicrobeType(otherMicrobe.MicrobeType);
            Vector2 spawnPos = spawner.GetValidSpawnPosition();

            // Generate the child
            if (spawnPos != Vector2.negativeInfinity)
            {
                Debug.Log(name + " and " + otherMicrobe.name + " have reproduced");
                Spawn(prefab, childType, spawnPos);
            }

            // With a probability, this microbe should be done reproducing.
            if (Random.value < 0.9)
            {
                StateMachine.ChangeState(Sleeping.Instance);
            }

            return true;
        }

        public static MicrobeTypes GetFoodTypes(MicrobeTypes microbeType)
        {
            MicrobeTypes foodTypes;

            switch (microbeType)
            {
                case MicrobeTypes.Blue:
                    foodTypes = MicrobeTypes.Red | MicrobeTypes.Green;
                    break;
                case MicrobeTypes.Red:
                    foodTypes = MicrobeTypes.Green | MicrobeTypes.Yellow;
                    break;
                case MicrobeTypes.Green:
                    foodTypes = MicrobeTypes.Yellow | MicrobeTypes.Blue;
                    break;
                case MicrobeTypes.Yellow:
                    foodTypes = MicrobeTypes.Blue | MicrobeTypes.Red;
                    break;
                default:
                    foodTypes = MicrobeTypes.None;
                    break;
            }

            return foodTypes;
        }

        // Types of microbes that this color can mate with
        private static MicrobeTypes GetMateTypes(MicrobeTypes microbeType)
        {
            MicrobeTypes mateTypes;

            switch (microbeType)
            {
                case MicrobeTypes.Blue:
                    mateTypes = MicrobeTypes.Blue | MicrobeTypes.Yellow;
                    break;
                case MicrobeTypes.Red:
                    mateTypes = MicrobeTypes.Red | MicrobeTypes.Green;
                    break;
                case MicrobeTypes.Green:
                    mateTypes = MicrobeTypes.Green | MicrobeTypes.Red;
                    break;
                case MicrobeTypes.Yellow:
                    mateTypes = MicrobeTypes.Yellow | MicrobeTypes.Blue;
                    break;
                default:
                    mateTypes = MicrobeTypes.None;
                    break;
            }

            return mateTypes;
        }

        private static MicrobeTypes GetAvoidTypes(MicrobeTypes microbeType)
        {
            MicrobeTypes avoidTypes;

            switch (microbeType)
            {
                case MicrobeTypes.Blue:
                    avoidTypes = MicrobeTypes.Blue | MicrobeTypes.Green;
                    break;
                case MicrobeTypes.Red:
                    avoidTypes = MicrobeTypes.Red | MicrobeTypes.Yellow;
                    break;
                case MicrobeTypes.Green:
                    avoidTypes = MicrobeTypes.Green | MicrobeTypes.Blue;
                    break;
                case MicrobeTypes.Yellow:
                    avoidTypes = MicrobeTypes.Yellow | MicrobeTypes.Red;
                    break;
                default:
                    avoidTypes = MicrobeTypes.None;
                    break;
            }

            return avoidTypes;
        }

        private static Color GetColor(MicrobeTypes microbeType)
        {
            switch (microbeType)
            {
                case MicrobeTypes.Blue:
                    return Color.blue;
                case MicrobeTypes.Red:
                    return Color.red;
                case MicrobeTypes.Green:
                    return Color.green;
                case MicrobeTypes.Yellow:
                    return Color.yellow;
                default:
                    return Color.magenta;
            }
        }
        /* Invincibility */
        /*************************************************************************/
        public void BecomeInvincible(){
            invinciblityTimer = maxInvinciblityTimer;
            invincible = true;
            StateMachine.ChangeState(Invincible.Instance);
        }

        public void UpdateInvincibilityTimer(){
            invinciblityTimer -= Time.deltaTime;
            if(invinciblityTimer <= 0f){
                StateMachine.ChangeState(Sleeping.Instance);
            }
        } 

        public void LoseInvincibility(){
            invincible = false;
            invinciblityTimer = 0f;
        }
    }
}