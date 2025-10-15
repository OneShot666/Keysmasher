using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

// L Player can only use defensive posture if has a shield
namespace Gameplay;
public class Player : Entity {
    [BsonRepresentation(BsonType.ObjectId)]
    public ObjectId UserId { get; set; }                                        // foreign key to user
    private bool is_defending = false;
    public int Score { get; set; } = 0;                                         // Based on xp
    public int Xp { get; set; } = 0;
    public int MaxXp { get; set; } = 100;                                       // To level up
    public int Gold { get; set; } = 0;
    public List<string> Inventory { get; set; } = new List<string>();
    private const int healAmount = 50;

    public override string ToString() {
        return $"Player(Name='{Name}', Level={Level}, HP={Hp}, Max HP={MaxHp}, " +
            $"Score={Score}), Attack={Attack}, Defense={Defense}, Gold={Gold})";
    }

    public Player(string name, ObjectId user_id) {
        Name = name;
        UserId = user_id;
        Attack = 12;                                                            // New default value
        Defense = 5;
        Inventory.Add("Shield");
        Inventory.Add("Potion");                                                // Player start with a potion
    }

    public override void Present() {
        Console.WriteLine($"\nYour profile : ");
        Console.WriteLine($"Name      : {Name}");
        Console.WriteLine($"Level     : {Level} ({Xp}/{MaxXp} xp)");
        Console.WriteLine($"Score     : {Score} pts");
        Console.WriteLine($"HP        : {Hp}/{MaxHp}");
        Console.WriteLine($"Attack    : {Attack}");
        Console.WriteLine($"Defense   : {Defense}");
        Console.WriteLine($"Coins     : x{Gold}");
        Console.WriteLine($"Inventory : {string.Join(", ", Inventory)}");
    }

    public void GainXp(int amount) {
        Score += amount;
        Xp += amount;
        Console.WriteLine($"Won {amount} XP !");
        while (Xp >= MaxXp) LevelUp();
    }

    public void LevelUp() {
        Level++;
        Xp -= MaxXp;
        MaxXp = (int)(MaxXp * 1.2);
        MaxHp += 10;
        Hp = MaxHp;                                                             // Regen player
        Attack += 2;
        Defense += 1;
        Console.WriteLine($"Level up ! You're now level : {Level}. HP restored.");
    }

    public void CheckLevelValue() {                                             // Use as debug function (for player)
        if (Xp % MaxXp == 0) Level = (Xp / MaxXp) + 1;
    }

    public void UseDefensivePosition() {
        is_defending = true;
        Console.WriteLine("You took a defensive posture : defense doubled !");
    }

    public override void TakeDamage(int brut_damage) {
        int real_damage = Math.Max(0, brut_damage - (is_defending ? Defense * 2 : Defense));

        Hp -= real_damage;                                                      // Get hurts
        Console.WriteLine($"You took only {real_damage} damage" +
            (is_defending ? "thanks to your defensive posture !" : "."));
        is_defending = false;                                                   // Stop defending

        if (Hp <= 0) Die();                                                     // If damage fatal
    }

    public void UsePotion() {
        if (Inventory.Contains("Potion")) {
            Inventory.Remove("Potion");
            Hp = Math.Min(MaxHp, Hp + healAmount);
            Console.WriteLine("No heal potion available.");
        } else Console.WriteLine($"You use a potion and heal {healAmount} HP. HP: {Hp}/{MaxHp}");
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
        Gold += enemy.rewardGold;
        Console.WriteLine($"You gained {enemy.rewardGold} coins. \nYour gold : {Gold}");
    }

    public void Die() {
        Console.WriteLine("You died... \nRespawn to the village with half your hp.");
        Hp = Math.Max(1, MaxHp / 2);                                            // Respawn half life
        Gold = Math.Max(0, Gold - 10);                                          // Lose 10 coins
    }
}
