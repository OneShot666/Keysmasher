using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace Gameplay;
public class Player {
    [BsonId]                                                                    // Mongo primary key type
    public ObjectId id { get; set; } = ObjectId.GenerateNewId();
    public string name { get; set; } = "";
    public int level { get; set; } = 1;
    public int hp { get; set; } = 100;
    public int maxHp { get; set; } = 100;
    public int score { get; set; } = 0;                                         // Xp
    public int maxScore { get; set; } = 100;
    public int atk { get; set; } = 12;
    public int def { get; set; } = 5;
    public int gold { get; set; } = 0;
    public List<string> Inventory { get; set; } = new List<string>();
    private const int healAmount = 50;

    public string? PasswordHash { get; set; } = "";
    public string? Salt { get; set; } = "";

    public override string ToString() {                                         // When display instances
        return $"Player(Name='{name}', Level={level}, Score={score}), " +
            $"Attack={atk}, Defense={def}, Gold={gold})";
    }

    public Player() {
        Inventory.Add("Potion");                                                // Player start with a potion
    }

    public Player(string name, string password, string salt) {
        this.name = name;
        PasswordHash = password;
        Salt = salt;
        Inventory.Add("Potion");                                                // Player start with a potion
    }

    public void Present() {
        Console.WriteLine($"\nYour profile : ");
        Console.WriteLine($"Name : {name}");
        Console.WriteLine($"Level : {level} ({score}/{maxScore} xp)");
        Console.WriteLine($"Score : {score} pts");
        Console.WriteLine($"HP : {hp}/{maxHp}");
        Console.WriteLine($"Attack : {atk}");
        Console.WriteLine($"Defense : {def}");
        Console.WriteLine($"Coins : x{gold}");
        Console.WriteLine($"Inventory : {string.Join(", ", Inventory)}");
    }

    public void GainXp(int amount) {
        score += amount;
        Console.WriteLine($"Gagné {amount} XP !");
        while (score >= maxScore) LevelUp();
    }

    public void LevelUp() {
        level++;
        score -= maxScore;
        maxScore = (int)(maxScore * 1.2);
        maxHp += 10;
        hp = maxHp;                                                             // Regen player
        atk += 2;
        def += 1;
        Console.WriteLine($"Niveau supérieur ! Nouveau niveau: {level}. PV restaurés.");
    }

    public void CheckLevelValue() {                                             // Use as debug function
        if (score % maxScore == 0) level = (score / maxScore) + 1;
    }

    public void UsePotion() {
        if (Inventory.Contains("Potion")) {
            Inventory.Remove("Potion");
            hp = Math.Min(maxHp, hp + healAmount);
            Console.WriteLine("Pas de potion disponible.");
        } else Console.WriteLine($"You use a potion and heal {healAmount} HP. HP: {hp}/{maxHp}");
    }

    public void ShowInventory() {
        Console.WriteLine("\n-- Inventaire --");
        if (Inventory.Count == 0) {
            Console.WriteLine("Vide");
            return;
        }

        var counts = new Dictionary<string, int>();
        foreach (var it in Inventory) {
            if (!counts.ContainsKey(it)) counts[it] = 0;
            counts[it]++;
        }

        foreach (var kv in counts) {
            Console.WriteLine($"{kv.Key} x{kv.Value}");
        }
    }
}
