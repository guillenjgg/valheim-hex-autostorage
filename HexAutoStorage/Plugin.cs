using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace HexAutoStorage
{
    [BepInPlugin(PluginGuid, PluginName, PluginVersion)]
    public class Plugin : BaseUnityPlugin
    {
        private const string PluginGuid = "com.hex.autostorage";
        private const string PluginName = "HexAutoStorage";
        private const string PluginVersion = "1.0.0";

        private Harmony _harmonyInstance;

        internal static ManualLogSource Log;
        internal static Plugin Instance;

        internal static ConfigEntry<bool> ModEnabled;
        internal static ConfigEntry<float> StorageRadius;
        internal static ConfigEntry<KeyboardShortcut> EditTagsShortcut;
        internal static ConfigEntry<bool> ShowStorageRadius;

        private void Awake()
        {
            Instance = this;
            Log = Logger;

            BindConfig();

            Assembly assembly = Assembly.GetExecutingAssembly();
            _harmonyInstance = new Harmony(PluginGuid);
            _harmonyInstance.PatchAll(assembly);

            Log.LogInfo($"{PluginName} v{PluginVersion} loaded.");
        }

        private void BindConfig()
        {
            ModEnabled = Config.Bind("General", "Enabled", true, "Enable or disable HexAutoStorage.");

            StorageRadius = Config.Bind(
            "General",
            "StorageRadius",
            5f,
            new ConfigDescription(
                "Radius in meters that production stations search for nearby containers.",
                new AcceptableValueRange<float>(5f, 100f)));
            
            EditTagsShortcut = Config.Bind("Input", "EditTagsShortcut", new KeyboardShortcut(KeyCode.T, KeyCode.LeftShift), "Keyboard shortcut used to edit Auto Storage tags while looking at a container.");

            ShowStorageRadius = Config.Bind("General", "ShowRadiusVisual", false, "Show storage radius outline");
        }

        private void OnDestroy()
        {
            Log.LogInfo($"{PluginName} v{PluginVersion} unloaded.");

            _harmonyInstance?.UnpatchSelf();
            _harmonyInstance = null;
            Instance = null;
            Log = null;
        }
    }
}