// Miguel Rodríguez Gallego
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace AICookbook.EditorTools
{
    /// <summary>
    ///     The Minimax demo (05) came from a project saved in binary mode, so its scenes, prefabs and
    ///     materials are still binary files. The project is in Force Text mode, but Unity only rewrites
    ///     an asset when it is saved again. This menu rewrites them all as text (YAML) at once.
    /// </summary>
    static class ReserializeTools
    {
        const string MinimaxFolder = "Assets/05_Minimax";

        static readonly string[] SerializedExtensions =
            { ".unity", ".prefab", ".mat", ".asset", ".controller", ".anim", ".physicMaterial", ".overrideController" };

        [MenuItem("Tools/AI Cookbook/Convert Minimax (05) assets to text")]
        static void ConvertMinimaxToText()
        {
            if (EditorSettings.serializationMode != SerializationMode.ForceText)
            {
                Debug.LogError("Set Edit > Project Settings > Editor > Asset Serialization > Mode to Force Text first.");
                return;
            }

            string[] paths = AssetDatabase.FindAssets("", new[] { MinimaxFolder })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(path => SerializedExtensions.Any(ext => path.EndsWith(ext)))
                .Distinct()
                .ToArray();

            AssetDatabase.ForceReserializeAssets(paths, ForceReserializeAssetsOptions.ReserializeAssets);
            AssetDatabase.SaveAssets();

            Debug.Log($"[AI Cookbook] {paths.Length} assets in {MinimaxFolder} rewritten as text:\n" + string.Join("\n", paths));
        }
    }
}
