namespace Items;
public class Spear : Weapon {
    public Spear(string name="Spear", int attack=1) : base(name, attack) {
        Description = $"A small spear. Can touch enemies from a longer distance.\nIncreases your attack by {Attack}.";
    }
}
