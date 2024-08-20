using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UniRx.Triggers;
using UniRx;
using Unity.XR.CoreUtils;

namespace Prefabs.Player
{
    public class CameraMove : MonoBehaviour
    {
        [SerializeField] private CharacterController _characterController;
        [SerializeField, Range(0, 0.1f)] private float _amplitude = 0.015f;
        [SerializeField, Range(0, 30)] private float _frequency = 10.0f;

        private Vector3 _baseLocalPosition;

        private void Start()
        {
            _baseLocalPosition = transform.localPosition;

            this.UpdateAsObservable()
                .Select(_ => CheckHeadBob())
                .Subscribe(x => PlayHeadBob(x));
        }
        /*private Vector3 FootStepMotion()
        {
            Vector3 pos = Vector3.zero;
            pos.y += Mathf.Sin(Time.time * _frequency) * _amplitude;
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
        }*/

        private float CheckHeadBob()
        {
            if (!_characterController.isGrounded
                || _characterController.velocity.magnitude == 0f) return 0f; //Ž~‚Ü‚Á‚Ä‚¢‚é
            else if (Input.GetKey(KeyCode.LeftShift)) return 2f; //‘–‚Á‚Ä‚¢‚é
            else return 1f; //•à‚¢‚Ä‚¢‚é
        }

        private void PlayHeadBob(float state)
        {
            Debug.Log(state);
            transform.localPosition = new Vector3(
                _baseLocalPosition.x,
                _baseLocalPosition.y + Mathf.Sin(Time.time * _frequency) * _amplitude * state,
                _baseLocalPosition.z);
        }
    }
}