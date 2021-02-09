using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace GameBrains.Microbes.Scripts.Collectible{
    public class CoinSpin : MonoBehaviour
    {
        // public float rotX=30, rotY=15, rotZ=45;
        public Vector3 rotPerSecond = new Vector3(30,15,45);
        
        void Update()
        {       
            transform.rotation = Quaternion.Euler( rotPerSecond*Time.time );
        }
    }
}

