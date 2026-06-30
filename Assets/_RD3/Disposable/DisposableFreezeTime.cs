using System;

namespace _RD3.Disposable
{
    public class DisposableFreezeTime : IDisposable
    {
        public DisposableFreezeTime()
        {
          //  TimeScaleService.SetTimeScale(0);
        }
        
        public void Dispose()
        {
            //  TimeScaleService.SetTimeScale(1);
        }
    }
}