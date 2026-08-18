using HarmonyLib;
using HexAutoStorage.Configuration;
using HexAutoStorage.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace HexAutoStorage.Features
{
    internal class ContainerTagEditor : MonoBehaviour
    {
        private const string TagsKey = "HexAutoStorage_Tags";
        private const int MaxTagLength = 200;
        private const string TagEditorTitle = "Auto Storage Tags - Example: Copper,Tin,Bronze,Iron,Silver,Blackmetal,Flametal,Coal,Flour,Eitr,Linen";

        private static readonly Dictionary<string, string> ValidTags =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "Copper", "Copper" },
                { "Tin", "Tin" },
                { "Bronze", "Bronze" },
                { "Iron", "Iron" },
                { "Silver", "Silver" },
                { "Blackmetal", "BlackMetal" },
                { "Flametal", "FlametalNew" },
                { "Coal", "Coal" },
                { "Flour", "BarleyFlour" },
                { "Eitr", "Eitr" },
                { "Linen", "LinenThread" }
            };

        private static readonly Dictionary<string, string> DisplayTags =
            ValidTags.ToDictionary(entry => entry.Value, entry => entry.Key, StringComparer.OrdinalIgnoreCase);

        private static readonly MethodInfo TextInputShowMethod =
            AccessTools.Method(typeof(TextInput), "Show", new[] { typeof(string), typeof(string), typeof(int) });

        private static Container _editingContainer;

        internal static GameObject HoveredObject;

        internal static bool IsEditing => _editingContainer != null;

        internal static string GetEditTagsShortcutDisplay()
        {
            var shortcut = StorageConfig.EditTagsShortcut.Value;
            var keys = shortcut.Modifiers.Select(modifier => modifier.ToString()).ToList();

            keys.Add(shortcut.MainKey.ToString());

            return string.Join(" + ", keys);
        }

        private void Update()
        {
            if (Plugin.Instance == null || !StorageConfig.ModEnabled.Value || Player.m_localPlayer == null || IsEditing)
            {
                return;
            }

            if (StorageConfig.EditTagsShortcut.Value.IsDown())
            {
                TryOpenHoveredContainer();
            }
        }

        private void TryOpenHoveredContainer()
        {
            if (HoveredObject == null)
            {
                return;
            }

            Piece piece = HoveredObject.GetComponentInParent<Piece>();

            if (piece == null)
            {
                return;
            }

            string prefabName = piece.gameObject.name.Replace("(Clone)", "");

            if (!StoragePrefabRules.TryGetStorageType(prefabName, out StorageType _))
            {
                return;
            }

            Container container = piece.GetComponentInChildren<Container>(true);
            ZNetView nview = piece.GetComponent<ZNetView>();

            if (container == null || nview == null || !nview.IsValid())
            {
                return;
            }

            if (TextInput.instance == null)
            {
                Plugin.Log.LogDebug("Unable to open Auto Storage tag editor because TextInput.instance is null.");
                return;
            }

            _editingContainer = container;

            TextInputShowMethod.Invoke(
                TextInput.instance,
                new object[]
                {
                    TagEditorTitle,
                    GetDisplayTags(container),
                    MaxTagLength
                });

            Plugin.Log.LogDebug($"Opened Auto Storage tag editor for {prefabName}.");
        }

        private static bool TryNormalizeTags(string value, out string normalizedTags, out string invalidTag)
        {
            normalizedTags = "";
            invalidTag = "";

            if (string.IsNullOrWhiteSpace(value))
            {
                return true;
            }

            var normalized = new List<string>();

            foreach (string rawTag in value.Split(','))
            {
                string tag = rawTag.Trim();

                if (string.IsNullOrWhiteSpace(tag))
                {
                    continue;
                }

                if (!ValidTags.TryGetValue(tag, out string prefabName))
                {
                    invalidTag = tag;
                    return false;
                }

                if (!normalized.Contains(prefabName, StringComparer.OrdinalIgnoreCase))
                {
                    normalized.Add(prefabName);
                }
            }

            normalizedTags = string.Join(",", normalized);
            return true;
        }

        internal static string GetTags(Container container)
        {
            if (!TryGetContainerZNetView(container, out ZNetView nview))
            {
                return "";
            }

            return nview.GetZDO().GetString(TagsKey, "");
        }

        internal static bool HasTag(Container container, string prefabName)
        {
            if (string.IsNullOrWhiteSpace(prefabName))
            {
                return false;
            }

            string tags = GetTags(container);

            if (string.IsNullOrWhiteSpace(tags))
            {
                return false;
            }

            return tags
                .Split(',')
                .Any(tag => tag.Trim().Equals(prefabName, StringComparison.OrdinalIgnoreCase));
        }

        internal static void SaveTags(string text)
        {
            if (_editingContainer == null)
            {
                return;
            }

            if (!TryGetContainerZNetView(_editingContainer, out ZNetView nview))
            {
                _editingContainer = null;
                return;
            }

            if (!TryNormalizeTags(text, out string tags, out string invalidTag))
            {
                Player.m_localPlayer?.Message(MessageHud.MessageType.Center, $"Invalid Auto Storage tag: {invalidTag}");
                
                Plugin.Log.LogDebug($"Invalid Auto Storage tag '{invalidTag}' rejected.");

                _editingContainer = null;
                return;
            }

            Piece piece = _editingContainer.GetComponentInParent<Piece>();

            nview.GetZDO().Set(TagsKey, tags);

            Plugin.Log.LogDebug($"Saved Auto Storage tags '{tags}' to {piece?.gameObject.name ?? "container"}.");

            _editingContainer = null;
        }

        internal static void CancelEditing()
        {
            _editingContainer = null;
        }

        internal static string GetDisplayTags(Container container)
        {
            string tags = GetTags(container);

            if (string.IsNullOrWhiteSpace(tags))
            {
                return "";
            }

            var displayTags = new List<string>();

            foreach (string rawTag in tags.Split(','))
            {
                string prefabName = rawTag.Trim();

                if (DisplayTags.TryGetValue(prefabName, out string displayName))
                {
                    displayTags.Add(displayName);
                }
                else
                {
                    displayTags.Add(prefabName);
                }
            }

            return string.Join(",", displayTags);
        }

        private static bool TryGetContainerZNetView(Container container, out ZNetView nview)
        {
            nview = null;

            if (container == null)
            {
                return false;
            }

            Piece piece = container.GetComponentInParent<Piece>();

            if (piece == null)
            {
                return false;
            }

            nview = piece.GetComponent<ZNetView>();

            return nview != null && nview.IsValid();
        }
    }
}