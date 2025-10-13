using System.Text.Json;

// . Write all texts in english
// ? Make WriteError function for text in red
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
                Console.WriteLine("1 - Créer un compte");
                Console.WriteLine("2 - Charger un compte");
                Console.WriteLine("3 - Quitter");
                Console.Write("\nVotre choix : ");
            } else {                                                            // If connected
                Console.WriteLine("1 - Afficher mon profile");
                Console.WriteLine("2 - Créer un nouveau profile");
                Console.WriteLine("3 - Charger un autre profile");
                Console.WriteLine("4 - Jouer");
                Console.WriteLine("5 - Afficher le leaderboard");
                Console.WriteLine("6 - Sauvegarder");
                Console.WriteLine("7 - Quitter");
                Console.Write("\nVotre choix : ");
            }

            string? choice = Console.ReadLine();

            if (gameplay.player == null) {
                switch (choice) {
                    case "1": CreateProfile(); break;
                    case "2": Load(); break;
                    case "3": Quit(); return;
                    default:
                        Console.WriteLine("Choix invalide !");
                        Console.ReadKey();
                        break;
                }
            } else {                                                            // If connected
                switch (choice) {
                    case "1": gameplay.player.Present(); Console.ReadKey(); break;
                    case "2": CreateProfile(); break;
                    case "3": Load(); break;
                    case "4": Play(); break;
                    case "5": ShowLeaderboard(); break;
                    case "6": Save(); break;
                    case "7": Quit(); return;
                default:
                    Console.WriteLine("Choix invalide !");
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
                password = gameplay.AskHiddenPassword("Choisissez un mot de passe : ");
                confirm =  gameplay.AskHiddenPassword("Confirmez le mot de passe  : ");

                if (password != confirm)
                    Console.WriteLine("Les mots de passe ne correspondent pas, veuillez réessayer.\n");
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
        Console.WriteLine($"\nUn profil avec le nom '{gameplay.player_name}' existe déjà.");
        Console.Write("Souhaitez-vous vous connecter à ce profil ? (O/N) : ");

        if (Console.ReadKey().Key == ConsoleKey.O) {
            string password = gameplay.AskHiddenPassword();
            string hashedInput = CryptoUtils.HashPassword(password, existing.Salt);

            if (hashedInput == existing.PasswordHash) {
                gameplay.player = existing;
                Console.WriteLine("\nConnexion réussie !");
                gameplay.player.Present();
            } else {
                Console.WriteLine("\nMot de passe incorrect !");
                gameplay.player = new();
            }
        } else Console.WriteLine("\nVeuillez choisir un autre nom de joueur.");
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
            Console.WriteLine($"Erreur de lecture du fichier local : {e.Message}");
            return null;
        }
    }

    private void Play() {
        if (gameplay.player == null) {
            Console.WriteLine("Aucun profil connecté !");
            Console.WriteLine("Veuillez vous connecter avec 'Créer un compte' [1] " +
                "ou 'Charger un compte' [2] pour jouer.");
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
            Console.WriteLine("Aucun joueur trouvé !");
            return;
        }

        int rank = 1;
        foreach (Player p in topPlayers) {
            Console.WriteLine($"{rank,2}. {p.name,-15} | Score : {p.score,5} | Niveau : {p.level}");
            rank++;
        }

        Console.WriteLine("\n" + new string('=', 25));

        if (gameplay.player != null) {
            bool synced = server.IsPlayerSynced(gameplay.player);
            if (!synced) {                                                      // Check if player is up-to-date
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine($"Vos données en ligne ne sont pas à jour !");
                Console.WriteLine("Pensez à sauvegarder [6] pour synchroniser votre progrès.");
            } else {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Profil à jour");
            }
            Console.ResetColor();
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
        Console.WriteLine($"Partie sauvegardée !");
    }

    private bool SaveOnline() {                                                 // Send json file to database
        if (gameplay.player == null) return false;

        Player? local = LoadLocal();
        if (local == null) {
            Console.WriteLine("Aucune sauvegarde pour le moment.");
            return false;
        }

        try {
            server.SavePlayer(local);
            return true;
        } catch (Exception e) {
            Console.WriteLine($"Erreur lors de la sauvegarde : {e.Message}");
            return false;
        }
    }

    private void Quit() {
        Save();                                                                 // Auto save before leaving game
    }
}
