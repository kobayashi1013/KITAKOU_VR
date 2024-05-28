using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

namespace Scenes.InScene
{
    public class CameraMove : MonoBehaviour
    {
        [Header("Parameters")]
        [SerializeField] private float _cameraRotationSensitive = 1.0f; //���x
        [SerializeField] private int _cameraRotationLimit = 50; //�p�x���E

        void Start()
        {
            //�J�[�\���Œ�
            Cursor.lockState = CursorLockMode.Locked;

            //�J������]�i�㉺�j
            this.UpdateAsObservable()
                .Select(_ => Input.GetAxis("Mouse Y") * _cameraRotationSensitive)
                .Select(x => AngleLimit(x))
                .Subscribe(x => transform.Rotate(-1 * x, 0, 0));
        }

        //�p�x���E
        private float AngleLimit(float yDiff)
        {
            var xAngle = transform.eulerAngles.x > 180 ? transform.eulerAngles.x - 360 : transform.eulerAngles.x;
            if (xAngle > _cameraRotationLimit && yDiff < 0) return 0;
            if (xAngle < -1 * _cameraRotationLimit && yDiff > 0) return 0;

            return yDiff;
        }
    }
}
