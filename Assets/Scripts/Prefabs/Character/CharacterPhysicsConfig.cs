using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Prefabs.Character
{
    [CreateAssetMenu(menuName = "CharacterPhysicsConfig")]
    public class CharacterPhysicsConfig : ScriptableObject
    {
        public float addForceSensitive = 0f;
        public float attenuationSensitive = 0f;
    }
}
