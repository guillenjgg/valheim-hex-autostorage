using System;
using System.Collections.Generic;

namespace HexAutoStorage.Core
{
    internal enum StorageType
    {
        Chest,
        Barrel,
        Cart
    }

    internal static class StoragePrefabRules
    {
        private static readonly HashSet<string> AllowedPrefabs = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "smelter",
            "charcoal_kiln",
            "blastfurnace",
            "windmill",
            "eitrrefinery",
            "piece_spinningwheel"
        };

        internal static readonly Dictionary<string, StorageType> SupportedPrefabs =
            new Dictionary<string, StorageType>(StringComparer.OrdinalIgnoreCase)
            {
                { "piece_chest_wood", StorageType.Chest },
                { "piece_chest_barrel", StorageType.Barrel },
                { "piece_chest", StorageType.Chest },
                { "piece_chest_blackmetal", StorageType.Chest },
                { "piece_chest_private", StorageType.Chest },
                { "Cart", StorageType.Cart }
            };

        internal static bool TryGetStorageType(string prefabName, out StorageType storageType)
        {
            return SupportedPrefabs.TryGetValue(prefabName, out storageType);
        }

        internal static bool IsSupportedSmelter(string prefabName)
        {
            return AllowedPrefabs.Contains(prefabName);
        }
    }
}