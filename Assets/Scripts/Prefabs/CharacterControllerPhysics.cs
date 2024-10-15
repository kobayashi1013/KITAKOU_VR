using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Prefabs
{
    public class CharacterControllerPhysics : MonoBehaviour
    {
        [SerializeField] private PhysicsConfig _physicsConfig;

        private Vector3 _externalForceVelocity = Vector3.zero;

        public void AddForce(Vector3 force)
        {
            _externalForceVelocity = force;
        }

        public Vector3 ExternalForce()
        {
            _externalForceVelocity = Vector3.Lerp(_externalForceVelocity, Vector3.zero, _physicsConfig.attenuationSensitive * Time.deltaTime);
            return _externalForceVelocity;
        }

        /*private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            if (this.gameObject.CompareTag("Avater") && hit.gameObject.CompareTag("Avater")) return;
            if (hit.gameObject.TryGetComponent<CharacterControllerPhysics>(out var physics) == false) return;

            Vector3 direction = hit.normal * -1;
            physics.AddForce(direction * _physicsConfig.addForceSensitive);
        }*/
    }
}
