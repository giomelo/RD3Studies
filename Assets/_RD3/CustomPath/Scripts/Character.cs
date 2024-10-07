using System;
using DG.Tweening;
using UnityEngine;

namespace _RD3.CustomPath
{
    public class Character : MonoBehaviour, IMovable
    {
        private float _speed = 3f;
        public void SetMovePosition(Vector3 finalPos, Action onReached = null)
        {
            var distance = Vector3.Distance(transform.position, finalPos);
            transform.DOMove(finalPos, distance / _speed).SetEase(Ease.Linear).OnComplete(() =>
            {
                onReached?.Invoke();
            });
        }
    }
}