using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Prefabs.Avater
{
    public class AvaterMove : MonoBehaviour
    {
        public void Move(Vector3 otherPosition)
        {
            var dir = transform.position - otherPosition;
            transform.Translate(dir);
        }
    }
}
