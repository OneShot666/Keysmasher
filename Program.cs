using System.Text.Json;

// . Check new structure work : launch program
// ! Add API and backend server online -> use Render
// L Transform project into .exe and/or a website
namespace Gameplay;                                                             // Avoid ambiguity with api
public class MainProgram {                                                      // Manage server and saves
    public string game_name = "Key smasher";
    private User? user;
    public string user_name = "";
    private readonly Gameplay gameplay = new();                                 // Manage player
    private readonly ServerService server = new();                              // Manage database
    private readonly string save_path = "Saves/";
    private readonly int min_username = 3;
    private readonly int max_username = 15;
    private readonly int min_password = 6;                                      // No max lenght for password for now

    public static void Main() {                                                 // Cmd admin : 'dotnet run' to launch (for now)
        new MainProgram().Menu();
    }

    private void Menu() {
        Console.Title = game_name;                                              // Rename console

        while (true) {
            Console.Clear();                                                    // Messages from Connect() won't be visible
            Console.WriteLine("===== MENU =====");
            if (user == null) {
                Console.WriteLine("1 - Create an account");
                Console.WriteLine("2 - Load an account");
                Console.WriteLine("3 - Quit");
            } else {                                                            // If connected
                Console.WriteLine("1 - Show profile");
                Console.WriteLine("2 - Create a new account");
                Console.WriteLine("3 - Load a new account");
                Console.WriteLine("4 - Play");
                Console.WriteLine("5 - Show leaderboard");
                Console.WriteLine("6 - Save");
                Console.WriteLine("7 - Quit");
            }

            int choice = Gameplay.AskIntChoice();                               // Get input integer

            if (user == null || gameplay.player == null) {
                switch (choice) {
                    case 1: CreateProfile(); break;
                    case 2: Load(); break;
                    case 3: Quit(); return;
                    default:
                        WriteColoredMessage("Incorrect choice !");
                        break;
                }
            } else {                                                            // If connected
                switch (choice) {
                    case 1: gameplay.player.Present(); break;
                    case 2: CreateProfile(); break;
                    case 3: Load(); break;
                    case 4: Play(); break;
                    case 5: ShowLeaderboard(); break;
                    case 6: Save(); break;
                    case 7: Quit(); return;
                    default:
                        WriteColoredMessage("Incorrect choice !");
                        break;
                }
            }
            Console.ReadKey();
        }
    }

