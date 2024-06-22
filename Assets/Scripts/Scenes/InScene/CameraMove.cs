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
        [SerializeField] private float _cameraRotationSensitive = 1.0f; //Š´“x
        [SerializeField] private int _cameraRotationLimit = 50; //Šp“xŒÀŠE

        void Start()
        {
            //ƒJ[ƒ\ƒ‹ŒÅ’è
            Cursor.lockState = CursorLockMode.Locked;

            //ƒJƒƒ‰‰ñ“]iã‰ºj
            this.UpdateAsObservable()
                .Select(_ => Input.GetAxis("Mouse Y") * _cameraRotationSensitive)
                .Select(x => AngleLimit(x))
                .Subscribe(x => transform.Rotate(-1 * x, 0, 0));
        }

        //Šp“xŒÀŠE
        private float AngleLimit(float yDiff)
        {
            var xAngle = transform.eulerAngles.x > 180 ? transform.eulerAngles.x - 360 : transform.eulerAngles.x;
            if (xAngle > _cameraRotationLimit && yDiff < 0) return 0;
            if (xAngle < -1 * _cameraRotationLimit && yDiff > 0) return 0;

            return yDiff;
        }
    }
}
