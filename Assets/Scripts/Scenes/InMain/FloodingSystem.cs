using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

namespace Scenes.InMain
{
    public class FloodingSystem : MonoBehaviour
    {
        [SerializeField] private AudioSource _floodingAudio;
        [SerializeField] private float _visibilityHeight;
        [SerializeField] private float _soundPos; //サウンドが始まる距離
        [SerializeField] private float _boundary; //洪水面

        private void Start()
        {
            this.UpdateAsObservable()
                .Select(_ => AudioVolume(InMainManager.Instance.playerObject.transform.position.y + _visibilityHeight))
                .Subscribe(x => OnFloodingAudio(x));
        }

        private void OnFloodingAudio(float volume)
        {
            _floodingAudio.volume = volume;
        }

        private float AudioVolume(float yPos)
        {
            if (yPos > _soundPos) return 0;
            else if (yPos < _boundary) return 0.3f;
            else return 1 - (yPos - _boundary) / (_soundPos - _boundary);
        }
    }
}
