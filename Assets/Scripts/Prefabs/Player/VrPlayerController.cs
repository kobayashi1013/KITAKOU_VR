using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UniRx;
using UniRx.Triggers;

namespace Prefabs.Player
{
    public class VrPlayerController : PlayerControllerBase
    {
        [SerializeField] private PlayerConfig _playerConfig;

        private ContinuousMoveProviderBase _moveProvider;
        private CharacterController _characterController;

        private void Start()
        {
            _moveProvider = GetComponent<ContinuousMoveProviderBase>();
            _characterController = GetComponent<CharacterController>();

            this.UpdateAsObservable().Subscribe(_ => PlayerPhysics());
        }

        private void PlayerPhysics()
        {
            Vector3 movement = Vector3.zero;
            movement = ExternalForce(_playerConfig.attenuationRate);

            _characterController.Move(movement * Time.deltaTime);
        }

        public void OnDash(InputAction.CallbackContext context)
        {
            if (context.performed) _moveProvider.moveSpeed = _playerConfig.dashSpeed;
            if (context.canceled) _moveProvider.moveSpeed = _playerConfig.walkSpeed;
        }

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            if (hit.gameObject.CompareTag("Avater") == false) return;
            if (hit.gameObject.TryGetComponent<Rigidbody>(out var rigidbody) == false) return;

            Vector3 direction = hit.normal * -1;
            rigidbody.AddForce(direction * _playerConfig.addForceSensitive, ForceMode.Impulse);
        }
    }
}
