using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Prefabs.Player
{
    [CreateAssetMenu(menuName = "ScriptableObjects/Prefabs/Player")]
    public class PlayerConfig : ScriptableObject
    {
        public LayerMask isGroundedMask;
        public float gravitySensitive = 0f;
        [Header("Move Speed")]
        public float walkSpeed = 0f;
        public float dashSpeed = 0f;
        [Header("Rotation")]
        public float rotationSensitive = 0f;
        public float verticalLimit = 0f;
    }
}
