using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

namespace Survivor
{
    [Serializable]
    public class Balance
    {
        public int NumEnemies;
        public float EnemyVelocity;
        public float EnemyRadius;
        public float SpawnRadius;
        public float PlayerVelocity;
        public float MinCollisionDistance;
        public float SpawnTime;

        public void LoadBalance()
        {
            TextAsset asset = Resources.Load("balance") as TextAsset;
            LoadBalance(asset.bytes);
        }

        public void LoadBalance(byte[] array)
        {
            Stream s = new MemoryStream(array);
            using (BinaryReader br = new BinaryReader(s))
            {
                int version = br.ReadInt32();

                NumEnemies = br.ReadInt32();
                EnemyVelocity = br.ReadSingle();
                EnemyRadius = br.ReadSingle();
                SpawnRadius = br.ReadSingle();
                PlayerVelocity = br.ReadSingle();
                MinCollisionDistance = br.ReadSingle();
                SpawnTime = br.ReadSingle();

                int magic = br.ReadInt32();
                Debug.Log(magic);
            }
        }
    }
}