using Core;

namespace Assets;
public abstract class Consumable : Item {
    public Consumable(string name="Consumable") : base(name) {
        Description = "Something you can only use once. Make it count.";
        Type = ItemType.Consumable;
    }

    public abstract void Use(Player player);
}
