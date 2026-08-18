using HarmonyLib;

namespace HexAutoStorage.Patches
{
    [HarmonyPatch(typeof(Smelter), nameof(Smelter.Awake))]
    internal static class SmelterTestSpeedPatch
    {
        [HarmonyPostfix]
        internal static void Postfix(Smelter __instance)
        {
            if (__instance == null)
            {
                return;
            }

            __instance.m_secPerProduct = 1f;
        }
    }
}
