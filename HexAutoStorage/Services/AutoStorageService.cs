using HexAutoStorage.Configuration;
using HexAutoStorage.Core;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HexAutoStorage
{
    internal static class AutoStorageService
    {
        private const long InvalidCreator = 0L;

        internal static bool TryStore(Smelter smelter, ItemDrop producedItem, int stack)
        {
            if (smelter == null || producedItem == null || stack <= 0)
            {
                return false;
            }

            Piece smelterPiece = smelter.GetComponent<Piece>();

            if (smelterPiece == null)
            {
                return false;
            }

            long smelterCreator = smelterPiece.GetCreator();

            if (smelterCreator == InvalidCreator)
            {
                return false;
            }

            List<Container> containers = FindNearbyContainers(
                smelter.transform.position,
                StorageConfig.StorageRadius.Value,
                smelterCreator);

            if (containers.Count == 0)
            {
                return false;
            }

            string prefabName = producedItem.gameObject.name;

            List<Container> existingItemContainers = containers
                .Where(container => ContainsProducedItem(container, prefabName))
                .OrderBy(container => Vector3.SqrMagnitude(container.transform.position - smelter.transform.position))
                .ToList();

            foreach (Container container in existingItemContainers)
            {
                if (TryAddToContainer(container, producedItem, stack))
                {
                    Plugin.Log.LogInfo($"Stored {stack}x {prefabName} in {container.gameObject.name}.");
                    return true;
                }
            }

            return false;
        }

        private static List<Container> FindNearbyContainers(Vector3 position, float radius, long creator)
        {
            Collider[] colliders = Physics.OverlapSphere(position, radius);
            var containers = new HashSet<Container>();

            foreach (Collider collider in colliders)
            {
                Piece piece = collider.GetComponentInParent<Piece>();

                if (piece == null || piece.GetCreator() != creator)
                {
                    continue;
                }

                string prefabName = piece.gameObject.name.Replace("(Clone)", "");

                if (!StoragePrefabRules.TryGetStorageType(prefabName, out StorageType storageType))
                {
                    continue;
                }

                Container container = piece.GetComponentInChildren<Container>(true);

                if (container == null)
                {
                    continue;
                }

                if (containers.Add(container))
                {
                    float distance = Vector3.Distance(position, piece.transform.position);

                    Plugin.Log.LogInfo($"Found supported storage {prefabName} ({storageType}) at {distance:F2}m. Scan radius: {radius:F2}m.");
                }
            }

            return containers.ToList();
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

        private static bool TryAddToContainer(Container container, ItemDrop producedItem, int stack)
        {
            if (container == null || producedItem == null)
            {
                return false;
            }

            Inventory inventory = container.GetInventory();

            if (inventory == null)
            {
                return false;
            }

            ItemDrop.ItemData itemData = producedItem.m_itemData.Clone();
            itemData.m_stack = stack;

            if (!inventory.CanAddItem(itemData, stack))
            {
                return false;
            }

            return inventory.AddItem(itemData);
        }
    }
}