using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UniRx;
using UniRx.Triggers;

namespace Prefabs.Player
{
    public class VrPlayerController : CharacterControllerPhysics
    {
        [SerializeField] private PlayerConfig _playerConfig;

        private ContinuousMoveProviderBase _moveProvider;
        private CharacterController _characterController;

        private void Start()
        {
            _moveProvider = GetComponent<ContinuousMoveProviderBase>();
            _characterController = GetComponent<CharacterController>();

            _moveProvider.moveSpeed = _playerConfig.walkSpeed;

            this.UpdateAsObservable().Subscribe(_ => PlayerPhysics());
        }

        /// <summary>
        /// ダッシュ
        /// </summary>
        /// <param name="context"></param>
        public void OnDash(InputAction.CallbackContext context)
        {
            if (context.performed) _moveProvider.moveSpeed = _playerConfig.dashSpeed;
            if (context.canceled) _moveProvider.moveSpeed = _playerConfig.walkSpeed;
        }

        /// <summary>
        /// プレイヤー動作
        /// </summary>
        private void PlayerPhysics()
        {
            Vector3 movement = Vector3.zero;
            movement = ExternalForce();

            _characterController.Move(movement * Time.deltaTime);
        }
    }
}
