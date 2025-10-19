using Items;

// L (Create another static class) to also have list of enemies
namespace Services;
public static class ItemStorageService {                                        // Manager/Library of items
    public static List<Item> AllItems = new List<Item>() {
        // Armors
        new Shield("Wooden shield", 3),
        new Shield("Iron shield", 5),
        // Consumables
        new Potion("Small heal potion", 25),
        new Potion("Middle heal potion", 50),
        new Potion("Big heal potion", 75),
        new Potion("Giant heal potion", 100),
        // Jewels
        new Amulet("Amulet of luck", 5),
        // Weapons
        new Axe("Axe", 6),
        new Spear("Spear", 5),
        new Sword("Sword", 7),
    };

    public static Item? GetItemByName(string name)
        => AllItems.FirstOrDefault(i => i.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

    public static List<Item>? GetItemsByType(ItemType type)
        => AllItems.Where(i => i.Type == type).ToList();

    public static List<Item>? GetItemsByType(EquipementType equipType)
        => AllItems.Where(i => i.SlotType != null && i.SlotType == equipType).ToList();

    public static List<Item>? GetItemsByType<T>() where T : Item
        => AllItems.Where(i => i is T).ToList();

    public static void AddItem(Item item) {
        if (AllItems.Any(i => i.Name.Equals(item.Name, StringComparison.OrdinalIgnoreCase))) return;
        AllItems.Add(item);                                                     // Add if doesn't exists
    }

    public static void RemoveItem(string name) {
        var item = GetItemByName(name);
        if (item != null) AllItems.Remove(item);                                // Remove if exists
    }
}
