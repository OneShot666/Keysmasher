namespace Items;
public abstract class Jewel : Item {
    public int Luck { get; set; }

    public Jewel(string name="Jewel of luck", int luck=1) : base(name) {
        Luck = luck;
        Description = $"A shiny jewel you can wear.\nIncreases your luck by {Luck}.";
        Type = ItemType.Jewel;
    }

    public override void Present() {
        base.Present();
        Console.WriteLine($"Luck : {Luck}");
    }
}
