// ! Move TakeDamage() function in player (in StartCombat() at line 152)
// ! Also make TakeDamage() function in enemy
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
            Console.WriteLine("1 - Attack");
            Console.WriteLine("2 - Defend");
            Console.WriteLine("3 - Heal");
            Console.WriteLine("4 - Open inventory");
            Console.WriteLine("5 - See profile");
            Console.WriteLine("6 - Save");
            Console.WriteLine("7 - Back to Menu");
            Console.Write("\nChoice : ");

            if (!int.TryParse(Console.ReadLine(), out int choice)) {            // ? Replace
                Console.WriteLine("Entrée invalide.");
                continue;
            }

            switch (choice) {
                case 1:
                    StartCombat();
                    break;
                case 2:
                    Console.WriteLine("Tu prends une posture défensive. (Utilité en combat uniquement)");
                    // ? Could toggle a defend flag for next combat round
                    // ? Can also just double defense for next round
                    break;
                case 3:
                    player.UsePotion();
                    break;
                case 4:
                    player.ShowInventory();
                    break;
                case 5:
                    player.Present();
                    break;
                case 6:
                    program.Save();
                    break;
                case 7:
                    running = false;
                    break;
                default:
                    Console.WriteLine("Incorrect choice !");
                    break;
            }
        }
    }

    public void AskName() {
        do {
            Console.Write("\nVotre nom : ");
            player_name = Console.ReadLine()?.Trim() ?? "";
            if (player_name.Length < min_username || player_name.Length > max_username)
                Console.WriteLine("Le pseudo doit contenir entre 3 et 15 caractères !");
        } while (player_name.Length < min_username || player_name.Length > max_username);
    }

    public string AskHiddenPassword(string prompt = "\nMot de passe : ") {
        string password = "";
        do {
            Console.Write(prompt);
            ConsoleKeyInfo key;

            while (true) {
                key = Console.ReadKey(intercept: true);

                if (key.Key == ConsoleKey.Enter) {                              // Confirm password
                    if (password.Length < min_password)
                        Console.WriteLine("\nLe mot de passe est trop court !");
                    else Console.WriteLine();
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
        Console.WriteLine($"\nUn {enemy.name} apparaît ! (HP {enemy.hp})");

        bool playerdefending = false;
        var rnd = new Random();

        while (enemy.hp > 0 && player.hp > 0) {
            Console.WriteLine($"\nTon HP: {player.hp}/{player.maxHp} | Ennemi HP: {enemy.hp}");
            Console.WriteLine("1 Attack  2 defend  3 Heal  4 Flee");
            Console.Write("Action: ");
            var input = Console.ReadLine();

            switch (input) {
                case "1":
                    int dmg = Math.Max(0, player.atk - enemy.def);
                    // add small variance
                    dmg += rnd.Next(0, 4);
                    enemy.hp -= dmg;
                    Console.WriteLine($"Tu attaques et infliges {dmg} dégâts.");
                    break;
                case "2":
                    playerdefending = true;
                    Console.WriteLine("Tu défends (moins de dégâts reçus au prochain tour).");
                    break;
                case "3":
                    player.UsePotion();
                    break;
                case "4":
                    if (rnd.NextDouble() < 0.5) {
                        Console.WriteLine("Tu prends la fuite ! (Réussi)");
                        return;
                    } else Console.WriteLine("La fuite a échoué !");
                    break;
                default:
                    Console.WriteLine("Action non reconnue.");
                    break;
            }

            if (enemy.hp <= 0) {
                Console.WriteLine($"Tu as vaincu le {enemy.name} !");
                int goldGain = enemy.level * 5;
                player.gold += goldGain;
                Console.WriteLine($"Tu récupères {goldGain} pièces d'or. Total or: {player.gold}");
                SpawnLoot();
                // give xp and maybe level up (simple)
                player.GainXp(20);
                return;
            }

            // Enemy turn
            int enemyDmg = Math.Max(0, enemy.atk - (playerdefending ? player.def * 2 : player.def));
            // minor variance
            enemyDmg += rnd.Next(0, 3);
            player.hp -= enemyDmg;
            Console.WriteLine($"{enemy.name} attaque et inflige {enemyDmg} dégâts.");

            if (player.hp <= 0) {
                Console.WriteLine("Tu es mort... Respawn au village avec la moitié des PV.");
                player.hp = Math.Max(1, player.maxHp / 2);
                player.gold = Math.Max(0, player.gold - 10);
                return;
            }

            // reset defend
            playerdefending = false;
        }
    }

    public void SpawnLoot() {
        if (player == null) return;
        var rnd = new Random();
        if (rnd.NextDouble() < 0.4) {
            Console.WriteLine("Tu trouves une Potion sur l'ennemi !");
            player.Inventory.Add("Potion");
        }
    }
}
