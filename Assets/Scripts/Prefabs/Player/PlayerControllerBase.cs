using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Prefabs.Player
{
    public class PlayerControllerBase : MonoBehaviour
    {
        public PlayerConfig _playerConfig;

        private Vector3 _externalForceVelocity = Vector3.zero;

        public void AddForce(Vector3 force)
        {
            _externalForceVelocity += force;
        }

        public Vector3 ExternalForce()
        {
            _externalForceVelocity = Vector3.Lerp(_externalForceVelocity, Vector3.zero, _playerConfig.attenuationSensitive * Time.deltaTime);
            return _externalForceVelocity;
        }

        private void OnControllerColliderHit(ControllerColliderHit hit)
        {
            if (hit.gameObject.CompareTag("Avater") == false) return;
            if (hit.gameObject.TryGetComponent<Rigidbody>(out var rigidbody) == false) return;

            Vector3 direction = hit.normal * -1;
            rigidbody.AddForce(direction * _playerConfig.addForceSensitive, ForceMode.Impulse);
        }
    }
}