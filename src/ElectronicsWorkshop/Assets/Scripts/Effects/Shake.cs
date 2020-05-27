using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shake : MonoBehaviour
{
    private bool _moveRight;
    private bool _moveUp;

    private int _shakeCounter = 0;
    private bool _shakeInProgress = false;

    [SerializeField] private bool _shakeY = true;
    [SerializeField] private bool _shakeX = true;
    [SerializeField] private bool _continuousShaking = false;
    [SerializeField] private float _shakeDist = 5f;
    [SerializeField] private int _shakeCount = 5;
    [SerializeField] private float _shakeEverySec = 0.03f;

    private void Update()
    {
        if (!_shakeInProgress)
        {
            _shakeInProgress = true;
            StartCoroutine(TryShake());
        }

        if (_shakeCounter == _shakeCount)
        {
            transform.Translate(0, 0, 0); // Back to default position
        }

        if (_continuousShaking)
        {
            ShakeIt();
        }
    }

    public void ShakeIt()
    {
        if (_shakeCounter == _shakeCount)
        {
            _shakeCounter = 0;
        }
    }

    IEnumerator TryShake()
    {
        yield return new WaitForSeconds(_shakeEverySec);

        if (_shakeCounter < _shakeCount)
        {
            ShakeSelf();
            _shakeCounter++;
        }

        _shakeInProgress = false;
    }

    private void ShakeSelf()
    {
        if (_shakeX)
        {
            ShakeX();
        }

        if(_shakeY)
        {
            ShakeY();
        }
    }

    private void ShakeX()
    {
        if (_moveRight)
        {
            transform.Translate(_shakeDist, 0, 0);
            _moveRight = false;
        }
        else
        {
            transform.Translate(-_shakeDist, 0, 0);
            _moveRight = true;
        }
    }

    private void ShakeY()
    {
        if (_moveUp)
        {
            transform.Translate(0, _shakeDist, 0);
            _moveUp = false;
        }
        else
        {
            transform.Translate(0, -_shakeDist, 0);
            _moveUp = true;
        }
    }
}
