
namespace AF.Inventory
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEditor;
    using UnityEngine;

    [CreateAssetMenu(fileName = "Inventory Database", menuName = "System/New Inventory Database", order = 0)]
    public class InventoryDatabase : ScriptableObject
    {

        [Header("Inventory")]
        public List<ItemInstance> ownedItems = new();

        public List<Item> defaultItems = new();

        [Header("Databases")]
        public EquipmentDatabase equipmentDatabase;


#if UNITY_EDITOR
        private void OnEnable()
        {
            // No need to populate the list; it's serialized directly
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;

        }

        private void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingPlayMode)
            {
                // Clear the list when exiting play mode
                Clear();
            }
        }
#endif
        public void Clear() => ownedItems.Clear();

        public void SetDefaultItems()
        {
            ownedItems.Clear();

            foreach (var defaultItem in defaultItems)
            {
                ItemInstance addedItem = AddItem(defaultItem);

                if (addedItem is ArmorInstance armorInstance)
                {
                    equipmentDatabase.EquipArmor(armorInstance);
                }
                else if (addedItem is LegwearInstance legwearInstance)
                {
                    equipmentDatabase.EquipLegwear(legwearInstance);
                }
            }
        }

        public void ReplenishItems()
        {
            foreach (var item in ownedItems)
            {
                if (item is ConsumableInstance consumableInstance && consumableInstance.wasUsed)
                {
                    consumableInstance.wasUsed = false;
                }
            }
        }

        public void AddUserCreatedItem(UserCreatedItem userCreatedItem)
        {
            Consumable consumable = userCreatedItem.GenerateItem();
            ConsumableInstance consumableInstance = new ConsumableInstance(Guid.NewGuid().ToString(), consumable);

            consumableInstance.createdItemThumbnailName = userCreatedItem.itemThumbnailName;
            consumableInstance.isUserCreatedItem = true;

            ownedItems.Add(consumableInstance);
        }

        public ItemInstance AddItem(Item itemToAdd) => AddItem(itemToAdd, 1).FirstOrDefault();

        public List<ItemInstance> AddItem(Item itemToAdd, int quantity)
        {
            List<ItemInstance> itemsAdded = new();

            for (int i = 0; i < quantity; i++)
            {
                string id = Guid.NewGuid().ToString();


                var toAdd = itemToAdd switch
                {
                    Weapon => new WeaponInstance(id, itemToAdd),
                    Shield => new ShieldInstance(id, itemToAdd),
                    Arrow => new ArrowInstance(id, itemToAdd),
                    Spell => new SpellInstance(id, itemToAdd),
                    Helmet => new HelmetInstance(id, itemToAdd),
                    Gauntlet => new GauntletInstance(id, itemToAdd),
                    Legwear => new LegwearInstance(id, itemToAdd),
                    Armor => new ArmorInstance(id, itemToAdd),
                    Accessory => new AccessoryInstance(id, itemToAdd),
                    Consumable => new ConsumableInstance(id, itemToAdd),
                    CraftingMaterial => new CraftingMaterialInstance(id, itemToAdd),
                    Gemstone => new GemstoneInstance(id, itemToAdd),
                    _ => new ItemInstance(id, itemToAdd),
                };

                ownedItems.Add(toAdd);
                itemsAdded.Add(toAdd);
            }

            return itemsAdded;
        }


        public ItemInstance GetFirst(Item itemToFind)
        {
            return ownedItems.FirstOrDefault(ownedItem => ownedItem.HasItem(itemToFind));
        }

        bool FindItemIndex(Item item, out int index)
        {
            index = ownedItems.FindIndex(ownedItem => ownedItem.HasItem(item));
            return index != -1;
        }

        public void RemoveItem(Item itemToRemove)
        {
            ItemInstance itemInstanceToRemove = GetFirst(itemToRemove);
            RemoveItemInstance(itemInstanceToRemove);
        }

        public void RemoveItemInstance(ItemInstance itemInstance)
        {
            if (itemInstance == null)
            {
                return;
            }

            if ((itemInstance.GetItem()?.isRenewable ?? false) && itemInstance is ConsumableInstance consumableInstance)
            {
                consumableInstance.wasUsed = true;
                return;
            }

            UnequipItemToRemove(itemInstance);
            ownedItems.Remove(itemInstance);
        }

        void UnequipItemToRemove(ItemInstance item) => equipmentDatabase.UnequipItem(item);

        public int GetItemAmount(Item itemToFind) => ownedItems.Sum(ownedItem => ownedItem.HasItem(itemToFind) ? 1 : 0);

        public bool HasItem(Item itemToFind) => FindItemIndex(itemToFind, out int idx);
        public bool HasItemAmount(Item itemToFind, int amount) => ownedItems.Sum(ownedItem => ownedItem.HasItem(itemToFind) ? 1 : 0) >= amount;
        public int GetWeaponsCount() => ownedItems.Sum(ownedItem => ownedItem.GetItem() is Weapon ? 1 : 0);

        public int GetSpellsCount() => ownedItems.Sum(ownedItem => ownedItem.GetItem() is Spell ? 1 : 0);

        public ItemInstance FindItemById(string id) => ownedItems.FirstOrDefault(ownedItem => ownedItem.IsId(id));

        public List<T> FilterByType<T>() where T : ItemInstance
        {
            return ownedItems.OfType<T>().ToList();
        }

        public void UpgradeWeapon(string weaponId) => FindItemById(weaponId)?.IncreaseLevel();
    }
}