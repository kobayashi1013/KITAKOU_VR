using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Utils.UI
{
    public class Blackout : MonoBehaviour
    {
        [SerializeField] private Material _material;

        private void Start()
        {
            SetAlpha(0f);
        }

        private void SetAlpha(float alpha)
        {
            if (_material != null)
            {
                Color color = _material.color;
                color.a = alpha;
                _material.color = color;
            }
        }

        public async Task Fade(float startAlpha, float endAlpha, float time)
        {
            float elapsed = 0f;

            while (elapsed < time)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(startAlpha, endAlpha, elapsed / time);
                SetAlpha(alpha);
                await Task.Yield();
            }

            SetAlpha(endAlpha);
        }
    }
}
