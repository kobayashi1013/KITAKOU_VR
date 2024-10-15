using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UniRx;
using UniRx.Triggers;

namespace Prefabs.Character.Player
{
    public class CameraMove : MonoBehaviour
    {
        [SerializeField] private PlayerConfig _playerConfig;

        private Vector2 _rotateInput = Vector2.zero; //âÒì]ì¸óÕ
        private float rotationX = 0f; //åªç›ÇÃâÒì]

        private void Start()
        {
            this.UpdateAsObservable().Subscribe(_ => CameraRotate());
        }

        public void OnRotate(InputAction.CallbackContext context)
        {
            _rotateInput = context.ReadValue<Vector2>();
        }

        /// <summary>
        /// ÉJÉÅÉââÒì]
        /// </summary>
        private void CameraRotate()
        {
            rotationX += _rotateInput.y * _playerConfig.rotationSensitive * -1;

            if (rotationX < _playerConfig.verticalLimit * -1) rotationX = _playerConfig.verticalLimit * -1;
            else if (rotationX > _playerConfig.verticalLimit) rotationX = _playerConfig.verticalLimit;

            transform.localEulerAngles = new Vector3(
                rotationX,
                transform.localEulerAngles.y,
                transform.localEulerAngles.z);
        }
    }
}