using HarmonyLib;
using HexAutoStorage.Core;
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

            if (hover == null)
            {
                SmelterHoverTracker.HoveredSmelter = null;
                return;
            }

            string prefabName = hover.gameObject.name.Replace("(Clone)", "");

            if(!StoragePrefabRules.IsSupportedSmelter(prefabName))
            {
                SmelterHoverTracker.HoveredSmelter = null;
                return;
            }

            SmelterHoverTracker.HoveredSmelter = hover.GetComponentInParent<Smelter>();
        }
    }
}