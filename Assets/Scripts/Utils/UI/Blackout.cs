using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

namespace Utils.UI
{
    public class Blackout : MonoBehaviour
    {
        [SerializeField] private Image _mask;
        
        private void SetAlpha(float alpha)
        {
            if (_mask != null)
            {
                Color color = _mask.color;
                color.a = alpha;
                _mask.color = color;
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
