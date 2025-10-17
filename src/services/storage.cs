using System.Text.Json;
using Assets;

// . Use [Security] to encrypt items json file
// ! Find a place to stock items objects then instance this class and save them
namespace Services;
public static class LocalItemStorage {
    private static readonly string item_folder = Path.Combine("Data", "Items");

    public static List<Item> LoadAllItems() {
        var items = new List<Item>();
        if (!Directory.Exists(item_folder)) return items;

        foreach (var file in Directory.GetFiles(item_folder, "*.json")) {
            try {
                string json = File.ReadAllText(file);
                var item = JsonSerializer.Deserialize<Item>(json);
                if (item != null) items.Add(item);
            } catch (Exception e) {
                Console.WriteLine($"Error while loading item : {e.Message}");
            }
        }
        return items;
    }

    public static void SaveItem(Item item) {                                    // ? Move in server.cs
        Directory.CreateDirectory(item_folder);                                 // Create directory if doesn't exists
        string path = Path.Combine(item_folder, $"{item.Name}.json");
        File.WriteAllText(path, JsonSerializer.Serialize(item));
    }
}
