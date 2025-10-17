using Security;
using Services;
using Core;

namespace Gameplay;
public class Game {                                                             // Manage server and saves
    private string game_name = "Key smasher";
    private readonly ServerService server = new();                              // Manage database
    private readonly Gameplay gameplay = new();                                 // Manage player
    private User? user;
    private string user_name = "";
    private readonly int min_username = 3;
    private readonly int max_username = 15;
    private readonly int min_password = 6;                                      // No max lenght for password

    public void Main() {
        new Game().Menu();
    }

    private void Menu() {
        Console.Title = game_name;                                              // Rename console

        while (true) {
            Console.Clear();                                                    // Messages from server won't be visible
            Console.WriteLine("===== MENU " + (server.is_connected ? "" : " [OFFLINE]") + " =====");
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
                    case 1: CreateUser(); break;
                    case 2: LoadUser(); break;
                    case 3: Quit(); return;
                    default:
                        WriteColoredMessage("\nIncorrect choice !");
                        break;
                }
            } else {                                                            // If connected
                switch (choice) {
                    case 1: gameplay.player.Present(); break;
                    case 2: CreateUser(); break;
                    case 3: LoadUser(); break;
                    case 4: Play(); break;
                    case 5: ShowLeaderboard(); break;
                    case 6: Save(); break;
                    case 7: Quit(); return;
                    default:
                        WriteColoredMessage("\nIncorrect choice !");
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

    private void CreateUser() {
        AskUsername();

        User? existing = server.GetOnlineUserByUsername(user_name);
        if (existing != null) {
            Console.WriteLine($"\nA profile with username '{existing.Username}' already exists.");
            Console.Write("Do you want to connect to this profile ? (O/N) : ");

            if (Console.ReadKey().Key == ConsoleKey.O) ConnectProfile(existing);
            else Console.WriteLine("\nPlease choose another username.");
        } else {                                                                  // If user doesn't exists
            string password, confirm;
            do {
                password = AskHiddenPassword("\nChoose a password : ");
                confirm =  AskHiddenPassword("\nConfirm password  : ");

                if (password != confirm)
                    WriteColoredMessage("Passwords doesn't match, please try again.\n", ConsoleColor.Yellow);
            } while (password != confirm);

            Save();                                                             // Save current user before

            string salt = CryptoUtils.GenerateSalt();
            string hashed = CryptoUtils.HashPassword(password, salt);
            user = new User(user_name, hashed, salt);
            gameplay.player = new Player(user_name, user.id);
            gameplay.save = new Save(gameplay.player.id);
            SaveLocal();                                                        // Only local save at first
            gameplay.player.Present();
        }
    }

    private void ConnectProfile(User existing) {                                // Load user online data
        string password = AskHiddenPassword();

        if (CryptoUtils.VerifyPassword(password, existing.Salt, existing.PasswordHash)) {
            Save();                                                             // Save current user before
            user = existing;
            gameplay.player = server.GetOnlinePlayerByName(existing.Username);
            if (gameplay.player == null) gameplay.player = new Player(user_name, user.id);  // Shouldn't happen
            gameplay.save = server.GetOnlineGameByPlayerId(gameplay.player.id);
            WriteColoredMessage("\nConnection successful !", ConsoleColor.Green);
            gameplay.player.Present();
        } else WriteColoredMessage("\nIncorrect password !");
    }

    private void LoadUser() {
        AskUsername();
        var (local_user, player, save, enemy) = LoadLocal();
        string name = (local_user == null) ? user_name : local_user.Username;   // User and player should have same name
        User? existing = server.GetOnlineUserByUsername(name);
        if (local_user != null && user != null && local_user.Username == user.Username) // If already connected
            WriteColoredMessage($"You're already connected as '{user_name}'!", ConsoleColor.Yellow);
        else if (existing != null) ConnectProfile(existing);
        else if (local_user != null && player != null) {                        // If no online save, load local save
            string password = AskHiddenPassword();

            if (CryptoUtils.VerifyPassword(password, local_user.Salt, local_user.PasswordHash)) {
                if (user != null) Save();                                       // Save previous user if exists
                user = local_user;
                gameplay.player = player;
                gameplay.save = save;
                gameplay.enemy = enemy;
                WriteColoredMessage("\nNo online save found, loaded local save.", ConsoleColor.Yellow);
                gameplay.player.Present();
            } else WriteColoredMessage("\nIncorrect password !");
        } else WriteColoredMessage($"No profile with username '{name}' found.", ConsoleColor.Yellow);
    }

    private (User?, Player?, Save?, Enemy?) LoadLocal() {                       // Return instances of local user json files
        (User?, Player?, Save?, Enemy?) empty = (null, null, null, null);
        if (user_name == null) return empty;

        try {
            User? user = server.LoadLocalUser(user_name);
            Player? player = server.LoadLocalPlayer(user_name);
            Save? save = server.LoadLocalSave(user_name);
            Enemy? enemy = server.LoadLocalEnemy(user_name);
            return (user, player, save, enemy);
        } catch (Exception e) {
            WriteColoredMessage($"\nError while reading local save : {e.Message}");
            return empty;
        }
    }

    private void Play() {
        if (gameplay.player == null) {
            WriteColoredMessage("\nYou're not connected !", ConsoleColor.Yellow);
            Console.WriteLine("Please connect with 'Create an account' [1] " +
                "or 'Load an account' [2] to play.");
            return;
        }

        gameplay.StartGame(this, gameplay);
    }

    public void ShowLeaderboard(int limit = 5) {
        Console.WriteLine("\n===== LEADERBOARD =====\n");

        List<Player>? topPlayers = server.GetTopPlayers(limit);

        if (topPlayers == null || topPlayers.Count == 0) {
            WriteColoredMessage("\nNo player found for leaderboard.", ConsoleColor.Yellow);
            Console.WriteLine("Save your progress and become Top 1 !");
            return;
        }

        int rank = 1;
        foreach (Player p in topPlayers) {
            Console.WriteLine($"{rank,2}. {p.Name,-15} | Score : {p.Score,5} | Level : {p.Level}");
            if (user != null && user.Username == p.Name) Console.WriteLine(" [You]");
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

        SaveLocal();                                                            // Instances to json
        SaveOnline();                                                           // Json to database
        _ = CleanDatabase();
    }

    private void SaveLocal() {                                                  // Create or update json file (player data)
        try {
            server.SaveLocalUser(user);
            server.SaveLocalPlayer(user, gameplay.player);
            server.SaveLocalGame(user, gameplay.save);
            server.SaveLocalEnemy(user, gameplay.enemy);
            WriteColoredMessage("Local save complete !", ConsoleColor.Green);
        } catch (Exception e) {
            WriteColoredMessage($"Error while saving : {e.Message}");
        }
    }

    private void SaveOnline() {                                                 // Send json files in database
        var (local_user, player, save, enemy) = LoadLocal();
        if (local_user == null)
            WriteColoredMessage("No local save for now.", ConsoleColor.Yellow);
        else {
            try {
                server.SaveOnlineUser(local_user);
                if (player != null) server.SaveOnlinePlayer(player);
                if (save != null) server.SaveOnlineGame(save);
                if (enemy != null) server.SaveOnlineEnemy(enemy);
                WriteColoredMessage("Online save complete !", ConsoleColor.Green);
            } catch (Exception e) {
                WriteColoredMessage($"Error while saving : {e.Message}");
            }
        }
    }

    private async Task CleanDatabase() {
        await server.CleanOrphanEnemiesAsync();
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
