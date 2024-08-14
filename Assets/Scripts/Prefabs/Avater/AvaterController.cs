using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Prefabs.Avater
{
    public class AvaterController : MonoBehaviour
    {
        [Header("Component")]
        [SerializeField] private CharacterController _characterCntroller;

        public void Move(Vector3 direction, float distance)
        {
            _characterCntroller.Move(direction * distance);
        }
    }
}
