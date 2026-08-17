using HarmonyLib;
using HexAutoStorage.Features;
using UnityEngine;

namespace HexAutoStorage.Patches
{
    [HarmonyPatch(typeof(Player), "FindHoverObject")]
    internal static class PlayerFindHoverObjectPatch
    {
        [HarmonyPostfix]
        internal static void Postfix(Player __instance, GameObject hover)
        {
            if (__instance != Player.m_localPlayer)
            {
                return;
            }

            ContainerTagEditor.HoveredObject = hover;
        }
    }
}