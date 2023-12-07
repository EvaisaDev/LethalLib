using LethalLib.Extras;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Unity.Netcode;
using UnityEngine;
using static LethalLib.Modules.Items;

namespace LethalLib.Modules
{
    public class MapObjects
    {
        public static void Init()
        {
            On.StartOfRound.Awake += StartOfRound_Awake;
            On.RoundManager.SpawnMapObjects += RoundManager_SpawnMapObjects;
        }

        private static void RoundManager_SpawnMapObjects(On.RoundManager.orig_SpawnMapObjects orig, RoundManager self)
        {
            RandomMapObject[] array = UnityEngine.Object.FindObjectsOfType<RandomMapObject>();

            foreach (RandomMapObject randomMapObject in array)
            {
                foreach (RegisteredMapObject mapObject in mapObjects)
                {
                    if (mapObject.mapObject != null)
                    {
                        if (!randomMapObject.spawnablePrefabs.Any((prefab) => prefab == mapObject.mapObject.prefabToSpawn))
                        {
                            randomMapObject.spawnablePrefabs.Add(mapObject.mapObject.prefabToSpawn);
                        }
                    }
                }
            }

            orig(self);
            /*
            if (self.currentLevel.spawnableMapObjects.Length == 0)
            {
                return;
            }
            self.mapPropsContainer = GameObject.FindGameObjectWithTag("MapPropsContainer");
            
            List<RandomMapObject> list = new List<RandomMapObject>();
            for (int i = 0; i < self.currentLevel.spawnableMapObjects.Length; i++)
            {
                var ran = (float)self.AnomalyRandom.NextDouble();
                Plugin.logger.LogInfo($"Random value: {ran}");
                var val = self.currentLevel.spawnableMapObjects[i].numberToSpawn.Evaluate(ran);
                Plugin.logger.LogInfo($"Evaluated value: {val}");
                int num = (int)val;
                // print
                Plugin.logger.LogInfo($"Spawning {self.currentLevel.spawnableMapObjects[i].prefabToSpawn.name} {num} times");
                if (num <= 0)
                {
                    continue;
                }
                for (int j = 0; j < array.Length; j++)
                {
                    if (array[j].spawnablePrefabs.Contains(self.currentLevel.spawnableMapObjects[i].prefabToSpawn))
                    {
                        list.Add(array[j]);
                    }
                }
                for (int k = 0; k < num; k++)
                {
                    RandomMapObject randomMapObject = list[self.AnomalyRandom.Next(0, list.Count)];
                    Vector3 position = randomMapObject.transform.position;
                    position = self.GetRandomNavMeshPositionInRadius(position, randomMapObject.spawnRange);
                    GameObject gameObject = UnityEngine.Object.Instantiate(self.currentLevel.spawnableMapObjects[i].prefabToSpawn, position, Quaternion.identity, self.mapPropsContainer.transform);
                    if (self.currentLevel.spawnableMapObjects[i].spawnFacingAwayFromWall)
                    {
                        gameObject.transform.eulerAngles = new Vector3(0f, self.YRotationThatFacesTheFarthestFromPosition(position + Vector3.up * 0.2f), 0f);
                    }
                    else
                    {
                        gameObject.transform.eulerAngles = new Vector3(gameObject.transform.eulerAngles.x, self.AnomalyRandom.Next(0, 360), gameObject.transform.eulerAngles.z);
                    }
                    gameObject.GetComponent<NetworkObject>().Spawn(destroyWithScene: true);
                }
            }
            for (int l = 0; l < array.Length; l++)
            {
                UnityEngine.Object.Destroy(array[l].gameObject);
            }*/
        }

