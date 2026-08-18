using HarmonyLib;
using HexAutoStorage.Features;

namespace HexAutoStorage.Patches
{
    [HarmonyPatch(typeof(Smelter), nameof(Smelter.Awake))]
    internal static class SmelterAwakePatch
    {
        [HarmonyPostfix]
        internal static void Postfix(Smelter __instance)
        {
            if (Plugin.Instance == null || __instance == null)
            {
                return;
            }

            // Only add visualizer if it doesn't exist (guards against multiple Awake calls)
            if (__instance.GetComponent<StorageRadiusVisualizer>() == null)
            {
                __instance.gameObject.AddComponent<StorageRadiusVisualizer>();

                // Only log once when actually adding the component
                Plugin.Log.LogInfo($"StorageRadiusVisualizer added to {__instance.gameObject.name} (Instance: {__instance.GetInstanceID()}).");
            }
        }
    }
}