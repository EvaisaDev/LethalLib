using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Audio;

namespace LethalLib.Modules
{
    public class Utilities
    {
        public static List<GameObject> prefabsToFix = new List<GameObject>();
        public static List<GameObject> fixedPrefabs = new List<GameObject>();
        public static void Init()
        {
            On.SoundManager.Awake += SoundManager_Awake;
            On.MenuManager.Awake += MenuManager_Awake;
        }

        private static void MenuManager_Awake(On.MenuManager.orig_Awake orig, MenuManager self)
        {
            orig(self);
            // non diagetic mixer
            AudioMixer audioMixer = self.MenuAudio.outputAudioMixerGroup.audioMixer;

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
                    Plugin.logger.LogInfo($"Mixer group for {audioSource.name} in {prefab.name} is {audioSource.outputAudioMixerGroup.audioMixer.name}");

                    if (audioSource.outputAudioMixerGroup.audioMixer.name == "NonDiagetic")
                    {

                        var mixerGroup = audioMixer.FindMatchingGroups(audioSource.outputAudioMixerGroup.name)[0];

                        // check if group was found
                        if (mixerGroup != null)
                        {
                            audioSource.outputAudioMixerGroup = mixerGroup;
                            // log
                            Plugin.logger.LogInfo($"Fixed mixer group for {audioSource.name} in {prefab.name}");
                            // remove
                            prefabsToFix.RemoveAt(i);
                        }
                    }
                }
            }
        }

        private static void SoundManager_Awake(On.SoundManager.orig_Awake orig, SoundManager self)
        {
            orig(self);
            
            AudioMixer audioMixer = self.diageticMixer;

            for (int i = prefabsToFix.Count - 1; i >= 0; i--)
            {
                GameObject prefab = prefabsToFix[i];
                // get audio sources and then use string matching to find the correct mixer group
                AudioSource[] audioSources = prefab.GetComponentsInChildren<AudioSource>();
                foreach(AudioSource audioSource in audioSources)
                {
                    // check if any mixer group is assigned
                    if (audioSource.outputAudioMixerGroup == null)
                    {
                        Plugin.logger.LogInfo($"No mixer group for {audioSource.name} in {prefab.name}");
                        continue;
                    }

                    // log mixer group name
                    Plugin.logger.LogInfo($"Mixer group for {audioSource.name} in {prefab.name} is {audioSource.outputAudioMixerGroup.audioMixer.name}");

                    if (audioSource.outputAudioMixerGroup.audioMixer.name == "Diagetic")
                    {

                        var mixerGroup = audioMixer.FindMatchingGroups(audioSource.outputAudioMixerGroup.name)[0];

                        // check if group was found
                        if (mixerGroup != null)
                        {
                            audioSource.outputAudioMixerGroup = mixerGroup;
                            // log
                            Plugin.logger.LogInfo($"Fixed mixer group for {audioSource.name} in {prefab.name}");
                            // remove
                            prefabsToFix.RemoveAt(i);
                        }
                    }
                }

            }
        }

        public static void FixMixerGroups(GameObject prefab)
        {
            if(fixedPrefabs.Contains(prefab))
            {
                return;
            }

            Plugin.logger.LogInfo($"Fixing mixer groups for {prefab.name}");

            fixedPrefabs.Add(prefab);

            prefabsToFix.Add(prefab);
        }
    }
}
