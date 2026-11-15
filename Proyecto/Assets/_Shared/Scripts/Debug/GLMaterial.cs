using UnityEngine;
using UnityEngine.Rendering;

namespace AICookbook.Shared
{
    /// <summary>
    /// Shared material for immediate-mode GL drawing: vertex colours, alpha blending,
    /// no culling, drawn on top of everything. Created once and reused by every demo.
    /// </summary>
    public static class GLMaterial
    {
        static Material material;

        /// <summary>Returns the material, or null if no suitable shader is available.</summary>
        public static Material Get()
        {
            if (material != null) return material;

            var shader = Shader.Find("Hidden/AICookbook/GLColored");
            bool builtIn = false;
            if (shader == null) { shader = Shader.Find("Hidden/Internal-Colored"); builtIn = true; }
            if (shader == null) return null;

            material = new Material(shader) { hideFlags = HideFlags.HideAndDontSave };
            if (builtIn)
            {
                // The built-in shader exposes its render state as properties.
                material.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha);
                material.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
                material.SetInt("_Cull", (int)CullMode.Off);
                material.SetInt("_ZWrite", 0);
                material.SetInt("_ZTest", (int)CompareFunction.Always);
            }
            return material;
        }
    }
}
