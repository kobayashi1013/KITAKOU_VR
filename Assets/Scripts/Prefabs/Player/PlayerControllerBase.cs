using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Prefabs.Player
{
    public class PlayerControllerBase : MonoBehaviour
    {
        public Vector3 _externalForceVelocity = Vector3.zero;

        public void AddForce(Vector3 force)
        {
            _externalForceVelocity += force;
        }

        public Vector3 ExternalForce(float attenuationRate)
        {
            _externalForceVelocity = Vector3.Lerp(_externalForceVelocity, Vector3.zero, attenuationRate * Time.deltaTime);
            return _externalForceVelocity;
        }
    }
}