using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace Assets;
public abstract class Item() {
    [BsonId]                                                                    // Mongo primary key type
    public ObjectId id { get; set; } = ObjectId.GenerateNewId();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = "No description available.";
    public int Price { get; set; } = 0;                                         // L For merchant
    public ItemType Type { get; set; }
    public EquipementType? SlotType { get; set; }

    public override string ToString() {
        return $"Item(Name='{Name}', Description='{Description}')";
    }

    public Item(string name="Unknown Item") : this() {
        Name = name;
    }

    public virtual void Present() {
        bool condition = SlotType != null && Type.ToString() != SlotType.ToString();
        Console.WriteLine($"{Name, 10} [{Type}" + (condition ? $"|{SlotType}]" : "]"));
        Console.WriteLine($"{Description}");
    }
}
