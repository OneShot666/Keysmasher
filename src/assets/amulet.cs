namespace Assets;
public abstract class Amulet : Item {
    public int Luck { get; set; }

    public Amulet(string name="Amulet of luck", int luck=1) : base(name) {
        Luck = luck;
        Description = $"An amulet that have the smell of clover. \nIncreases your luck by {Luck}.";
        Type = ItemType.Amulet;
        SlotType = EquipementType.Amulet;
    }

    public override void Present() {
        base.Present();
        Console.WriteLine($"Luck : {Luck}");
    }
}
