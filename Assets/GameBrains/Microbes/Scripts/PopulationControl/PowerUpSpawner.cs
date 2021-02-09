/* Abdullah Arif 
*      class to spawn power-ups
*/
using GameBrains.Common.Managers;
using GameBrains.Common.Timers;
using GameBrains.Microbes.Scripts.Entities;
using UnityEngine;

namespace GameBrains.Microbes.Scripts.PopulationControl{
    public class PowerUpSpawner : MonoBehaviour
    {
        public GameObject cubePrefab;
        public float spawnPointRadius = 1f;
        public float updatesPerSecond = 2f;
        public float xMin=-10.0f, zMin=-10.0f, xMax=10.0f, zMax=10.0f;

        public void Awake()
        {
            SimpleRegulator = new SimpleRegulator(updatesPerSecond);
        }

        /// <summary>
        /// Gets or sets the regulator to control the updates per second.
        /// </summary>
        public SimpleRegulator SimpleRegulator { get; protected set; }
        protected virtual void Update()
        {
            if (SimpleRegulator.IsReady && cubePrefab)
            {
                GameObject powerUp = Instantiate(cubePrefab);
                powerUp.transform.position = new Vector3(Random.Range(xMin, xMax), 0.5f, Random.Range(zMin, zMax));
            }
        
        }
    }
}
