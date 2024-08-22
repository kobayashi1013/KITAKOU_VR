using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using UniRx.Triggers;
using UniRx;
using Unity.XR.CoreUtils;
using Constant;

namespace Prefabs.Player
{
    public class HeadBob : MonoBehaviour
    {
        [SerializeField] private CharacterController _characterController;

        [SerializeField] private Transform _camera;
        [SerializeField] private Transform _cameraHolder;

        [SerializeField] private float _amplitude = 0.001f;
        [SerializeField] private float _frequency = 10.0f;

        private Vector3 _startPosition;
        private PlayerMoveState _playerMoveState;

        public void SetPlayerMoveState(PlayerMoveState state)
        {
            //Debug.Log(state);
            _playerMoveState = state;
        }

        private void Start()
        {
            _startPosition = _camera.localPosition;

            this.UpdateAsObservable()
                .Subscribe(_ =>
                {
                    PlayMotion(FootStepMotion(CheckMotion()));
                    //ResetPosition();
                    _camera.LookAt(FocusTarget());
                });
        }

        private void PlayMotion(Vector3 position)
        {
            _camera.localPosition += position;
        }

        private Vector3 FootStepMotion(float moveState)
        {
            var pos = new Vector3(
                Mathf.Cos(Time.time * _frequency / 2) * _amplitude * 2 * moveState,
                Mathf.Sin(Time.time * _frequency) * _amplitude * moveState,
                0f);

            return pos;
        }

        private float CheckMotion()
        {
            if (!_characterController.isGrounded
                || _playerMoveState == PlayerMoveState.Idle) return 0f;
            else if (_playerMoveState == PlayerMoveState.Dash) return 1.5f;
            else return 1f;
        }

        private void ResetPosition()
        {
            if (_camera.localPosition == _startPosition) return;
            _camera.localPosition = Vector3.Lerp(_camera.localPosition, _startPosition, 1 * Time.deltaTime);
        }

        private Vector3 FocusTarget()
        {
            Vector3 pos = new Vector3(transform.position.x, transform.position.y + _cameraHolder.localPosition.y, transform.position.z);
            pos += _cameraHolder.forward * 15.0f;
            return pos;
        }
    }
}