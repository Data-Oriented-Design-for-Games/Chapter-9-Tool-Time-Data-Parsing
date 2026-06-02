using UnityEngine;

namespace Survivor
{
    [CreateAssetMenu(fileName = "BalanceSO", menuName = "DOD/BalanceSO", order = 1)]
    public class BalanceSO : ScriptableObject
    {
        public int NumEnemies;
        public float EnemyVelocity;
        public float EnemyRadius;
        public float SpawnRadius;
        public float PlayerVelocity;
        public float MinCollisionDistance;
        public float SpawnTime;
    }
}