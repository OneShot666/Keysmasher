namespace Assets;
public abstract class Armor : Item {
    public int Defense { get; set; }

    public Armor(string name="Armor", int defense=1) : base(name) {
        Description = "A piece of leather pr metal you can put on your body";
        Defense = defense;
        Type = ItemType.Armor;
    }

    public override void Present() {
        base.Present();
        Console.WriteLine($"Defense : {Defense}");
    }
}
