using System.Text.Json;

// . Add 'Saves' table in db and connect them to player -> make Save class
// ! Add API and backend server online -> use Render
// L Transform project into .exe and/or a website
namespace Gameplay;                                                             // Avoid ambiguity with api
public class MainProgram {                                                      // Manage server and saves
    public string game_name = "Key smasher";
    private readonly Gameplay gameplay = new();
    private readonly ServerService server = new();
    private readonly string save_path = "Saves/";

    public static void Main() {                                                 // Cmd admin : 'dotnet run' to launch (for now)
        new MainProgram().Menu();
    }

    private void Menu() {
        Console.Title = game_name;                                              // Rename console

        while (true) {
            Console.Clear();                                                    // Messages from Connect() won't be visible
            Console.WriteLine("===== MENU =====");
            if (gameplay.player == null) {
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

            if (gameplay.player == null) {
                switch (choice) {
                    case 1: CreateProfile(); break;
                    case 2: Load(); break;
                    case 3: Quit(); return;
                    default:
                        WriteColoredMessage("Incorrect choice !");
                        Console.ReadKey();
                        break;
                }
            } else {                                                            // If connected
                switch (choice) {
                    case 1: gameplay.player.Present(); Console.ReadKey(); break;
                    case 2: CreateProfile(); break;
                    case 3: Load(); break;
                    case 4: Play(); break;
                    case 5: ShowLeaderboard(); break;
                    case 6: Save(); break;
                    case 7: Quit(); return;
                    default:
                        WriteColoredMessage("Incorrect choice !");
                        Console.ReadKey();
                        break;
                }
            }
        }
    }

    private void CreateProfile() {
        gameplay.AskName();

        Player? existing = server.FindPlayerByName(gameplay.player_name);
        if (existing != null) ConnectProfile(existing);
        else {                                                                  // Create password
            string password, confirm;
            do {
                password = gameplay.AskHiddenPassword("Choose a password : ");
                confirm =  gameplay.AskHiddenPassword("Confirm password  : ");

                if (password != confirm)
                    WriteColoredMessage("Passwords doesn't match, please try again.\n", ConsoleColor.Yellow);
            } while (password != confirm);

            string salt = CryptoUtils.GenerateSalt();
            string hashed = CryptoUtils.HashPassword(password, salt);

            gameplay.player = new Player(gameplay.player_name, hashed, salt);
            SaveLocal();
            gameplay.player.Present();
        }
        Console.ReadKey();
    }

    private void ConnectProfile(Player existing) {
        Console.WriteLine($"\nA profile with username '{gameplay.player_name}' already exists.");
        Console.Write("Do you want to connect to this profile ? (O/N) : ");

        if (Console.ReadKey().Key == ConsoleKey.O) {
            string password = gameplay.AskHiddenPassword();
            string hashedInput = CryptoUtils.HashPassword(password, existing.Salt);

            if (hashedInput == existing.PasswordHash) {
                gameplay.player = existing;
                WriteColoredMessage("\nConnection successful !", ConsoleColor.Green);
                gameplay.player.Present();
            } else {
                WriteColoredMessage("\nIncorrect password !");
                gameplay.player = new();
            }
        } else Console.WriteLine("\nPlease choose another username.");
    }

    private void Load() {
        gameplay.AskName();
        Player? local = LoadLocal();
        string name = (local == null) ? gameplay.player_name : local.name;

        string password = gameplay.AskHiddenPassword();
        gameplay.player = server.LoadPlayer(name, password);
        if (gameplay.player != null) gameplay.player.Present();
        Console.ReadKey();
    }

    private Player? LoadLocal() {                                               // Load player json file data
        if (gameplay.player == null) return null;

        string chemin = Path.Combine(save_path, $"{gameplay.player.name}.json");
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
            Console.ReadKey();
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
            Console.WriteLine($"{rank,2}. {p.name,-15} | Score : {p.score,5} | Level : {p.level}");
            rank++;
        }

        Console.WriteLine("\n" + new string('=', 25));

        if (gameplay.player != null) {
            bool synced = server.IsPlayerSynced(gameplay.player);               // Check if player is up-to-date
            if (!synced) WriteColoredMessage("Your online data is out of date!" +
                "\nRemember to save [6] to synchronize your progress.", ConsoleColor.Yellow);
            else WriteColoredMessage("Profile is updated", ConsoleColor.Green);
        }

        Console.ReadKey();
    }

    public void Save() {
        if (gameplay.player == null) return;

        SaveLocal();
        SaveOnline();
        Console.ReadKey();
    }

    private void SaveLocal() {                                                  // Create or update json file (player data)
        if (gameplay.player == null) return;

        Directory.CreateDirectory(save_path);                                   // Create directory if doesn't exists
        string chemin = Path.Combine(save_path, $"{gameplay.player.name}.json");
        string json = JsonSerializer.Serialize(gameplay.player, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(chemin, json);
        WriteColoredMessage("Profile saved !", ConsoleColor.Green);
    }

    private bool SaveOnline() {                                                 // Send json file to database
        if (gameplay.player == null) return false;

        Player? local = LoadLocal();
        if (local == null) {
            WriteColoredMessage("No local save for now", ConsoleColor.Yellow);
            return false;
        }

        try {
            server.SavePlayer(local);
            return true;
        } catch (Exception e) {
            WriteColoredMessage($"Error while saving : {e.Message}");
            return false;
        }
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
