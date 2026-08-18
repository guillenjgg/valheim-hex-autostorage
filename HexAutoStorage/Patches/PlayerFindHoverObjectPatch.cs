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
                StorageRadiusVisualizer.HoveredSmelter = null;
                return;
            }

            Smelter smelter = hover.GetComponentInParent<Smelter>();

            if (smelter == null)
            {
                StorageRadiusVisualizer.HoveredSmelter = null;
                return;
            }

            string prefabName = smelter.gameObject.name.Replace("(Clone)", "");

            Plugin.Log.LogDebug($"FindHoverObject pathc Prefab name: {prefabName}");

            if (!StoragePrefabRules.IsSupportedSmelter(prefabName))
            {
                StorageRadiusVisualizer.HoveredSmelter = null;
                return;
            }

            StorageRadiusVisualizer.HoveredSmelter = smelter;
        }
    }
}