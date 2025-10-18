namespace Items;
public class Shield : Armor {
    public Shield(string name="Shield", int defense=1) : base(name, defense) {
        Description = $"A piece of wood or metal to protect you.\nIncreases your defense by {Defense}.";
        SlotType = EquipementType.Shield;
    }
}
