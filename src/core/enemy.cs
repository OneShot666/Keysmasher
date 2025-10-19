using MongoDB.Bson;

// ! Add loot dict of {item, chance to drop[0-100]} -> player's luck is add to chance
// ! Add different enemy types with varying stats and rewards (herited from this abstract class)
namespace Core;
public class Enemy : Entity {
    public int rewardGold { get; set; } = 5;
    public int rewardXp { get; set; } = 20;

    public Enemy() {
        id = ObjectId.GenerateNewId();
        Name = "Goblin";
        Level = 1;
        Hp = 30;
        Attack = 4;
        Defense = 1;
    }

    public static Enemy GenerateByLevel(int playerLevel) {
        var rnd = new Random();
        int lvl = Math.Max(1, playerLevel + rnd.Next(-1, 2));                   // A gap away from player's level
        return new Enemy { Name = lvl <= 1 ? "Goblin" : (lvl == 2 ? "Bandit" : "Orc"),
            Level = lvl, Hp = 20 + (lvl - 1) * 15, MaxHp = 20 + (lvl - 1) * 15,
            Attack = 4 + (lvl - 1) * 2, Defense = 1 + (lvl - 1),
            rewardGold = lvl * 5, rewardXp = lvl * 20 };
    }
}
