using System.Threading.Tasks;
using UnityEngine;

namespace _RD3.Disposable
{
    public class ManagerTest : MonoBehaviour
    {
        private Animator _animator;        
        
        public async Task DoVictorySequence()
        {
            using (new DisposableLockKeyboardInput())
                using(new DisposableFreezeTime())
                using(new DisposableLockUIInput(new CanvasGroup()))
            {
                // shake screen
                // play victorySound

                await PlayVictoryAnimation();
            }
        }

        private async Task PlayVictoryAnimation()
        {
            _animator.Play("A");

            await Task.Yield(); 

            float animationLength = _animator.GetCurrentAnimatorStateInfo(0).length;
            await Task.Delay((int)(animationLength * 1000));
        }
    }
}