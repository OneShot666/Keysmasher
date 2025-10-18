namespace Items;
public class Sword : Weapon {
    public Sword(string name="Sword", int attack=1) : base(name, attack) {
        Description = $"A fine blade to hurt your enemies.\nIncreases your attack by {Attack}.";
    }
}
