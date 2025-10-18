namespace Items;
public class Axe : Weapon {
    public Axe(string name="Axe", int attack=1) : base(name, attack) {
        Description = $"A simple axe. Can also be use to cut wood.\nIncreases your attack by {Attack}.";
    }
}
