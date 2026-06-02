using UnityEngine;
using System.IO;

namespace Survivor
{
    public class GameData
    {
        public bool InGame;

        public int[] AliveEnemyIndices;
        public int AliveEnemyCount;
        public int[] DeadEnemyIndices;
        public int DeadEnemyCount;

        public float SpawnTime;

        public Vector2[] EnemyPosition;

        public Vector2 PlayerDirection;

        public float GameTime;
    }
}