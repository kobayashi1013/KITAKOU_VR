using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Prefabs.Player
{
    [CreateAssetMenu(menuName = "ScriptableObjects/Prefabs/Player")]
    public class PlayerConfig : ScriptableObject
    {
        [Header("Move Speed")]
        public float walkSpeed = 0f;
        public float dashSpeed = 0f;
        [Header("Force")]
        public float addForceSensitive = 0f;
        public float attenuationRate = 0f;
    }
}
