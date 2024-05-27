using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

namespace Scenes.InScene
{
    public class CameraMove : MonoBehaviour
    {
        void Start()
        {
            this.UpdateAsObservable()
                .Select(_ => Input.GetAxis("Mouse X"))
                .Subscribe(x => transform.RotateAround(transform.position, Vector3.up, x));
        }
    }
}
