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
        private const string SetTagsRpc = "HexAutoStorage_SetTags";
        private const string TagsResultRpc = "HexAutoStorage_TagsResult";
        private const string TagEditorTitle = "Auto Storage Tags - Example: Copper,Tin,Bronze,Iron,Silver,Blackmetal,Flametal,Coal,Flour,Eitr,Linen,Bloodgold,LiquidFrost";

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
                { "Linen", "LinenThread" },
                { "Bloodgold", "Gold" },
                { "LiquidFrost", "FrozenFuel" }
            };

        private static readonly Dictionary<string, string> DisplayTags =
            ValidTags.ToDictionary(entry => entry.Value, entry => entry.Key, StringComparer.OrdinalIgnoreCase);

        private static readonly MethodInfo TextInputShowMethod =
            AccessTools.Method(typeof(TextInput), "Show", new[] { typeof(string), typeof(string), typeof(int) });

        private static Container _editingContainer;
        private static Container _pendingContainer;
        private static long _pendingOwner;
        private static long _requestSequence;
        private static long _pendingRequest;
        private static float _pendingSince;

        private static readonly MethodInfo CheckContainerAccess = AccessTools.Method(typeof(Container), "CheckAccess");
        private static readonly FieldInfo AllWards = AccessTools.Field(typeof(PrivateArea), "m_allAreas");
        private static readonly MethodInfo WardEnabled = AccessTools.Method(typeof(PrivateArea), "IsEnabled");
        private static readonly MethodInfo WardContains = AccessTools.Method(typeof(PrivateArea), "IsInside");
        private static readonly MethodInfo WardPermitted = AccessTools.Method(typeof(PrivateArea), "IsPermitted");

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
            if (_pendingContainer != null)
            {
                if (Time.realtimeSinceStartup - _pendingSince >= 10f)
                {
                    _pendingContainer = null;
                    StorageConfig.DebugLog("Auto Storage tag save timed out without owner confirmation.");
                    Player.m_localPlayer?.Message(MessageHud.MessageType.Center,
                        "Auto Storage tag save was not confirmed. The chest owner must have HexAutoStorage installed. Reopen the editor to check saved tags.");
                }
                else
                {
                    return;
                }
            }

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

            if (!CanEditTags(container, Player.m_localPlayer))
            {
                Player.m_localPlayer.Message(MessageHud.MessageType.Center, "$msg_cantopen");
                return;
            }

            if (TextInput.instance == null)
            {
                StorageConfig.DebugLog("Unable to open Auto Storage tag editor because TextInput.instance is null.");
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

            StorageConfig.DebugLog($"Opened Auto Storage tag editor for {prefabName}.");
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
                
                StorageConfig.DebugLog($"Invalid Auto Storage tag '{invalidTag}' rejected.");

                _editingContainer = null;
                return;
            }

            Container container = _editingContainer;
            _editingContainer = null;
            if (text == null || text.Length > MaxTagLength || !CanEditTags(container, Player.m_localPlayer))
            {
                Player.m_localPlayer?.Message(MessageHud.MessageType.Center, "Unable to edit Auto Storage tags.");
                return;
            }

            _pendingContainer = container;
            _pendingOwner = nview.GetZDO().GetOwner();
            _pendingRequest = ++_requestSequence;
            _pendingSince = Time.realtimeSinceStartup;
            StorageConfig.DebugLog($"Tag save request {_pendingRequest} for {container.gameObject.name}: " +
                $"requester={ZDOMan.GetSessionID()}, owner={_pendingOwner}, localOwner={nview.IsOwner()}.");
            if (nview.IsOwner())
            {
                HandleTagRequest(container, ZDOMan.GetSessionID(), _pendingRequest, text);
            }
            else
            {
                nview.InvokeRPC(SetTagsRpc, _pendingRequest, text);
            }
        }

        internal static void RegisterTagRpcs(Container container)
        {
            if (!TryGetContainerZNetView(container, out ZNetView nview))
            {
                return;
            }

            Piece piece = container.GetComponentInParent<Piece>();
            if (!StoragePrefabRules.TryGetStorageType(piece.gameObject.name.Replace("(Clone)", ""), out StorageType _))
            {
                return;
            }

            nview.Register<long, string>(SetTagsRpc, (sender, request, text) => HandleTagRequest(container, sender, request, text));
            nview.Register<long, string>(TagsResultRpc, (sender, request, message) => HandleTagResult(container, sender, request, message));
        }

        private static void HandleTagRequest(Container container, long sender, long request, string text)
        {
            if (Plugin.Instance == null || !StorageConfig.ModEnabled.Value ||
                !TryGetContainerZNetView(container, out ZNetView nview) || !nview.IsOwner())
            {
                return;
            }

            // Derive the player from the sending client's owned player object, not a supplied ID.
            Player player = sender == ZDOMan.GetSessionID() ? Player.m_localPlayer :
                Player.GetAllPlayers().FirstOrDefault(candidate =>
                {
                    ZNetView playerView = candidate.GetComponent<ZNetView>();
                    return playerView != null && playerView.IsValid() && playerView.GetZDO().GetOwner() == sender;
                });

            string message;
            StorageConfig.DebugLog($"Owner received tag save request {request} from {sender} for {container.gameObject.name}.");
            if (text == null || text.Length > MaxTagLength || !CanEditTags(container, player))
            {
                message = "Auto Storage tags were not saved: access denied or invalid request.";
            }
            else if (!TryNormalizeTags(text, out string tags, out string invalidTag))
            {
                message = $"Invalid Auto Storage tag: {invalidTag}";
            }
            else
            {
                nview.GetZDO().Set(TagsKey, tags);
                message = "Auto Storage tags saved.";
                StorageConfig.DebugLog($"Owner saved Auto Storage tags '{tags}' to {container.gameObject.name}.");
            }

            if (sender == ZDOMan.GetSessionID())
            {
                HandleTagResult(container, sender, request, message);
            }
            else
            {
                nview.InvokeRPC(sender, TagsResultRpc, request, message);
            }
        }

        private static void HandleTagResult(Container container, long sender, long request, string message)
        {
            if (Plugin.Instance == null || _pendingContainer != container || sender != _pendingOwner || request != _pendingRequest)
            {
                return;
            }

            _pendingContainer = null;
            StorageConfig.DebugLog($"Tag save response {request} from owner {sender}: {message}");
            Player.m_localPlayer?.Message(MessageHud.MessageType.Center, message);
        }

        private static bool CanEditTags(Container container, Player player)
        {
            if (container == null || player == null ||
                Vector3.Distance(player.transform.position, container.transform.position) > player.m_maxInteractDistance + 2f ||
                !(bool)CheckContainerAccess.Invoke(container, new object[] { player.GetPlayerID() }))
            {
                return false;
            }

            if (!container.m_checkGuardStone)
            {
                return true;
            }

            // Match vanilla's default overlapping-ward rule for the requesting player,
            // rather than CheckAccess, which checks only the owner's local player.
            bool insideWard = false;
            foreach (PrivateArea ward in (List<PrivateArea>)AllWards.GetValue(null))
            {
                if (ward == null || !(bool)WardEnabled.Invoke(ward, null) ||
                    !(bool)WardContains.Invoke(ward, new object[] { container.transform.position, 0f }))
                {
                    continue;
                }

                insideWard = true;
                Piece wardPiece = ward.GetComponent<Piece>();
                if ((wardPiece != null && wardPiece.GetCreator() == player.GetPlayerID()) ||
                    (bool)WardPermitted.Invoke(ward, new object[] { player.GetPlayerID() }))
                {
                    return true;
                }
            }

            return !insideWard;
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

            nview = container.m_rootObjectOverride != null
                ? container.m_rootObjectOverride.GetComponent<ZNetView>()
                : container.GetComponent<ZNetView>();

            return nview != null && nview.IsValid();
        }
    }

    [HarmonyPatch(typeof(Container), "Awake")]
    internal static class ContainerTagRpcRegistrationPatch
    {
        [HarmonyPostfix]
        internal static void Postfix(Container __instance)
        {
            ContainerTagEditor.RegisterTagRpcs(__instance);
        }
    }
}
