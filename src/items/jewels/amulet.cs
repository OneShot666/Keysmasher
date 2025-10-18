namespace Items;
public class Amulet : Jewel {
    public Amulet(string name="Amulet of luck", int luck=1) : base(name, luck) {
        Description = $"An amulet that have the smell of clover.\nIncreases your luck by {Luck}.";
        SlotType = EquipementType.Amulet;
    }
}
