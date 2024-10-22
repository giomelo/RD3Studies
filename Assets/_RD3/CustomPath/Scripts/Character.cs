using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace _RD3.CustomPath
{
    public class Character : MonoBehaviour, IMovable
    {
        private float _speed = 3f;
        private Coroutine _routine;
        
        public void SetMovePosition(Vector3 finalPos, Action onReached = null)
        {
            if(_routine != null) StopCoroutine(_routine);
            _routine = StartCoroutine(MoveToPosition(finalPos, onReached));
        }

        private IEnumerator MoveToPosition(Vector3 finalPos, Action onReached)
        {
            var startPos = transform.position;
            var distance = Vector3.Distance(startPos, finalPos);
            var duration = distance / _speed;
            var elapsedTime = 0f;

            while (elapsedTime < duration)
            {
                transform.position = Vector3.Lerp(startPos, finalPos, elapsedTime / duration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            transform.position = finalPos;
            onReached?.Invoke();
        }
    }
}