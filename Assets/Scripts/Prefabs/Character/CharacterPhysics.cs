using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Prefabs.Character
{
    public class CharacterPhysics : MonoBehaviour
    {
        public CharacterPhysicsConfig characterPhysicsConfig;

        private Vector3 _externalForceVelocity = Vector3.zero;

        public void AddForce(Vector3 force)
        {
            _externalForceVelocity += force;
        }

        public Vector3 ExternalForce(float deltaTime)
        {
            _externalForceVelocity = Vector3.Lerp(_externalForceVelocity, Vector3.zero, characterPhysicsConfig.attenuationSensitive * deltaTime);
            return _externalForceVelocity;
        }
    }
}
