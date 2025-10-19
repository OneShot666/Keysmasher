using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using Gameplay;
using Services;
using Items;

// ? Add energy/energyMax for player
namespace Core;
public class Player : Entity {
    [BsonRepresentation(BsonType.ObjectId)]
    public ObjectId UserId { get; set; }                                        // Foreign key to user
    private bool is_defending = false;
    public bool hasPotion => Inventory.HasItem<Potion>();                       // Check if player has potion
    public int Score { get; set; } = 0;                                         // Based on xp
    public int Xp { get; set; } = 0;
    public int MaxXp { get; set; } = 100;                                       // To level up
    public int Luck { get; set; } = 0;                                          // +X% chance (max: 100)
    public int Gold { get; set; } = 0;
    public InventoryService Inventory = new(5);
    public Dictionary<EquipementType, Item?> EquippedItems { get; set; } = new() {
        {EquipementType.Weapon, null}, {EquipementType.Shield, null},
        {EquipementType.Amulet, null} };

    public override string ToString() {
        return $"Player(Name='{Name}', Level={Level}, HP={Hp}, Max HP={MaxHp}, " +
            $"Score={Score}), Attack={Attack}, Defense={Defense}, Luck={Luck}, Gold={Gold})";
    }

    public Player() {                                                           // Use for local saves
        Inventory.Clear();
        CollectItem(ItemStorageService.GetItemsByType<Weapon>()![0]);           // Basic equipement
        CollectItem(ItemStorageService.GetItemsByType<Shield>()![0]);
        CollectItem(ItemStorageService.GetItemsByType<Potion>()![0]);
    }

    public Player(string name, ObjectId user_id) {
        Name = name;
        UserId = user_id;
        Inventory.Clear();
        CollectItem(ItemStorageService.GetItemsByType<Weapon>()![0]);           // Basic equipement
        CollectItem(ItemStorageService.GetItemsByType<Shield>()![0]);
        CollectItem(ItemStorageService.GetItemsByType<Potion>()![0]);
    }

    public override void Present() {
        Console.WriteLine($"\nYour profile : \n");
        Console.WriteLine($"Name      : {Name}");
        Console.WriteLine($"Level     : {Level} ({Xp}/{MaxXp} xp)");
        Console.WriteLine($"Score     : {Score} pts");
        Console.WriteLine($"HP        : {Hp}/{MaxHp}");
        Console.WriteLine($"Attack    : {GetTotalAttack()}");
        Console.WriteLine($"Defense   : {GetTotalDefense()}");
        Console.WriteLine($"Luck      : {GetTotalLuck()}");
        Console.WriteLine($"Coins     : x{Gold}\n");
        Console.WriteLine($"Inventory : ({Inventory.GetCount()}/{Inventory.Capacity})");
        if (Inventory.GetCount() <= 0) Console.WriteLine(" Vide");
        else foreach (Item item in Inventory.GetAllItems()) item.Present();
        Console.WriteLine("\nEquipped items : ");
        foreach (var kv in EquippedItems) {
            Console.Write($"{kv.Key} : ");
            if (kv.Value != null) Console.WriteLine($"{kv.Value!.Name}");
            else Console.WriteLine("None");
        }
    }

    public int GetTotalAttack() {
        int weapon_attack = 0;
        if (EquippedItems[EquipementType.Weapon] is Item weapon && weapon is Weapon wpn)
            weapon_attack = wpn.Attack;
        return Attack + weapon_attack;
    }

    public int GetTotalDefense() {
        int shield_defense = 0;
        if (EquippedItems[EquipementType.Shield] is Item shield && shield is Shield shd)
            shield_defense = shd.Defense;
        return Defense + shield_defense;
    }

    public int GetTotalLuck() {
        int amulet_luck = 0;
        if (EquippedItems[EquipementType.Amulet] is Item amulet && amulet is Amulet mlt)
            amulet_luck = mlt.Luck;
        return Luck + amulet_luck;
    }

    public float GetPercentLuck() {
        return GetTotalLuck() * 0.01f;
    }

