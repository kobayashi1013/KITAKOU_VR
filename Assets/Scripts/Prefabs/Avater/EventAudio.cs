using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;
using UniRx;

namespace Prefabs.Avater
{
    public class EventAudio : MonoBehaviour
    {
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private float _interval = 60.0f;
        [SerializeField] private List<AudioClip> _audioClipList = new List<AudioClip>();

        private void Start()
        {
            Observable.Timer(System.TimeSpan.FromSeconds(0), System.TimeSpan.FromSeconds(_interval))
                .Subscribe(_ => Play())
                .AddTo(gameObject);
        }

        private async void Play()
        {
            foreach (var clip in _audioClipList)
            {
                _audioSource.PlayOneShot(clip);

                var token = this.GetCancellationTokenOnDestroy();
                await UniTask.WaitUntil(() => _audioSource.isPlaying == false, cancellationToken: token);
            }
        }
    }
}
