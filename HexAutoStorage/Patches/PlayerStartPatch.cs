using HarmonyLib;
using HexAutoStorage.Features;

namespace HexAutoStorage.Patches
{
    [HarmonyPatch(typeof(Player), nameof(Player.Start))]
    internal static class PlayerStartPatch
    {
        [HarmonyPostfix]
        internal static void Postfix(Player __instance)
        {
            if (Plugin.Instance == null || __instance == null)
            {
                return;
            }

            if (__instance != Player.m_localPlayer)
            {
                return;
            }

            if (__instance.GetComponent<ContainerTagEditor>() == null)
            {
                __instance.gameObject.AddComponent<ContainerTagEditor>();
                Plugin.Log.LogInfo("ContainerTagEditor added to local player.");
            }
        }
    }
}