    public void GainXp(int amount) {
        Score += amount;
        Xp += amount;
        Game.WriteColoredMessage($"You won {amount} XP !", Game.success);
        while (Xp >= MaxXp) LevelUp();
    }

    public void LevelUp() {
        Level++;
        Xp -= MaxXp;
        MaxXp = (int)(MaxXp * 1.2);                                             // Increase xp needed by 20%
        MaxHp += 10;
        Hp = MaxHp;                                                             // Regen player
        Attack += 2;
        Defense += 1;
        Inventory.AddCapacity();
        Game.WriteColoredMessage($"Level up ! You're now level : {Level}. HP restored.", Game.success);
    }

    public void CheckLevelValue() {                                             // Use as debug function (for player)
        if (Xp % MaxXp == 0) Level = (Xp / MaxXp) + 1;
    }

    public void UseDefensivePosition() {
        if (EquippedItems[EquipementType.Shield] != null) {
            is_defending = true;
            Console.WriteLine("You took a defensive posture : defense doubled !");
        } else Game.WriteColoredMessage("You don't have any shield equipped !", Game.warning);
    }

    public override void TakeDamage(int brut_damage) {
        int player_defense = is_defending ? GetTotalDefense() * 2 : GetTotalDefense();
        int real_damage = Math.Max(0, brut_damage - player_defense);

        Hp -= real_damage;                                                      // Get hurts
        Console.WriteLine($"You took only {real_damage} damage" +
            (is_defending ? " thanks to your defensive posture !" : "."));
        is_defending = false;                                                   // Stop defending

        if (Hp <= 0) Die();                                                     // If damage fatal
    }

    public void UsePotion() {
        if (Hp >= MaxHp) Game.WriteColoredMessage("You're already full life !", Game.success);  // Don't consume if full life
        else if (hasPotion) {
            Potion potion = (Potion)Inventory.GetItemsByType<Potion>()!.First();
            UseItem(potion);
            Hp = Math.Min(Hp, MaxHp);                                           // Cap hp to max hp
            Console.WriteLine($"HP: {Hp}/{MaxHp}");
        } else Game.WriteColoredMessage("No heal potion available.", Game.warning);
    }

    public void ShowInventory() {
        Console.WriteLine($"\n===== INVENTORY ({Inventory.GetCount()}/{Inventory.Capacity}) =====");
        if (Inventory.GetCount() == 0) {
            Game.WriteColoredMessage("You don't have any item yet");
            return;
        }

        var counts = new Dictionary<string, int>();
        foreach (var it in Inventory.GetAllItems().ToArray().Select(i => i.Name)) {
            if (!counts.ContainsKey(it)) counts[it] = 0;
            counts[it]++;
        }

        foreach (var kv in counts) Console.WriteLine($"{kv.Key} x{kv.Value}");
    }

    public void CollectItem(Item item) {                                        // Auto-equip if slot free
        if (!Inventory.AddItem(item)) Game.WriteColoredMessage("Inventory is full !", Game.warning);
        if (item.SlotType != null && EquippedItems[item.SlotType.Value] == null) Equip(item);
    }

    public void UseItem(Item item) {
        if (Inventory.HasItem(item.Name)) {
            if (item is Consumable consumable) {
                consumable.Use(this);
                Inventory.RemoveItem(item);                                     // Consume item
            } else Game.WriteColoredMessage($"{item.Name} is not a consumable item.", Game.warning);
        } else Game.WriteColoredMessage($"You don't have {item.Name} in your inventory.", Game.warning);
    }

    public void Equip(Item item) {
        if (item.SlotType == null) {
            Game.WriteColoredMessage($"{item.Name} cannot be equipped.", Game.warning);
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
        Console.WriteLine($"You gained {enemy.rewardGold} coins.\nYour gold : {Gold}");
    }

    public void Die() {
        Game.WriteColoredMessage("You died...\nRespawn to the village with half your hp.", Game.fail);
        Hp = Math.Max(1, MaxHp / 2);                                            // Respawn half life
        Gold = Math.Max(0, Gold - 10);                                          // Lose 10 coins
    }
}
