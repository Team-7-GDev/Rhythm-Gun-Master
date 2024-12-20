using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "BotSpawnPosition", menuName = "Rhythm-Gun-Master/BotSpawnPosition")]
public class BotSpawnPositions : ScriptableObject
{
    public Vector3[] spawnPositions;

    public Vector3[] SpawnPositions()
    {
        return spawnPositions;
    }

    private void OnDrawGizmos()
    {
        if (spawnPositions.Length == 0)
            return;

        for (int i = 0; i < spawnPositions.Length; i++)
            Gizmos.DrawSphere(spawnPositions[i], 0.25f);
    }
}
