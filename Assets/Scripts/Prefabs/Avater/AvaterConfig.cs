using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Prefabs.Avater
{
    [CreateAssetMenu(menuName = "ScriptableObjects/Prefabs/Avater")]
    public class AvaterConfig : ScriptableObject
    {
        [Header("Time")]
        public float rotationTime = 0f;
        public float moveTime = 0f;
        public float minWaitingTime = 0f;
        public float maxWaitingTime = 0f;
    }
}
