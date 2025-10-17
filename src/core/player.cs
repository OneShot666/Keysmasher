using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using Assets;

// ! Create specific items
// ! Add slot system for equipping items (weapon, shield, amulet)
// ! Add weapon attack to basic attack of player (ex: 5 + 7)
// ! Add shield defense to basic defense of player (ex: 2 + 3)
// ! Add amulet to increase player's luck (chance of potion for now)
// ? Add energy/energyMax for player
namespace Core;
public class Player : Entity {
    [BsonRepresentation(BsonType.ObjectId)]
    public ObjectId UserId { get; set; }                                        // foreign key to user
    private bool is_defending = false;
    public bool hasPotion = true;                                               // Player start with a potion (normally)
    private const int healAmount = 50;
    public int Score { get; set; } = 0;                                         // Based on xp
    public int Xp { get; set; } = 0;
    public int MaxXp { get; set; } = 100;                                       // To level up
    public int Gold { get; set; } = 0;
    public int Luck { get; set; } = 0;                                          // +X% chance (max: 100)
    public List<string> Inventory { get; set; } = new List<string>();
    public Dictionary<EquipementType, Item?> EquippedItems { get; set; } = new() {
        {EquipementType.Weapon, null}, {EquipementType.Shield, null},
        {EquipementType.Amulet, null} };
    public int inv_capacity { get; set; } = 6;                                  // Max items in inventory

    public override string ToString() {
        return $"Player(Name='{Name}', Level={Level}, HP={Hp}, Max HP={MaxHp}, " +
            $"Score={Score}), Attack={Attack}, Defense={Defense}, Gold={Gold})";
    }

    public Player() {                                                           // Use for local saves
        Inventory.Clear();
        CollectItem("Sword");
        CollectItem("Shield");
        CollectItem("Potion");
    }

    public Player(string name, ObjectId user_id) {
        Name = name;
        UserId = user_id;
        Inventory.Clear();
        CollectItem("Sword");                                                   // Basic equipement
        CollectItem("Shield");
        CollectItem("Potion");
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
        string content = (Inventory.Count > 0) ? string.Join(", ", Inventory) : "Vide";
        Console.WriteLine($"Inventory : ({Inventory.Count}/{inv_capacity}) {content}");
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
        inv_capacity += 1;
        Console.WriteLine($"Level up ! You're now level : {Level}. HP restored.");
    }

    public void CheckLevelValue() {                                             // Use as debug function (for player)
        if (Xp % MaxXp == 0) Level = (Xp / MaxXp) + 1;
    }

    public void UseDefensivePosition() {
        if (Inventory.Contains("Shield")) {
            is_defending = true;
            Console.WriteLine("You took a defensive posture : defense doubled !");
        } else Console.WriteLine("You don't have any shield !");
    }

    public override void TakeDamage(int brut_damage) {
        int real_damage = Math.Max(0, brut_damage - (is_defending ? Defense * 2 : Defense));

        Hp -= real_damage;                                                      // Get hurts
        Console.WriteLine($"You took only {real_damage} damage" +
            (is_defending ? " thanks to your defensive posture !" : "."));
        is_defending = false;                                                   // Stop defending

        if (Hp <= 0) Die();                                                     // If damage fatal
    }

    public void UsePotion() {
        if (Hp >= MaxHp) Console.WriteLine("You're already full life !");       // Don't consume if full life
        else if (hasPotion) {
            UseItem("Potion");
            Hp = Math.Min(MaxHp, Hp + healAmount);
            Console.WriteLine($"You use a potion and heal {healAmount} HP. HP: {Hp}/{MaxHp}");
        } else Console.WriteLine("No heal potion available.");
    }

    public void ShowInventory() {
        Console.WriteLine($"\n===== INVENTORY ({Inventory.Count}/{inv_capacity}) =====");
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

    public void CollectItem(string item) {
        if (Inventory.Count >= inv_capacity) Console.WriteLine("Inventory is full !");
        else {
            Inventory.Add(item);
            hasPotion = Inventory.Contains("Potion");
        }
    }

    public void UseItem(string item) {
        if (Inventory.Contains(item)) Inventory.Remove(item);
        hasPotion = Inventory.Contains("Potion");
    }

    public void Equip(Item item) {
        if (item.SlotType == null) {
            Console.WriteLine($"{item.Name} cannot be equipped.");
            return;
        }

        EquippedItems[item.SlotType.Value] = item;
        Console.WriteLine($"{item.Name} equipped on {item.SlotType.Value}.");
    }

    public void Unequip(EquipementType slot) {
        if (EquippedItems[slot] != null) {
            Console.WriteLine($"{EquippedItems[slot]!.Name} unequipped from {slot}.");
            EquippedItems[slot] = null;
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
