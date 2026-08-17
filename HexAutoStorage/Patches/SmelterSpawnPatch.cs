using HarmonyLib;
using HexAutoStorage.Configuration;

namespace HexAutoStorage.Patches
{
    [HarmonyPatch(typeof(Smelter), nameof(Smelter.Spawn))]
    internal static class SmelterSpawnPatch
    {
        [HarmonyPrefix]
        internal static bool Prefix(Smelter __instance, string ore, int stack)
        {
            if (Plugin.Instance == null || !StorageConfig.ModEnabled.Value)
            {
                return true;
            }

            Smelter.ItemConversion conversion = null;

            foreach (Smelter.ItemConversion itemConversion in __instance.m_conversion)
            {
                if (itemConversion.m_from != null && itemConversion.m_from.gameObject.name == ore)
                {
                    conversion = itemConversion;
                    break;
                }
            }

            if (conversion == null || conversion.m_to == null)
            {
                return true;
            }

            if (!AutoStorageService.TryStore(__instance, conversion.m_to, stack))
            {
                return true;
            }

            __instance.m_produceEffects.Create(__instance.transform.position, __instance.transform.rotation, null, 1f, -1);
            return false;
        }
    }
}