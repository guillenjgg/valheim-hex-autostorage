using HarmonyLib;
using HexAutoStorage.Configuration;
using HexAutoStorage.Core;
using HexAutoStorage.Features;

namespace HexAutoStorage.Patches
{
    [HarmonyPatch(typeof(Container), nameof(Container.GetHoverText))]
    [HarmonyAfter("shudnal.MyLittleUI")]
    internal static class ContainerHoverTextPatch
    {
        [HarmonyPostfix]
        internal static void Postfix(Container __instance, ref string __result)
        {
            if (Plugin.Instance == null || !StorageConfig.ModEnabled.Value || __instance == null)
            {
                return;
            }

            Piece piece = __instance.GetComponentInParent<Piece>();

            if (piece == null)
            {
                return;
            }

            string prefabName = piece.gameObject.name.Replace("(Clone)", "");

            if (!StoragePrefabRules.TryGetStorageType(prefabName, out StorageType _))
            {
                return;
            }

            string shortcut = ContainerTagEditor.GetEditTagsShortcutDisplay();

            __result += $"\n[<color=#ffff00ff><b>{shortcut}</b></color>] Edit Auto Storage Tags";
        }
    }
}