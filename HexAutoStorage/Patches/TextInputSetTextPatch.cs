using HarmonyLib;
using HexAutoStorage.Features;

namespace HexAutoStorage.Patches
{
    [HarmonyPatch(typeof(TextInput), nameof(TextInput.setText))]
    internal static class TextInputSetTextPatch
    {
        [HarmonyPostfix]
        internal static void Postfix(string text)
        {
            if (!ContainerTagEditor.IsEditing)
            {
                return;
            }

            ContainerTagEditor.SaveTags(text);
        }
    }

    [HarmonyPatch(typeof(TextInput), nameof(TextInput.Hide))]
    internal static class TextInputHidePatch
    {
        [HarmonyPostfix]
        internal static void Postfix()
        {
            if (!ContainerTagEditor.IsEditing)
            {
                return;
            }

            ContainerTagEditor.CancelEditing();
        }
    }
}