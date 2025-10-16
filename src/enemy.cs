namespace Gameplay;
public class Enemy : Entity {
    public int rewardGold { get; set; } = 5;
    public int rewardXp { get; set; } = 20;

    public Enemy() {
        Name = "Goblin";
        Level = 1;
        Hp = 30;
        Attack = 8;
        Defense = 2;
    }

    public static Enemy GenerateByLevel(int playerLevel) {
        var rnd = new Random();
        int lvl = Math.Max(1, playerLevel + rnd.Next(-1, 2));                   // A gap away from player's level
        return new Enemy { Name = lvl <= 1 ? "Goblin" : (lvl == 2 ? "Bandit" : "Orc"),
            Level = lvl, Hp = 20 + lvl * 15, MaxHp = 20 + lvl * 15,
            Attack = 6 + lvl * 3, Defense = 1 + lvl,
            rewardGold = lvl * 5, rewardXp = lvl * 20 };
    }
}
