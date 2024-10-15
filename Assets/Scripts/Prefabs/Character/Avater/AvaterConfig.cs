using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Prefabs.Character.Avater
{
    [CreateAssetMenu(menuName = "ScriptableObjects/Prefabs/Character/Avater")]
    public class AvaterConfig : ScriptableObject
    {
        [Header("ŽžŠÔ")]
        public float rotationTime = 0f;
        public float moveTime = 0f;
        public float minWaitingTime = 0f;
        public float maxWaitingTime = 0f;
    }
}
