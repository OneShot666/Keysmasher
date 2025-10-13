public class Enemy {
    public string name { get; set; } = "Gobelin";
    public int level { get; set; } = 1;
    public int hp { get; set; } = 30;
    public int atk { get; set; } = 8;
    public int def { get; set; } = 2;

    public static Enemy GenerateByLevel(int playerLevel) {
        var rnd = new Random();
        int lvl = Math.Max(1, playerLevel + rnd.Next(-1, 2)); // playerLevel-1..playerLevel+1
        return new Enemy { name = lvl <= 1 ? "Gobelin" : (lvl == 2 ? "Bandit" : "Orc"),
            level = lvl, hp = 20 + lvl * 15, atk = 6 + lvl * 3, def = 1 + lvl };
    }
}
