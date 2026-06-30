using UnityEngine;

namespace _RD3.ServiceLocator
{
    public class AudioSystem : IAudioSystem
    {
        public void PlaySpawnSound()
        {
            Debug.Log("PlaySound000");
        }

        public void PlayOtherSound()
        {
            Debug.Log("PlaySound001");
        }
    }
}