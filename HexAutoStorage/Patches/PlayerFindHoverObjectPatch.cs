using HarmonyLib;
using HexAutoStorage.Features;
using UnityEngine;

namespace HexAutoStorage.Patches
{
    [HarmonyPatch(typeof(Player), nameof(Player.FindHoverObject))]
    internal static class PlayerFindHoverObjectPatch
    {
        [HarmonyPostfix]
        internal static void Postfix(Player __instance, GameObject hover)
        {
            if (__instance != Player.m_localPlayer)
            {
                return;
            }

            if (hover == null)
            {
                SmelterHoverTracker.HoveredSmelter = null;
                return;
            }

            SmelterHoverTracker.HoveredSmelter = hover.GetComponentInParent<Smelter>();
        }
    }
}