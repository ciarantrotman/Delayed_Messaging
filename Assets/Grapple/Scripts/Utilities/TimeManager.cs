using System;
using UnityEngine;

namespace Grapple.Scripts
{
    public class TimeManager: MonoBehaviour
    {
        private float duration = 5f;
        private const float TimeScaleFactor = .02f;
        private bool staySlow;
        [Serializable] public struct SlowTimeData { public float duration, factor; public bool staySlow; }
        
        private void Update()
        {
            Time.timeScale += (1f / duration) * Time.unscaledDeltaTime;
            
            // Only return to standard speed if stay slow is false
            if (staySlow) return;
            Time.timeScale = Mathf.Clamp(Time.timeScale, 0f, 1f);
        }
        /// <summary>
        /// Requires a slow time data struct to dictate the time slow factor and time slow duration 
        /// </summary>
        /// <param name="slowTimeData"></param>
        public void SlowTime(SlowTimeData slowTimeData)
        {
            duration = slowTimeData.duration;
            staySlow = slowTimeData.staySlow;
            Time.timeScale = slowTimeData.factor;
            Time.fixedDeltaTime = Time.timeScale * TimeScaleFactor;
        }
    }
}
