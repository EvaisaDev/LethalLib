#region

using UnityEngine;

#endregion

namespace LethalLib.Extras;

[CreateAssetMenu(menuName = "ScriptableObjects/SpawnableMapObject")]
public class SpawnableMapObjectDef : ScriptableObject
{
    public SpawnableMapObject spawnableMapObject;

    private IndoorMapHazardType runtimeCreatedIndoorMapHazardType;
    internal IndoorMapHazardType ConvertToIndoorMapHazardType()
    {
        if (runtimeCreatedIndoorMapHazardType != null)
        {
            return runtimeCreatedIndoorMapHazardType;
        }

        IndoorMapHazardType indoorMapHazardType = ScriptableObject.CreateInstance<IndoorMapHazardType>();
        if (spawnableMapObject == null)
        {
            return indoorMapHazardType;
        }

        indoorMapHazardType.prefabToSpawn = spawnableMapObject.prefabToSpawn;
        indoorMapHazardType.requireDistanceBetweenSpawns = spawnableMapObject.requireDistanceBetweenSpawns;
        indoorMapHazardType.disallowSpawningNearEntrances = spawnableMapObject.disallowSpawningNearEntrances;
        indoorMapHazardType.spawnFacingAwayFromWall = spawnableMapObject.spawnFacingAwayFromWall;
        indoorMapHazardType.spawnWithBackToWall = spawnableMapObject.spawnWithBackToWall;
        indoorMapHazardType.spawnWithBackFlushAgainstWall = spawnableMapObject.spawnWithBackFlushAgainstWall;
        indoorMapHazardType.spawnFacingWall = spawnableMapObject.spawnFacingWall;

        runtimeCreatedIndoorMapHazardType = indoorMapHazardType;
        return runtimeCreatedIndoorMapHazardType;
    }
}