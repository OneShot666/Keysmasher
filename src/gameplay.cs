namespace Gameplay;
public class Gameplay {                                                         // Manage player
    public bool running = false;
    public Save? save;                                                          // Use, load and save it (in MainProgram)
    public Player? player;
    public string player_name = "";
    public Enemy? enemy;

    public void StartGame(MainProgram program) {
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
                case 1: StartCombat(); break;
                case 2: player.UsePotion(); break;
                case 3: player.ShowInventory(); break;
                case 4: player.Present(); break;
                case 5: program.Save(); break;
                case 6: running = false; break;
                default:
                    MainProgram.WriteColoredMessage("Incorrect choice !");
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

    public void StartCombat() {
        if (player == null) return;
        enemy = Enemy.GenerateByLevel(player.Level);
        Console.WriteLine($"\nA '{enemy.Name}' appears ! (HP {enemy.Hp})");

        var rnd = new Random();
        while (enemy.Hp > 0 && player.Hp > 0) {
            Thread.Sleep(1000);                                                 // Wait a second
            Console.WriteLine($"\n{player.Name} HP: {player.Hp}/{player.MaxHp}" +
                $" | {enemy.Name} HP: {enemy.Hp}/{enemy.MaxHp}");
            Console.WriteLine("1 Attack  2 Defend  3 Heal  4 Flee");
            int action = AskIntChoice("Action : ");

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
                        return;
                    } else Console.WriteLine("[Fail] The escape failed!");
                    break;
                default:
                    Console.WriteLine("Action not recognized.");
                    break;
            }

            if (enemy.Hp <= 0) {                                                // Check enemy still alive
                Console.WriteLine($"You defeated '{enemy.Name}' !");
                SpawnLoot();
                player.LootEnemy(enemy);
                return;
            }

            // Enemy turn
            int enemyDmg = enemy.GetRandomDamage(3);
            Console.WriteLine($"{enemy.Name} attack and inflict {enemyDmg} damage.");
            player.TakeDamage(enemyDmg);
        }
    }

    public void SpawnLoot() {
        if (player == null) return;
        var rnd = new Random();
        if (rnd.NextDouble() < 0.4) {                                           // 40% chance to drop a potion
            Console.WriteLine("You find a 'Potion' on the enemy !");
            player.Inventory.Add("Potion");
            player.hasPotion = true;
        }
    }
}
