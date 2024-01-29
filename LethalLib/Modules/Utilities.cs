#region

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

#endregion

namespace LethalLib.Modules;

public class Utilities
{
    public static List<GameObject> prefabsToFix = new List<GameObject>();
    public static List<GameObject> fixedPrefabs = new List<GameObject>();
    public static void Init()
    {
        On.StartOfRound.Start += StartOfRound_Start;
        On.MenuManager.Start += MenuManager_Start;
    }

    private static void StartOfRound_Start(On.StartOfRound.orig_Start orig, StartOfRound self)
    {
        AudioMixer audioMixer = SoundManager.Instance.diageticMixer;

        // log
        if (Plugin.extendedLogging.Value)
            Plugin.logger.LogInfo($"Diagetic mixer is {audioMixer.name}");

        Plugin.logger.LogInfo($"Found {prefabsToFix.Count} prefabs to fix");

        List<GameObject> prefabsToRemove = new List<GameObject>();

        for (int i = prefabsToFix.Count - 1; i >= 0; i--)
        {
            GameObject prefab = prefabsToFix[i];
            // get audio sources and then use string matching to find the correct mixer group
            AudioSource[] audioSources = prefab.GetComponentsInChildren<AudioSource>();
            foreach (AudioSource audioSource in audioSources)
            {
                // check if any mixer group is assigned
                if (audioSource.outputAudioMixerGroup == null)
                {
                    //Plugin.logger.LogInfo($"No mixer group for {audioSource.name} in {prefab.name}");
                    continue;
                }

                // log mixer group name
                //Plugin.logger.LogInfo($"Mixer group for {audioSource.name} in {prefab.name} is {audioSource.outputAudioMixerGroup.audioMixer.name}");

                if (audioSource.outputAudioMixerGroup.audioMixer.name == "Diagetic")
                {

                    var mixerGroup = audioMixer.FindMatchingGroups(audioSource.outputAudioMixerGroup.name)[0];

                    // check if group was found
                    if (mixerGroup != null)
                    {
                        audioSource.outputAudioMixerGroup = mixerGroup;
                        // log
                        if (Plugin.extendedLogging.Value)
                            Plugin.logger.LogInfo($"Set mixer group for {audioSource.name} in {prefab.name} to Diagetic:{mixerGroup.name}");

                        // remove from list
                        prefabsToRemove.Add(prefab);
                    }
                }
            }

        }

        // remove fixed prefabs from list
        foreach (GameObject prefab in prefabsToRemove)
        {
            prefabsToFix.Remove(prefab);
        }

        orig(self);
    }

    private static void MenuManager_Start(On.MenuManager.orig_Start orig, MenuManager self)
    {
        orig(self);

        if(self.GetComponent<AudioSource>() == null)
        {
            return;
        }
        // non diagetic mixer
        AudioMixer audioMixer = self.GetComponent<AudioSource>().outputAudioMixerGroup.audioMixer;

        List<GameObject> prefabsToRemove = new List<GameObject>();
        // reverse loop so we can remove items
        for (int i = prefabsToFix.Count - 1; i >= 0; i--)
        {
            GameObject prefab = prefabsToFix[i];
            // get audio sources and then use string matching to find the correct mixer group
            AudioSource[] audioSources = prefab.GetComponentsInChildren<AudioSource>();
            foreach (AudioSource audioSource in audioSources)
            {
                // check if any mixer group is assigned
                if (audioSource.outputAudioMixerGroup == null)
                {
                    continue;
                }

                // log mixer group name
                //Plugin.logger.LogInfo($"Mixer group for {audioSource.name} in {prefab.name} is {audioSource.outputAudioMixerGroup.audioMixer.name}");

                if (audioSource.outputAudioMixerGroup.audioMixer.name == "NonDiagetic")
                {

                    var mixerGroup = audioMixer.FindMatchingGroups(audioSource.outputAudioMixerGroup.name)[0];

                    // check if group was found
                    if (mixerGroup != null)
                    {
                        audioSource.outputAudioMixerGroup = mixerGroup;
                        // log
                        if (Plugin.extendedLogging.Value)
                            Plugin.logger.LogInfo($"Set mixer group for {audioSource.name} in {prefab.name} to NonDiagetic:{mixerGroup.name}");

                        // remove from list
                        prefabsToRemove.Add(prefab);
                    }
                }
            }
        }

        // remove fixed prefabs from list
        foreach (GameObject prefab in prefabsToRemove)
        {
            prefabsToFix.Remove(prefab);
        }
    }

    public static void FixMixerGroups(GameObject prefab)
    {
        if(fixedPrefabs.Contains(prefab))
        {
            return;
        }

        //Plugin.logger.LogInfo($"Fixing mixer groups for {prefab.name}");

        fixedPrefabs.Add(prefab);

        prefabsToFix.Add(prefab);
    }
}
