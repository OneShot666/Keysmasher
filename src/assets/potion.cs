using Core;

// L Make it a type of consumable and add sub-types (health, energy, xp...)
namespace Assets;
public abstract class Potion : Consumable {
    public int HealAmount { get; set; }

    public Potion(string name="Healt potion", int healAmount=50) : base(name) {
        HealAmount = healAmount;
        Description = $"A potion that heal you by {HealAmount} hp.";
    }

    public override void Present() {
        base.Present();
        Console.WriteLine($"Heal amount : {HealAmount}");
    }

    public override void Use(Player player) {
        Console.WriteLine($"You drink '{Name}'. (+{HealAmount} hp)");
        player.Hp += HealAmount;
    }
}
