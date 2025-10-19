using Items;

namespace Services;
public class InventoryService {                                                 // Manager player's inventory
    private List<Item> _Items;                                                  // List of items in inventory
    public int Capacity { get; private set; }                                   // Max items in inventory

    public InventoryService(int capacity=5) {
        _Items = new List<Item>();
        Capacity = capacity;
    }

    public bool IsFull() => _Items.Count >= Capacity;

    public bool HasItem(string name)
        => _Items.Any(i => i.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

    public bool HasItem(ItemType type) => _Items.Any(i => i.Type == type);

    public bool HasItem<T>() where T : Item => _Items.Any(i => i is T);

    public List<Item> GetAllItems() => _Items;

    public int GetCount() => _Items.Count;

    public Item? GetItemByName(string name)
        => _Items.FirstOrDefault(i => i.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

    public List<Item>? GetItemsByType(ItemType type)
        => _Items.Where(i => i.Type == type).ToList();

    public List<Item>? GetItemsByType(EquipementType equipType)
        => _Items.Where(i => i.SlotType != null && i.SlotType == equipType).ToList();

    public List<Item>? GetItemsByType<T>() where T : Item => _Items.Where(i => i is T).ToList();

    public int CountItemsByType(ItemType type)
        => _Items.Count(i => i.Type == type);

    public bool AddItem(Item item) {
        if (_Items.Count >= Capacity) return false;                             // Inventory full
        _Items.Add(item);
        return true;
    }

    public bool RemoveItem(Item item) {
        return _Items.Remove(item);
    }

    public bool RemoveItemByName(string name) {
        var item = GetItemByName(name);
        if (item == null) return false;
        return _Items.Remove(item);
    }

    public void Clear() => _Items.Clear();

    public void AddCapacity(int amount=1) {
        if (amount <= 0) return;
        Capacity += amount;
    }
}
