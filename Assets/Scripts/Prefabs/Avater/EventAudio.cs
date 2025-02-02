using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Prefabs.Avater
{
    public class EventAudio : MonoBehaviour
    {
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private float _minInterval = 0f;
        [SerializeField] private float _maxInterval = 0f;
        [SerializeField] private List<AudioClip> _audioClipList = new List<AudioClip>();

        private void Start()
        {
            Play();
        }

        private async void Play()
        {
            try
            {
                //初期待機時間
                float initialWaitTimeSeconds = UnityEngine.Random.Range(0f, _minInterval);
                await UniTask.Delay(TimeSpan.FromSeconds(initialWaitTimeSeconds));

                while (true)
                {
                    //音声再生
                    foreach (var clip in _audioClipList)
                    {
                        _audioSource.PlayOneShot(clip);

                        var token = this.GetCancellationTokenOnDestroy();
                        await UniTask.WaitUntil(() => _audioSource.isPlaying == false);
                    }

                    //ランダム時間待機
                    float waitTimeSeconds = UnityEngine.Random.Range(_minInterval, _maxInterval);
                    await UniTask.Delay(TimeSpan.FromSeconds(waitTimeSeconds));
                }
            }
            catch { }
        }
    }
}
