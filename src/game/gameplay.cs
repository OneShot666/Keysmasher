using MongoDB.Bson;
using Core;

namespace Gameplay;
public class Gameplay {                                                         // Manage player
    public bool running = false;
    public Save? save;
    public Player? player;
    public string player_name = "";
    public Enemy? enemy;

    public void StartGame(Game program, Gameplay gameplay) {
        if (player == null) return;

        bool running = true;
        while (running) {
            Console.WriteLine("\n===== ACTIONS =====");
            Console.WriteLine("1 - Start a fight");
            Console.WriteLine("2 - Heal" + (player.hasPotion ? "" : " (potion required)"));
            Console.WriteLine("3 - Open inventory");
            Console.WriteLine("4 - Show profile");
            Console.WriteLine("5 - Save");
            Console.WriteLine("6 - Back to Menu");
            int choice = AskIntChoice();

            switch (choice) {
                case 1: StartCombat(gameplay.save); break;
                case 2: player.UsePotion(); break;
                case 3: player.ShowInventory(); break;
                case 4: player.Present(); break;
                case 5: program.Save(); break;
                case 6: running = false; break;
                default:
                    Game.WriteColoredMessage("Incorrect choice !");
                    break;
            }
            Console.ReadKey();
        }
    }

    public static int AskIntChoice(string question = "\nChoice : ") {           // Ask until user choose an integer
        int choice;
        string? input;
        do {
            Console.Write(question);
            input = Console.ReadLine();
        } while (!int.TryParse(input, out choice));                             // Check integer entered
        return choice;
    }

    public void StartCombat(Save? save) {
        if (player == null) return;
        enemy = Enemy.GenerateByLevel(player.Level);
        if (save != null) save.EnemyId = enemy.id;                              // Add enemy id in save
        Console.WriteLine($"\nA '{enemy.Name}' appears ! (HP {enemy.Hp})");

        var rnd = new Random();
        while (enemy.Hp > 0 && player.Hp > 0) {
            Thread.Sleep(1000);                                                 // Wait a second
            Console.WriteLine($"\n{player.Name} HP: {player.Hp}/{player.MaxHp}" +
                $" | {enemy.Name} HP: {enemy.Hp}/{enemy.MaxHp}");
            Console.WriteLine("1 Attack  2 Defend  3 Heal  4 Flee");
            int action = AskIntChoice("Action : ");
            Console.WriteLine();

            switch (action) {
                case 1:
                    int dmg = player.GetRandomDamage(4);
                    Console.WriteLine($"You attack and inflict {dmg} damage.");
                    enemy.TakeDamage(dmg);
                    break;
                case 2:
                    player.UseDefensivePosition();
                    break;
                case 3:
                    player.UsePotion();
                    break;
                case 4:
                    if (rnd.NextDouble() < 0.5) {                               // fifty-fifty chance to flee
                        Console.WriteLine("[Success] You escaped the combat !");
                        if (save != null) save.EnemyId = ObjectId.Empty;        // Remove enemy id if fled
                        return;
                    } else Console.WriteLine("[Fail] The escape failed!");
                    break;
                default:
                    Console.WriteLine("Action not recognized.");
                    continue;
            }

            if (enemy.Hp <= 0) {                                                // Check enemy still alive
                Console.WriteLine($"You defeated '{enemy.Name}' !");
                if (save != null) save.EnemyId = ObjectId.Empty;                // Remove enemy id if defeated
                SpawnLoot();
                player.LootEnemy(enemy);
                return;
            }

            // Enemy turn
            int enemyDmg = enemy.GetRandomDamage(3);
            Console.WriteLine($"{enemy.Name} attack and inflict {enemyDmg} damage.");
            player.TakeDamage(enemyDmg);
            if (player.Hp <= 0 && save != null) save.EnemyId = ObjectId.Empty;  // Remove enemy id if player died
        }
    }

    public void SpawnLoot()
    {
        if (player == null) return;
        var rnd = new Random();

        // ---- Drop potion ----
        double potionBaseDrop = 0.4;          // 40 % chance to drop a potion
        double potionAmuletBonus = 0.2;       // +20 % if amulet (→ 60 %)
        double potionChance = potionBaseDrop;

        if (player.Inventory.Contains("Amulet"))
            potionChance += potionAmuletBonus;

        if (rnd.NextDouble() < potionChance)
        {
            Console.WriteLine("You find a 'Potion' on the enemy !");
            player.CollectItem("Potion");
            if (player.Inventory.Contains("Amulet"))
                Console.WriteLine("Your amulet helped you find this potion!");
        }

        // ---- Drop amulet ----
        // Amulet is rarest : 20 % chance to drop, and only one time
        if (!player.Inventory.Contains("Amulet") && rnd.NextDouble() < 0.2)
        {
            Console.WriteLine("You found a mysterious 'Amulet' !");
            player.CollectItem("Amulet");
        }
    }

}
