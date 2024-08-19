using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;

namespace Prefabs.Player
{
    public class HeadBob : MonoBehaviour
    {
        [SerializeField] private CharacterController _characterController;
        [SerializeField, Range(0, 0.1f)] private float _amplitude = 0.015f;
        [SerializeField, Range(0, 30)] private float _frequency = 10.0f;

        private Vector3 FootStepMotion()
        {
            Vector3 pos = Vector3.zero;
            pos.y += Mathf.Sin(Time.deltaTime * _frequency) * _amplitude;
            return pos;
        }

        private void CheckMotion()
        {
            if (!_characterController.isGrounded) return;

            PlayMotion(FootStepMotion());
        }

        private void PlayMotion(Vector3 mortion)
        {
            transform.localPosition += mortion;
        }

        private void Update()
        {
            CheckMotion();
        }
    }
}