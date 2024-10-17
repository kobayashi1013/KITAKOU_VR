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
                //‰Šú‘Ò‹@ŽžŠÔ
                float initialWaitTimeSeconds = UnityEngine.Random.Range(0f, _minInterval);
                await UniTask.Delay(TimeSpan.FromSeconds(initialWaitTimeSeconds));

                while (true)
                {
                    //‰¹ºÄ¶
                    foreach (var clip in _audioClipList)
                    {
                        _audioSource.PlayOneShot(clip);

                        var token = this.GetCancellationTokenOnDestroy();
                        await UniTask.WaitUntil(() => _audioSource.isPlaying == false);
                    }

                    //ƒ‰ƒ“ƒ_ƒ€ŽžŠÔ‘Ò‹@
                    float waitTimeSeconds = UnityEngine.Random.Range(_minInterval, _maxInterval);
                    await UniTask.Delay(TimeSpan.FromSeconds(waitTimeSeconds));
                }
            }
            catch { }
        }
    }
}
