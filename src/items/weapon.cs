namespace Items;
public abstract class Weapon : Item {
    public int Attack { get; set; }

    public Weapon(string name="Weapon", int attack=1) : base(name) {
        Attack = attack;
        Description = $"A sharp piece of metal. Can be use to kill things.\nIncreases your attack by {Attack}.";
        Type = ItemType.Weapon;
        SlotType = EquipementType.Weapon;
    }

    public override void Present() {
        base.Present();
        Console.WriteLine($"Attack : {Attack}");
    }
}