        private static void StartOfRound_Awake(On.StartOfRound.orig_Awake orig, StartOfRound self)
        {
            orig(self);

            foreach (SelectableLevel level in self.levels)
            {
                var name = level.name;

                if (Enum.IsDefined(typeof(Levels.LevelTypes), name))
                {
                    var levelEnum = (Levels.LevelTypes)Enum.Parse(typeof(Levels.LevelTypes), name);

                    foreach (RegisteredMapObject mapObject in mapObjects)
                    {
                        if (mapObject.levels.HasFlag(levelEnum))
                        {
                            if (mapObject.mapObject != null)
                            {
                                if (!level.spawnableMapObjects.Any(x => x.prefabToSpawn == mapObject.mapObject.prefabToSpawn))
                                {
                                    // remove the object from the list
                                    var list = level.spawnableMapObjects.ToList();
                                    list.RemoveAll(x => x.prefabToSpawn == mapObject.mapObject.prefabToSpawn);
                                    level.spawnableMapObjects = list.ToArray();
                                }

                                SpawnableMapObject spawnableMapObject = mapObject.mapObject;
                                if (mapObject.spawnRateFunction != null)
                                {
                                    spawnableMapObject.numberToSpawn = mapObject.spawnRateFunction(level);
                                }
                                var mapObjectsList = level.spawnableMapObjects.ToList();
                                mapObjectsList.Add(spawnableMapObject);
                                level.spawnableMapObjects = mapObjectsList.ToArray();
                                Plugin.logger.LogInfo($"Added {spawnableMapObject.prefabToSpawn.name} to {name}");
                            }
                            else if (mapObject.outsideObject != null)
                            {
                                if (!level.spawnableOutsideObjects.Any(x => x.spawnableObject.prefabToSpawn == mapObject.outsideObject.spawnableObject.prefabToSpawn))
                                {
                                    // remove the object from the list
                                    var list = level.spawnableOutsideObjects.ToList();
                                    list.RemoveAll(x => x.spawnableObject.prefabToSpawn == mapObject.outsideObject.spawnableObject.prefabToSpawn);
                                    level.spawnableOutsideObjects = list.ToArray();
                                }

                                SpawnableOutsideObjectWithRarity spawnableOutsideObject = mapObject.outsideObject;
                                if (mapObject.spawnRateFunction != null)
                                {
                                    spawnableOutsideObject.randomAmount = mapObject.spawnRateFunction(level);
                                }   
                                var mapObjectsList = level.spawnableOutsideObjects.ToList();
                                mapObjectsList.Add(spawnableOutsideObject);
                                level.spawnableOutsideObjects = mapObjectsList.ToArray();
                                Plugin.logger.LogInfo($"Added {spawnableOutsideObject.spawnableObject.prefabToSpawn.name} to {name}");
                            }
                        }
                    }
                }
            }
        }

        public class RegisteredMapObject
        {
            public SpawnableMapObject mapObject;
            public SpawnableOutsideObjectWithRarity outsideObject;
            public Levels.LevelTypes levels;
            public Func<SelectableLevel, AnimationCurve> spawnRateFunction;
        }

        public static List<RegisteredMapObject> mapObjects = new List<RegisteredMapObject>();

        public static void RegisterMapObject(SpawnableMapObjectDef mapObject, Levels.LevelTypes levels, Func<SelectableLevel, AnimationCurve> spawnRateFunction = null)
        {
            RegisterMapObject(mapObject.spawnableMapObject, levels, spawnRateFunction);
        }

        public static void RegisterMapObject(SpawnableMapObject mapObject, Levels.LevelTypes levels, Func<SelectableLevel, AnimationCurve> spawnRateFunction = null)
        {
            mapObjects.Add(new RegisteredMapObject
            {
                mapObject = mapObject,
                levels = levels,
                spawnRateFunction = spawnRateFunction
            });
        }

        public static void RegisterOutsideObject(SpawnableOutsideObjectDef mapObject, Levels.LevelTypes levels, Func<SelectableLevel, AnimationCurve> spawnRateFunction = null)
        {
            RegisterOutsideObject(mapObject.spawnableMapObject, levels, spawnRateFunction);
        }

        public static void RegisterOutsideObject(SpawnableOutsideObjectWithRarity mapObject, Levels.LevelTypes levels, Func<SelectableLevel, AnimationCurve> spawnRateFunction = null)
        {
            mapObjects.Add(new RegisteredMapObject
            {
                outsideObject = mapObject,
                levels = levels,
                spawnRateFunction = spawnRateFunction
            });
        }

    }
}
