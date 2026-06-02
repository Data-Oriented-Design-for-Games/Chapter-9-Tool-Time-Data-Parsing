using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Survivor
{
    public static class CommonVisual
    {
        public static string GetTimeElapsedString(float time)
        {
            int totalSeconds = (int)time;
            int minutes = totalSeconds / 60;
            int seconds = totalSeconds % 60;

            return $"{minutes:00}:{seconds:00}";
        }
    }
}