using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.TextCore.Text;


namespace Survivor
{
    public class BalanceParser : MonoBehaviour
    {
#if UNITY_EDITOR
        [MenuItem("DOD/Balance/Parse Local")]
        public static void ParseLocal()
        {
            Debug.Log("Parse balance started!");

            if (!Directory.Exists(Application.persistentDataPath + "/Resources"))
                Directory.CreateDirectory(Application.persistentDataPath + "/Resources");

            validate();
            byte[] array = parse();
            // save array
            string path = "Assets/Resources/balance.bytes";
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            using (FileStream fs = File.Create(path))
            using (BinaryWriter bw = new BinaryWriter(fs))
                bw.Write(array);

            Debug.Log("Parse balance finished!");

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        static void validate()
        {
            BalanceSO balanceSO = (BalanceSO)AssetDatabase.LoadAssetAtPath("Assets/Data/Balance.asset", typeof(BalanceSO));

            if (balanceSO.NumEnemies == 0)
                Debug.LogError("NumEnemies set to 0!");
            // if (balanceSO.EnemyVelocityMin > balanceSO.EnemyVelocityMax)
            //     Debug.LogError("EnemyVelocityMin is larger than EnemyVelocityMax!");
        }

        static byte[] parse()
        {
            using (MemoryStream stream = new MemoryStream())
            {
                using (BinaryWriter bw = new BinaryWriter(stream))
                {
                    int version = 1;
                    bw.Write(version);

                    BalanceSO balanceSO = (BalanceSO)AssetDatabase.LoadAssetAtPath("Assets/Data/Balance.asset", typeof(BalanceSO));

                    bw.Write(balanceSO.NumEnemies);
                    bw.Write(balanceSO.EnemyVelocity);
                    bw.Write(balanceSO.EnemyRadius);
                    bw.Write(balanceSO.SpawnRadius);
                    bw.Write(balanceSO.PlayerVelocity);
                    bw.Write(balanceSO.MinCollisionDistance);
                    bw.Write(balanceSO.SpawnTime);

                    int magic = 123456789;
                    bw.Write(magic);
                    return stream.ToArray();
                }
            }
        }
#endif
    }
}