    public void AskUsername() {
        do {
            Console.Write("\nUsername : ");
            user_name = Console.ReadLine()?.Trim() ?? "";
            if (user_name.Length < min_username || user_name.Length > max_username)
                WriteColoredMessage("Username must contain between 3 and 15 characters!",
                    ConsoleColor.Yellow);
        } while (user_name.Length < min_username || user_name.Length > max_username);
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
                        WriteColoredMessage("\nPassword is too short !", ConsoleColor.Yellow);
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

    private void CreateProfile() {
        AskUsername();

        User? existing = server.GetUserByUsername(user_name);
        if (existing != null) {
            Console.WriteLine($"\nA profile with username '{gameplay.player_name}' already exists.");
            Console.Write("Do you want to connect to this profile ? (O/N) : ");

            if (Console.ReadKey().Key == ConsoleKey.O) ConnectProfile(existing);
            else Console.WriteLine("\nPlease choose another username.");
        } else {                                                                  // If user doesn't exists
            string password, confirm;
            do {
                password = AskHiddenPassword("Choose a password : ");
                confirm =  AskHiddenPassword("Confirm password  : ");

                if (password != confirm)
                    WriteColoredMessage("Passwords doesn't match, please try again.\n", ConsoleColor.Yellow);
            } while (password != confirm);

            string salt = CryptoUtils.GenerateSalt();
            string hashed = CryptoUtils.HashPassword(password, salt);

            user = new User(user_name, hashed, salt);
            gameplay.player = new Player(user_name, user.id);
            SaveLocal();
            gameplay.player.Present();
        }
    }

    private void ConnectProfile(User existing) {                                // Load user and player online data
        string password = AskHiddenPassword();
        string hashedInput = CryptoUtils.HashPassword(password, existing.Salt);

        if (hashedInput == existing.PasswordHash) {
            gameplay.player = server.GetPlayerByName(existing.Username);
            if (gameplay.player == null) return;                            // Shouldn't happened
            WriteColoredMessage("\nConnection successful !", ConsoleColor.Green);
            gameplay.player.Present();
        } else {
            WriteColoredMessage("\nIncorrect password !");
            user = null;
            gameplay.player = null;
        }
    }

    private void Load() {
        AskUsername();
        Player? local = LoadLocal();
        string name = (local == null) ? user_name : local.Name;                 // User and player should have same name
        User? existing = server.GetUserByUsername(name);
        if (existing != null) ConnectProfile(existing);
    }

    private Player? LoadLocal() {                                               // Load player json file data
        if (user_name == null) return null;

        string chemin = Path.Combine(save_path, $"{user_name}.json");
        if (!File.Exists(chemin)) return null;                                  // If local save doesn't exists

        try {
            string contenu = File.ReadAllText(chemin);
            return JsonSerializer.Deserialize<Player>(contenu);                 // Return player instance
        } catch (Exception e) {
            WriteColoredMessage($"Error while reading local save : {e.Message}");
            return null;
        }
    }

    private void Play() {
        if (gameplay.player == null) {
            WriteColoredMessage("You're not connected !", ConsoleColor.Yellow);
            Console.WriteLine("Please connect with 'Create an account' [1] " +
                "or 'Load an account' [2] to play.");
            return;
        }

        gameplay.StartGame(this);
    }

    public void ShowLeaderboard(int limit = 5) {
        Console.WriteLine("\n===== LEADERBOARD =====\n");

        List<Player>? topPlayers = server.GetTopPlayers(limit);
        if (topPlayers == null) return;                                         // ! Add error message

        if (topPlayers.Count == 0) {
            WriteColoredMessage("No player found for leaderboard.", ConsoleColor.Yellow);
            Console.WriteLine("Save your progress and become Top 1 !");
            return;
        }

        int rank = 1;
        foreach (Player p in topPlayers) {
            Console.WriteLine($"{rank,2}. {p.Name,-15} | Score : {p.Score,5} | Level : {p.Level}");
            rank++;
        }

        Console.WriteLine("\n" + new string('=', 25));

        if (gameplay.player != null) {
            bool synced = server.IsPlayerSynced(gameplay.player);               // Check if player is up-to-date
            if (!synced) WriteColoredMessage("Your online data is out of date!" +
                "\nRemember to save [6] to synchronize your progress.", ConsoleColor.Yellow);
            else WriteColoredMessage("Profile is updated", ConsoleColor.Green);
        }
    }

    public void Save() {
        if (gameplay.player == null) return;

        SaveLocal();                                                            // Only save player's data
        SaveAllOnline();
    }

    private void SaveLocal() {                                                  // Create or update json file (player data)
        if (gameplay.player == null) return;

        Directory.CreateDirectory(save_path);                                   // Create directory if doesn't exists
        string chemin = Path.Combine(save_path, $"{user_name}.json");
        string json = JsonSerializer.Serialize(gameplay.player, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(chemin, json);
        WriteColoredMessage("Profile saved !", ConsoleColor.Green);             // Locally
    }

    private void SaveAllOnline() {                                              // Send json file to database
        if (user != null) server.SaveUser(user);

        Player? local = LoadLocal();
        if (local == null)
            WriteColoredMessage("No local save for now", ConsoleColor.Yellow);
        else {
            try {
                server.SavePlayer(local);
            } catch (Exception e) {
                WriteColoredMessage($"Error while saving : {e.Message}");
            }
        }

        // !! Add createSave() function in MainProgram

        if (gameplay.enemy != null) server.SaveEnemy(gameplay.enemy);
    }

    public static void WriteColoredMessage(string message, ConsoleColor color = ConsoleColor.Red) {
        Console.ForegroundColor = color;
        Console.WriteLine(message);
        Console.ResetColor();
    }

    private void Quit() {
        Save();                                                                 // Auto save before leaving game
    }
}
