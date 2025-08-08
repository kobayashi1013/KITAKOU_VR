using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils.UI;

namespace Utils.Prefab
{
    public class BlackoutRange : MonoBehaviour
    {
        private async void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.tag == "Player")
            {
                var blackout = FindFirstObjectByType<Blackout>();
                if (blackout != null)
                {
                    await blackout.Fade(0f, 1f, 1f);
                }
            }
        }
    }
}
