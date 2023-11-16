using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace LethalLib.Modules
{
    public class Shaders
    {

        public static void FixShaders(GameObject gameObject)
        {
            foreach (var renderer in gameObject.GetComponentsInChildren<Renderer>())
            {
                foreach (var material in renderer.materials)
                {
                    if (material.shader.name.Contains("Standard"))
                    {
                        // ge
                        material.shader = Shader.Find("HDRP/Lit");
                    }
                }
            }
        }
    }
}
