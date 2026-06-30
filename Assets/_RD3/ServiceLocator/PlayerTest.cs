using System;
using UnityEngine;

namespace _RD3.ServiceLocator
{
    public class PlayerTest : MonoBehaviour
    {
        private IAudioSystem audioSystem;

        private void Awake()
        {
            audioSystem = ServiceLocator.Get<IAudioSystem>();
        }

        private void Start()
        {
            audioSystem.PlayOtherSound();
        }
    }
}