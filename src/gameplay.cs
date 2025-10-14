namespace Gameplay;
public class Gameplay {                                                         // Manage player
    public bool running = false;
    public Player? player;
    public string player_name = "";
    private readonly int min_username = 3;
    private readonly int max_username = 15;
    private readonly int min_password = 6;                                      // No max lenght for password for now

    public void StartGame(MainProgram program) {
        if (player == null) return;

        bool running = true;
        while (running) {
            Console.WriteLine("\n===== ACTIONS =====");
            Console.WriteLine("1 - Start a fight");
            Console.WriteLine("2 - Heal");
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

    public void AskName() {
        do {
            Console.Write("\nUsername : ");
            player_name = Console.ReadLine()?.Trim() ?? "";
            if (player_name.Length < min_username || player_name.Length > max_username)
                MainProgram.WriteColoredMessage("Username must contain between 3 and 15 characters!",
                    ConsoleColor.Yellow);
        } while (player_name.Length < min_username || player_name.Length > max_username);
    }

    public string AskHiddenPassword(string prompt = "\nPassword : ") {
        string password = "";
        do {
            Console.Write(prompt);
            ConsoleKeyInfo key;

            while (true) {
                key = Console.ReadKey(intercept: true);

                if (key.Key == ConsoleKey.Enter) {                              // Confirm password
                    if (password.Length < min_password)
                        MainProgram.WriteColoredMessage("\nPassword is too short !", ConsoleColor.Yellow);
                    break;
                } else if (key.Key == ConsoleKey.Backspace) {                   // Remove last char
                    if (password.Length > 0) {
                        password = password[..^1];
                        Console.Write("\b \b");
                    }
                } else {                                                        // Display hidden char
                    password += key.KeyChar;
                    Console.Write("*");
                }
            }
        } while(password.Length < min_password);

        return password;
    }

    public void StartCombat() {
        if (player == null) return;
        var enemy = Enemy.GenerateByLevel(player.level);
        Console.WriteLine($"\nA '{enemy.name}' appears ! (HP {enemy.hp})");

        var rnd = new Random();
        while (enemy.hp > 0 && player.hp > 0) {
            Console.WriteLine($"\n{player.name} HP: {player.hp}/{player.maxHp} | {enemy.name} HP: {enemy.hp}");
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

            if (enemy.hp <= 0) {                                                // Check enemy still alive
                Console.WriteLine($"You defeated '{enemy.name}' !");
                SpawnLoot();
                player.LootEnemy(enemy);
                return;
            }

            // Enemy turn
            int enemyDmg = enemy.GetRandomDamage(3);
            Console.WriteLine($"{enemy.name} attack and inflict {enemyDmg} damage.");
            player.TakeDamage(enemyDmg);
        }
    }

    public void SpawnLoot() {
        if (player == null) return;
        var rnd = new Random();
        if (rnd.NextDouble() < 0.4) {                                           // 40% chance to drop a potion
            Console.WriteLine("You find a 'Potion' on the enemy !");
            player.Inventory.Add("Potion");
        }
    }
}
