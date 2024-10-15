using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Prefabs
{
    [CreateAssetMenu(menuName = "ScriptableObjects/Prefabs/PhysicsConfig", fileName = "PhysicsConfig")]
    public class PhysicsConfig : ScriptableObject
    {
        [Header("External Force")]
        public float addForceSensitive = 0f;
        public float attenuationSensitive = 0f;
    }
}
