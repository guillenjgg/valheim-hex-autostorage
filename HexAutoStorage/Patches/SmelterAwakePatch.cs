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

            if (__instance.GetComponent<StorageRadiusVisualizer>() == null)
            {
                __instance.gameObject.AddComponent<StorageRadiusVisualizer>();

#if DEBUG
                __instance.m_secPerProduct = 1f;
                Plugin.Log.LogInfo($"StorageRadiusVisualizer added to {__instance.gameObject.name} (Instance: {__instance.GetInstanceID()}).");
#endif
            }
        }
    }
}