using UnityEngine;

namespace _RD3.RNA._Scripts
{
    public class SimulationSpeedController : MonoBehaviour
    {
        private float _fpsAvg = 0;
        private float _timeSum;
        public bool autoAdjust;
        public float gameSpeed = 1;


        private void Update() 
        {
            //if the space bar is pressed, change autoAdjust to the opposite of what it currently is
            if (Input.GetKeyDown(KeyCode.Space))
            {
                autoAdjust = !autoAdjust;
            }

            //calculate fps for update not fixed update
            float fps = 1.0f / Time.unscaledDeltaTime;

            //get moving average of fps
            _fpsAvg = (fps + _fpsAvg * 2) / 3;

            _timeSum += Time.deltaTime;

            if (_timeSum > 1)
            {
                _timeSum = 0;
                if(autoAdjust)
                    AdjustGameSpeed();
                else
                    Time.timeScale = gameSpeed;
            }
        }

        private void AdjustGameSpeed() 
        {
            gameSpeed = _fpsAvg switch
            {
                < 60 => gameSpeed * .9f,
                > 60 when gameSpeed < 100 => gameSpeed * 1.1f,
                _ => gameSpeed
            };

            switch (gameSpeed)
            {
                //keeps the game speed between 0.1 and 100 to prevent the game from freezing or crashing
                case < 0.1f:
                    gameSpeed = 0.1f;
                    break;
                case > 100:
                    gameSpeed = 100;
                    break;
            }

            Time.timeScale = gameSpeed;
        }
    }
}
