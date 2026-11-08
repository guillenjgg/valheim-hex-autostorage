using HexAutoStorage.Configuration;
using HexAutoStorage.Core;
using HexAutoStorage.Features;
using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace HexAutoStorage
{
    internal static class AutoStorageService
    {
        private static readonly MethodInfo LoadContainer = AccessTools.Method(typeof(Container), "Load");
        private static readonly ConditionalWeakTable<Smelter, FallbackLogState> FallbackLogs =
            new ConditionalWeakTable<Smelter, FallbackLogState>();

        private sealed class FallbackLogState
        {
            internal float NextLogTime;
        }

        internal static bool TryStore(Smelter smelter, ItemDrop producedItem, int stack)
        {
            if (smelter == null || producedItem == null || stack <= 0)
            {
                return false;
            }

            ZNetView stationView = smelter.GetComponentInParent<ZNetView>();
            if (stationView == null || !stationView.IsValid())
            {
                return false;
            }

            List<Container> containers = FindNearbyContainers(
                smelter.transform.position,
                StorageConfig.StorageRadius.Value);

            if (containers.Count == 0)
            {
                LogStorageFallback(smelter, producedItem, stack, containers);
                return false;
            }

            string prefabName = producedItem.gameObject.name;

            // Priority 1: Matching tagged containers
            List<Container> taggedContainers = containers
                .Where(container => ContainerTagEditor.HasTag(container, prefabName))
                .OrderBy(container => Vector3.SqrMagnitude(container.transform.position - smelter.transform.position))
                .ToList();

            foreach (Container container in taggedContainers)
            {
                if (TryAddToContainer(container, smelter, producedItem, stack))
                {
                    StorageConfig.DebugLog($"Stored {stack}x {prefabName} in tagged container {container.gameObject.name}.");
                    return true;
                }
            }

            // Priority 2: Untagged containers that already contain the item
            List<Container> existingItemContainers = containers
                .Where(container =>
                    !HasTags(container) &&
                    ContainsProducedItem(container, prefabName))
                .OrderBy(container => Vector3.SqrMagnitude(container.transform.position - smelter.transform.position))
                .ToList();

            foreach (Container container in existingItemContainers)
            {
                if (TryAddToContainer(container, smelter, producedItem, stack))
                {
                    StorageConfig.DebugLog($"Stored {stack}x {prefabName} in untagged existing-item container {container.gameObject.name}.");
                    return true;
                }
            }

            LogStorageFallback(smelter, producedItem, stack, containers);
            return false;
        }

        private static void LogStorageFallback(Smelter smelter, ItemDrop producedItem, int stack, List<Container> containers)
        {
            if (StorageConfig.EnableDebugLogging?.Value != true)
            {
                return;
            }

            FallbackLogState state = FallbackLogs.GetValue(smelter, key => new FallbackLogState());
            if (Time.realtimeSinceStartup < state.NextLogTime)
            {
                return;
            }

            state.NextLogTime = Time.realtimeSinceStartup + 10f;
            string prefabName = producedItem.gameObject.name;
            var reasons = new List<string>();
            ItemDrop.ItemData itemData = CreateProducedItemData(smelter, producedItem, stack);

            foreach (Container container in containers.OrderBy(candidate =>
                Vector3.SqrMagnitude(candidate.transform.position - smelter.transform.position)))
            {
                string tags = ContainerTagEditor.GetTags(container);
                string reason;
                if (!string.IsNullOrWhiteSpace(tags) && !ContainerTagEditor.HasTag(container, prefabName))
                {
                    reason = "tag does not match";
                }
                else if (string.IsNullOrWhiteSpace(tags) && !ContainsProducedItem(container, prefabName))
                {
                    reason = "untagged and item absent";
                }
                else if (!container.IsOwner())
                {
                    reason = "remote network owner";
                }
                else if (!container.GetInventory().CanAddItem(itemData, stack))
                {
                    reason = "insufficient capacity for entire output stack";
                }
                else
                {
                    reason = "inventory rejected deposit despite capacity check";
                }

                reasons.Add($"{container.gameObject.name} at {container.transform.position} tags='{tags}': {reason}");
            }

            string details = reasons.Count == 0
                ? "No supported public containers were loaded within collider range."
                : string.Join("; ", reasons);
            StorageConfig.DebugLog($"Normal ground drop: {stack}x {prefabName} from {smelter.gameObject.name} " +
                $"at {smelter.transform.position}, radius {StorageConfig.StorageRadius.Value:F2}m. {details} " +
                "(Drop diagnostics limited to once per station every 10 seconds.)");
        }

        private static List<Container> FindNearbyContainers(Vector3 position, float radius)
        {
            Collider[] colliders = Physics.OverlapSphere(position, radius);
            var containers = new HashSet<Container>();

            foreach (Collider collider in colliders)
            {
                Piece piece = collider.GetComponentInParent<Piece>();

                if (piece == null)
                {
                    continue;
                }

                string prefabName = piece.gameObject.name.Replace("(Clone)", "");

                if (!StoragePrefabRules.TryGetStorageType(prefabName, out StorageType storageType))
                {
                    continue;
                }

                Container container = piece.GetComponentInChildren<Container>(true);

                if (!IsPublicStorage(container))
                {
                    continue;
                }

                if (containers.Add(container))
                {
                    ZNetView nview = piece.GetComponent<ZNetView>();
                    if (nview == null || !nview.IsValid() || container.GetInventory() == null)
                    {
                        containers.Remove(container);
                        continue;
                    }

                    // Awake creates an empty inventory; the scheduled CheckForChanges loads it
                    // later. Refresh first so routing and capacity use the saved contents.
                    // Load itself guards unchanged revisions and inventories currently in use.
                    LoadContainer.Invoke(container, null);
                    float distance = Vector3.Distance(position, piece.transform.position);
                    StorageConfig.DebugLog($"Found supported storage {prefabName} ({storageType}) at {distance:F2}m. Scan radius: {radius:F2}m.");
                }
            }

            return containers.ToList();
        }

        internal static bool IsPublicStorage(Container container)
        {
            return container != null && container.m_privacy == Container.PrivacySetting.Public;
        }

        private static bool HasTags(Container container)
        {
            return !string.IsNullOrWhiteSpace(ContainerTagEditor.GetTags(container));
        }

        private static bool ContainsProducedItem(Container container, string prefabName)
        {
            if (container == null)
            {
                return false;
            }

            Inventory inventory = container.GetInventory();

            if (inventory == null)
            {
                return false;
            }

            return inventory.GetItem(prefabName, -1, true) != null;
        }

        internal static ItemDrop.ItemData CreateProducedItemData(Smelter smelter, ItemDrop producedItem, int stack)
        {
            ItemDrop.ItemData itemData = producedItem.m_itemData.Clone();
            itemData.m_stack = stack;
            // Preserve the same output metadata as vanilla Smelter.Spawn.
            ZDO zdo = smelter.GetComponentInParent<ZNetView>().GetZDO();
            itemData.m_worldLevel = (byte)Game.m_worldLevel;
            itemData.m_cheated = (zdo.GetBool(ZDOVars.s_cheatedQueued, false) ||
                zdo.GetInt(ZDOVars.s_cheated, 0) == 2) && !PlayerProfile.s_bypassCheatChecks;
            return itemData;
        }

        private static bool TryAddToContainer(Container container, Smelter smelter, ItemDrop producedItem, int stack)
        {
            if (!IsPublicStorage(container) || producedItem == null)
            {
                return false;
            }

            // OnContainerChanged only saves on the container's network owner.
            if (!container.IsOwner())
            {
                StorageConfig.DebugLog($"Skipped remotely owned storage {container.gameObject.name}; no items were deposited.");
                return false;
            }

            Inventory inventory = container.GetInventory();

            if (inventory == null)
            {
                return false;
            }

            ItemDrop.ItemData itemData = CreateProducedItemData(smelter, producedItem, stack);

            if (!inventory.CanAddItem(itemData, stack))
            {
                return false;
            }

            return inventory.AddItem(itemData);
        }
    }
}
