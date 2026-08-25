namespace _RD3.ServiceLocator
{
    public interface IAudioSystem : IService
    {
        void PlaySpawnSound();
        void PlayOtherSound();
    }
}