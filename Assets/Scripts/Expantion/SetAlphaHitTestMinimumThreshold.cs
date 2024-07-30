using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SetAlphaHitTestMinimumThreshold : MonoBehaviour
{
    [SerializeField] private Image _image;
    [SerializeField] private float _alphaThreshold = 0.1f;

    private void Awake()
    {
        if (_image != null)
        {
            _image.alphaHitTestMinimumThreshold = _alphaThreshold;
        }
    }
}
