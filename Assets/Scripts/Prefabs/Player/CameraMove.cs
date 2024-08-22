using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

namespace Prefabs.Player
{
    public class CameraMove : MonoBehaviour
    {
        [SerializeField] private float y_sensitivity = 3f;
        [SerializeField] private float rot_limit = 50.0f;

        private float x_rot = 0f;

        void Start()
        {
            this.UpdateAsObservable()
                .Subscribe(_ =>
                {
                    float y_mouse = Input.GetAxis("Mouse Y");
                    x_rot -= y_mouse * y_sensitivity;

                    if (x_rot < -1 * rot_limit) x_rot = -1 * rot_limit;
                    else if (x_rot > rot_limit) x_rot = rot_limit;

                    transform.localEulerAngles = new Vector3(
                        x_rot,
                        transform.localEulerAngles.y,
                        transform.localEulerAngles.z);
                });
        }
    }
}