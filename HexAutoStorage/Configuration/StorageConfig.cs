using BepInEx.Configuration;
using BepInEx.Configuration;
using UnityEngine;

namespace HexAutoStorage.Configuration
{
    internal static class StorageConfig
    {
        internal static ConfigEntry<bool> ModEnabled;
        internal static ConfigEntry<float> StorageRadius;
        internal static ConfigEntry<KeyboardShortcut> EditTagsShortcut;
        internal static ConfigEntry<bool> ShowStorageRadius;

        internal static void Bind(ConfigFile config)
        {
            ModEnabled = config.Bind(
                "General",
                "Enabled",
                true,
                "Enable or disable HexAutoStorage.");

            StorageRadius = config.Bind(
                "General",
                "StorageRadius",
                5f,
                new ConfigDescription(
                    "Radius in meters that production stations search for nearby containers.",
                    new AcceptableValueRange<float>(5f, 100f)));

            ShowStorageRadius = config.Bind(
                "General",
                "ShowRadiusVisual",
                false,
                "Show storage radius outline.");

            EditTagsShortcut = config.Bind(
                "Input",
                "EditTagsShortcut",
                new KeyboardShortcut(KeyCode.LeftShift, KeyCode.P),
                "Keyboard shortcut used to edit Auto Storage tags while looking at a container. " +
                "Supported tags: Copper, Tin, Bronze, Iron, Silver, Blackmetal, Flametal, Coal, Flour, Eitr.");
        }
    }
}