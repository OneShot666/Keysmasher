public class Enemy : Entity{
    public int rewardGold { get; set; } = 5;
    public int rewardXp { get; set; } = 20;

    public Enemy() {
        name = "Goblin";
        level = 1;
        hp = 30;
        atk = 8;
        def = 2;
    }

    public static Enemy GenerateByLevel(int playerLevel) {
        var rnd = new Random();
        int lvl = Math.Max(1, playerLevel + rnd.Next(-1, 2));                   // A gap away from player's level
        return new Enemy { name = lvl <= 1 ? "Goblin" : (lvl == 2 ? "Bandit" : "Orc"),
            level = lvl, hp = 20 + lvl * 15, atk = 6 + lvl * 3, def = 1 + lvl,
            rewardGold = lvl * 5, rewardXp = lvl * 20 };
    }
}
