using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;
using UniRx;
using UniRx.Triggers;
using Prefabs.Avater;

namespace Prefabs.Player
{
    public class PlayerController_VR : MonoBehaviour
    {
        private static readonly float _colliderRange = 0.8f; //プレイヤーの周囲ぶつかり範囲

        [Header("Parameters")]
        [SerializeField] LayerMask _avaterLayer; //アバターのレイヤー
        [SerializeField] private float _playerSpeed = 1.0f; //スピード

        private List<AvaterController> _avaterControllerList = new List<AvaterController>();
        private ContinuousMoveProviderBase _moveProvider;
        private float _collectionTime = 0f;

        void Start()
        {
            //VRMover
            _moveProvider = GetComponent<ContinuousMoveProviderBase>();

            //周囲のアバターを押しのける
            /*this.UpdateAsObservable()
                .Subscribe(_ => AvaterMove());*/
        }

        public void OnDash(InputAction.CallbackContext context)
        {
            if (context.performed)
            {
                _moveProvider.moveSpeed = _playerSpeed * 1.8f;
            }

            if (context.canceled)
            {
                _moveProvider.moveSpeed = _playerSpeed;
            }
        }
    }
}
