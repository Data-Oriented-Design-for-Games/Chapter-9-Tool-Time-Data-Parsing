using UnityEngine;
using System;

namespace Survivor
{
    public static class Logic
    {
        public static void AllocateGameData(GameData gameData, Balance balance)
        {
            gameData.EnemyPosition = new Vector2[balance.NumEnemies];

            gameData.AliveEnemyIndices = new int[balance.NumEnemies];
            gameData.DeadEnemyIndices = new int[balance.NumEnemies];
        }

        public static void Init(MetaData metaData)
        {
            metaData.MenuState = MENU_STATE.NONE;
        }

        public static void StartGame(GameData gameData, Balance balance, float cameraSize, float screenRatio)
        {
            gameData.InGame = true;

            gameData.GameTime = 0.0f;
            gameData.SpawnTime = 0.0f;

            gameData.PlayerDirection = Vector2.zero;

            for (int i = 0; i < balance.NumEnemies; i++)
                gameData.DeadEnemyIndices[i] = balance.NumEnemies - 1 - i;
            gameData.DeadEnemyCount = balance.NumEnemies;
            gameData.AliveEnemyCount = 0;
        }

        static void spawnEnemy(GameData gameData, Balance balance, Span<int> addedEnemyIndices, ref int addedEnemyCount)
        {
            int enemyIndex = gameData.DeadEnemyIndices[--gameData.DeadEnemyCount];
            gameData.AliveEnemyIndices[gameData.AliveEnemyCount++] = enemyIndex;
            addedEnemyIndices[addedEnemyCount++] = enemyIndex;

            Vector2 direction = gameData.PlayerDirection;
            float angle;
            if (direction.magnitude == 0.0f)
            {
                direction = new Vector2(0.0f, 1.0f);
                angle = UnityEngine.Random.value * 360.0f;
            }
            else
            {
                direction = gameData.PlayerDirection;
                angle = UnityEngine.Random.value * 180.0f - 90.0f;
            }
            direction = RotateVector(direction, angle);
            gameData.EnemyPosition[enemyIndex] = direction.normalized * balance.SpawnRadius;
        }

        static void removeEnemy(GameData gameData, int enemyIndex, Span<int> removedEnemyIndices, ref int removedEnemyCount)
        {
            int count = 0;
            for (int i = 0; i < gameData.AliveEnemyCount; i++)
                if (gameData.AliveEnemyIndices[i] != enemyIndex)
                    gameData.AliveEnemyIndices[count++] = gameData.AliveEnemyIndices[i];
            gameData.AliveEnemyCount = count;

            gameData.DeadEnemyIndices[gameData.DeadEnemyCount++] = enemyIndex;
            removedEnemyIndices[removedEnemyCount++] = enemyIndex;
        }

        static void removeEnemyArrayCopy(GameData gameData, int enemyIndex)
        {
            int index = -1;
            // parse through the array, only re-store values that are not value
            for (int i = 0; i < gameData.AliveEnemyCount; i++)
                if (gameData.AliveEnemyIndices[i] == enemyIndex)
                {
                    index = i;
                    break;
                }

            if (index > -1)
                Array.Copy(gameData.AliveEnemyIndices, index + 1, gameData.AliveEnemyIndices, index, gameData.AliveEnemyCount - index - 1);

            gameData.AliveEnemyCount--;
        }

        private const double DegToRad = Math.PI / 180.0d;
        private const double RadToDeg = 180.0d / Math.PI;

        public static Vector2 RotateVector(Vector2 a, double degrees)
        {
            double radians = degrees * DegToRad;
            double ca = Math.Cos(radians);
            double sa = Math.Sin(radians);
            a.x = (float)(ca * a.x - sa * a.y);
            a.y = (float)(sa * a.x + ca * a.y);
            return a;
        }

        public static void Tick(
            MetaData metaData,
            GameData gameData,
            Balance balance,
            float dt,
            out bool gameOver,
            Span<int> addedEnemyIndices,
            ref int addedEnemyCount,
            Span<int> removedEnemyIndices,
            ref int removedEnemyCount
            )
        {
            gameData.GameTime += dt;

            gameData.SpawnTime += dt;
            if (gameData.SpawnTime >= balance.SpawnTime)
            {
                gameData.SpawnTime -= balance.SpawnTime;
                spawnEnemy(gameData, balance, addedEnemyIndices, ref addedEnemyCount);
            }

            moveEnemies(gameData, balance, dt);

            checkEnemyOutOfBounds(gameData, balance, removedEnemyIndices, ref removedEnemyCount);

            doEemyToEnemyCollision(gameData, balance);

            movePlayer(gameData, balance, dt);

            gameOver = checkGameOver(metaData, gameData, balance);
        }

