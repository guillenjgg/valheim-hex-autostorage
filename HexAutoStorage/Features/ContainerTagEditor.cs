using HarmonyLib;
using HexAutoStorage.Configuration;
using HexAutoStorage.Core;
using System;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace HexAutoStorage.Features
{
    internal class ContainerTagEditor : MonoBehaviour
    {
        private const string TagsKey = "HexAutoStorage_Tags";
        private const int MaxTagLength = 200;

        private static readonly MethodInfo TextInputShowMethod = AccessTools.Method(typeof(TextInput), "Show", new[] { typeof(string), typeof(string), typeof(int) });

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
            if (Plugin.Instance == null || !StorageConfig.ModEnabled.Value || Player.m_localPlayer == null)
            {
                return;
            }

            if (IsEditing)
            {
                return;
            }

            if (!StorageConfig.EditTagsShortcut.Value.IsDown())
            {
                return;
            }

            TryOpenHoveredContainer();
        }

        private void TryOpenHoveredContainer()
        {
            GameObject hover = HoveredObject;

            if (hover == null)
            {
                return;
            }

            Piece piece = hover.GetComponentInParent<Piece>();

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

            if (container == null)
            {
                return;
            }

            ZNetView nview = piece.GetComponent<ZNetView>();

            if (nview == null || !nview.IsValid())
            {
                return;
            }

            if (TextInput.instance == null)
            {
                Plugin.Log.LogWarning("Unable to open Auto Storage tag editor because TextInput.instance is null.");
                return;
            }

            _editingContainer = container;

            TextInputShowMethod.Invoke(
                TextInput.instance,
                new object[]
                {
                    "Auto Storage Tags",
                    GetTags(container),
                    MaxTagLength
                });

            Plugin.Log.LogInfo($"Opened Auto Storage tag editor for {prefabName}.");
        }

        private static string NormalizeTags(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "";
            }

            var tags = value
                .Split(',')
                .Select(tag => tag.Trim())
                .Where(tag => !string.IsNullOrWhiteSpace(tag))
                .Distinct(StringComparer.OrdinalIgnoreCase);

            return string.Join(",", tags);
        }

        internal static string GetTags(Container container)
        {
            if (container == null)
            {
                return "";
            }

            Piece piece = container.GetComponentInParent<Piece>();

            if (piece == null)
            {
                return "";
            }

            ZNetView nview = piece.GetComponent<ZNetView>();

            if (nview == null || !nview.IsValid())
            {
                return "";
            }

            return nview.GetZDO().GetString(TagsKey, "");
        }

        internal static bool HasTag(Container container, string tag)
        {
            if (string.IsNullOrWhiteSpace(tag))
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
                .Any(existingTag => existingTag.Trim().Equals(tag, StringComparison.OrdinalIgnoreCase));
        }

        internal static void SaveTags(string text)
        {
            if (_editingContainer == null)
            {
                return;
            }

            Piece piece = _editingContainer.GetComponentInParent<Piece>();

            if (piece == null)
            {
                _editingContainer = null;
                return;
            }

            ZNetView nview = piece.GetComponent<ZNetView>();

            if (nview == null || !nview.IsValid())
            {
                _editingContainer = null;
                return;
            }

            string tags = NormalizeTags(text);

            nview.GetZDO().Set(TagsKey, tags);

            Plugin.Log.LogInfo($"Saved Auto Storage tags '{tags}' to {piece.gameObject.name}.");

            _editingContainer = null;
        }

        internal static void CancelEditing()
        {
            _editingContainer = null;
        }
    }
}