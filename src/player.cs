using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

// L Player can only use defensive posture if has a shield
namespace Gameplay;
public class Player : Entity {
    [BsonId]                                                                    // Mongo primary key type
    public ObjectId id { get; set; } = ObjectId.GenerateNewId();
    public int score { get; set; } = 0;                                         // Xp
    public int maxScore { get; set; } = 100;
    private bool is_defending = false;
    public int gold { get; set; } = 0;
    public List<string> Inventory { get; set; } = new List<string>();
    private const int healAmount = 50;

    public string? PasswordHash { get; set; } = "";
    public string? Salt { get; set; } = "";

    public override string ToString() {
        return $"Player(Name='{name}', Level={level}, HP={hp}, Max HP={maxHp}, " +
            $"Score={score}), Attack={atk}, Defense={def}, Gold={gold})";
    }

    public Player() {
        atk = 12;                                                               // New default value
        def = 5;
        Inventory.Add("Shield");
        Inventory.Add("Potion");                                                // Player start with a potion
    }

    public Player(string name, string password, string salt) {
        this.name = name;
        PasswordHash = password;
        Salt = salt;
        atk = 12;
        def = 5;
        Inventory.Add("Shield");
        Inventory.Add("Potion");                                                // Player start with a potion
    }

    public override void Present() {
        Console.WriteLine($"\nYour profile : ");
        Console.WriteLine($"Name      : {name}");
        Console.WriteLine($"Level     : {level} ({score}/{maxScore} xp)");
        Console.WriteLine($"Score     : {score} pts");
        Console.WriteLine($"HP        : {hp}/{maxHp}");
        Console.WriteLine($"Attack    : {atk}");
        Console.WriteLine($"Defense   : {def}");
        Console.WriteLine($"Coins     : x{gold}");
        Console.WriteLine($"Inventory : {string.Join(", ", Inventory)}");
    }

    public void GainXp(int amount) {
        score += amount;
        Console.WriteLine($"Won {amount} XP !");
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
        Console.WriteLine($"Level up ! You're now level : {level}. HP restored.");
    }

    public void CheckLevelValue() {                                             // Use as debug function (for player)
        if (score % maxScore == 0) level = (score / maxScore) + 1;
    }

    public void UseDefensivePosition() {
        is_defending = true;
        Console.WriteLine("You took a defensive posture : defense doubled !");
    }

    public override void TakeDamage(int brut_damage) {
        int real_damage = Math.Max(0, brut_damage - (is_defending ? def * 2 : def));

        hp -= real_damage;                                                      // Get hurts
        Console.WriteLine($"You took only {real_damage} damage" +
            (is_defending ? "thanks to your defensive posture !" : "."));
        is_defending = false;                                                   // Stop defending

        if (hp <= 0) Die();                                                     // If damage fatal
    }

    public void UsePotion() {
        if (Inventory.Contains("Potion")) {
            Inventory.Remove("Potion");
            hp = Math.Min(maxHp, hp + healAmount);
            Console.WriteLine("No heal potion available.");
        } else Console.WriteLine($"You use a potion and heal {healAmount} HP. HP: {hp}/{maxHp}");
    }

    public void ShowInventory() {
        Console.WriteLine("\n===== INVENTORY =====");
        if (Inventory.Count == 0) {
            Console.WriteLine("You don't have any item yet");
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

    public void LootEnemy(Enemy enemy) {
        GainXp(enemy.rewardXp);
        gold += enemy.rewardGold;
        Console.WriteLine($"You gained {enemy.rewardGold} coins. \nYour gold : {gold}");
    }

    public void Die() {
        Console.WriteLine("You died... \nRespawn to the village with half your hp.");
        hp = Math.Max(1, maxHp / 2);                                            // Respawn half life
        gold = Math.Max(0, gold - 10);                                          // Lose 10 coins
    }
}