        static void moveEnemies(GameData gameData, Balance balance, float dt)
        {
            for (int i = 0; i < gameData.AliveEnemyCount; i++)
            {
                int enemyIndex = gameData.AliveEnemyIndices[i];
                Vector2 dir = -gameData.EnemyPosition[enemyIndex].normalized;
                gameData.EnemyPosition[enemyIndex] = gameData.EnemyPosition[enemyIndex] + dir * balance.EnemyVelocity * dt;
            }
        }

        static void doEemyToEnemyCollision(GameData gameData, Balance balance)
        {
            float diameter = balance.EnemyRadius + balance.EnemyRadius;
            float diameterSqr = diameter * diameter;
            for (int i = 0; i < gameData.AliveEnemyCount; i++)
            {
                int enemyIndex1 = gameData.AliveEnemyIndices[i];
                for (int j = i + 1; j < gameData.AliveEnemyCount; j++)
                {
                    int enemyIndex2 = gameData.AliveEnemyIndices[j];
                    Vector2 diff = gameData.EnemyPosition[enemyIndex1] - gameData.EnemyPosition[enemyIndex2];
                    if (diff.sqrMagnitude <= diameterSqr)
                    {
                        Vector2 diffNormalized = diff.normalized;
                        Vector2 midPoint = (gameData.EnemyPosition[enemyIndex1] + gameData.EnemyPosition[enemyIndex2]) / 2.0f;
                        gameData.EnemyPosition[enemyIndex1] = midPoint + diffNormalized * balance.EnemyRadius;
                        gameData.EnemyPosition[enemyIndex2] = midPoint - diffNormalized * balance.EnemyRadius;
                    }
                }
            }
        }

        static void checkEnemyOutOfBounds(
            GameData gameData,
            Balance balance,
            Span<int> removedEnemyIndices,
            ref int removedEnemyCount)
        {
            float distanceSqr = balance.SpawnRadius * balance.SpawnRadius * 1.1f;

            Span<int> outOfBoundsEnemyIndices = stackalloc int[balance.NumEnemies];
            int outOfBoundsEnemyCount = 0;

            for (int i = 0; i < gameData.AliveEnemyCount; i++)
            {
                int enemyIndex = gameData.AliveEnemyIndices[i];

                if (gameData.EnemyPosition[enemyIndex].sqrMagnitude > distanceSqr)
                    outOfBoundsEnemyIndices[outOfBoundsEnemyCount++] = enemyIndex;
            }

            for (int i = 0; i < outOfBoundsEnemyCount; i++)
            {
                removeEnemy(
                    gameData,
                    outOfBoundsEnemyIndices[i],
                    removedEnemyIndices,
                    ref removedEnemyCount);
            }
        }

        static void movePlayer(GameData gameData, Balance balance, float dt)
        {
            Vector2 playerPosition = gameData.PlayerDirection * balance.PlayerVelocity * dt;
            for (int i = 0; i < gameData.AliveEnemyCount; i++)
            {
                int enemyIndex = gameData.AliveEnemyIndices[i];
                gameData.EnemyPosition[enemyIndex] -= playerPosition;
            }
        }

        public static void MouseMove(GameData gameData, Vector2 mouseDownPos, Vector2 mouseCurrentPos)
        {
            gameData.PlayerDirection = (mouseCurrentPos - mouseDownPos).normalized;
        }

        public static void MouseUp(GameData gameData)
        {
            gameData.PlayerDirection = Vector2.zero;
        }

        static bool checkGameOver(MetaData metaData, GameData gameData, Balance balance)
        {
            for (int i = 0; i < gameData.AliveEnemyCount; i++)
            {
                int enemyIndex = gameData.AliveEnemyIndices[i];
                if (gameData.EnemyPosition[enemyIndex].magnitude < balance.MinCollisionDistance)
                {
                    if (gameData.GameTime > metaData.BestTime)
                        metaData.BestTime = gameData.GameTime;

                    gameData.InGame = false;
                    return true;
                }
            }
            return false;
        }

        public static void SetMenuState(MetaData metaData, MENU_STATE newMenuState)
        {
            metaData.MenuState = newMenuState;
        }
    }
}