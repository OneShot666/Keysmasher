using MongoDB.Driver;

// ! Make API and backend server (online) -> use Render (separate it from game code)
// L Transform project into .exe
#pragma warning disable CS8600                                                  // Disable possible null object warnings
#pragma warning disable CS8601
namespace Gameplay;
public class MainProgram {
    public string game_name = "Key smasher";
    private Player? joueur;
    private string player_name = "";
    private readonly ServerService server = new();
    private readonly int min_username = 3;
    private readonly int max_username = 15;
    private readonly int min_password = 6;

    public static void Main() {                                                 // Cmd admin : 'dotnet run' to launch (for now)
        new MainProgram().Menu();
    }

    private void Menu() {
        while (true) {
            Console.Clear();                                                    // Messages from Connect() won't be visible
            Console.WriteLine("===== MENU =====");
            if (joueur == null) {
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

            string? choix = Console.ReadLine();

            if (joueur == null) {
                switch (choix) {
                    case "1": CreateProfile(); break;
                    case "2": Load(); break;
                    case "3": Quit(); return;
                    default:
                        Console.WriteLine("Choix invalide !");
                        Console.ReadKey();
                        break;
                }
            } else {                                                            // If connected
                switch (choix) {
                    case "1": joueur.Present(); Console.ReadKey(); break;
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
        AskName();

        var existing = server.FindPlayerByName(player_name);
        if (existing != null) ConnectProfile(existing);
        else {
            string password, confirm;
            do {
                password = AskHiddenPassword("Choisissez un mot de passe : ");
                confirm =  AskHiddenPassword("Confirmez le mot de passe  : ");

                if (password != confirm)
                    Console.WriteLine("Les mots de passe ne correspondent pas, veuillez réessayer.\n");
            } while (password != confirm);

            string salt = CryptoUtils.GenerateSalt();
            string hashed = CryptoUtils.HashPassword(password, salt);

            joueur = new Player { Nom = player_name, Niveau = 1, Score = 0, PasswordHash = hashed, Salt = salt };
            server.SavePlayer(joueur);
            joueur.Present();
        }
        Console.ReadKey();
    }

    private void ConnectProfile(Player existing) {
        Console.WriteLine($"\nUn profil avec le nom '{player_name}' existe déjà.");
        Console.Write("Souhaitez-vous vous connecter à ce profil ? (O/N) : ");

        if (Console.ReadKey().Key == ConsoleKey.O) {
            string password = AskHiddenPassword();
            string hashedInput = CryptoUtils.HashPassword(password, existing.Salt);

            if (hashedInput == existing.PasswordHash) {
                joueur = existing;
                Console.WriteLine("\nConnexion réussie !");
                joueur.Present();
            } else {
                Console.WriteLine("\nMot de passe incorrect !");
                joueur = null;
            }
        } else Console.WriteLine("\nVeuillez choisir un autre nom de joueur.");
    }

    private void Load() {
        AskName();
        string password = AskHiddenPassword();

        joueur = server.LoadPlayer(player_name, password);
        if (joueur != null) joueur.Present();
        Console.ReadKey();
    }

    private void Play() {
        if (joueur == null) {
            Console.WriteLine("Aucun profil connecté !");
            Console.WriteLine("Veuillez vous connecter avec 'Nouvelle partie' [1] ou 'Charger' [2] pour jouer.");
            Console.ReadKey();
            return;
        }

        Console.Clear();
        Console.WriteLine($"===== {game_name} =====");

        Random rnd = new();
        while (true) {
            char lettre = (char)rnd.Next('A', 'Z' + 1);
            Console.Write($"\nAppuyez sur la touche '{lettre}' : ");
            ConsoleKeyInfo touche = Console.ReadKey(true);

            if (touche.KeyChar.ToString().ToUpper() == lettre.ToString()) {
                joueur.Score += 10;
                joueur.CheckLevel();
                Console.WriteLine($"\nBravo ! Score = {joueur.Score}, Niveau = {joueur.Niveau}");
            } else {
                Console.WriteLine($"\nRaté ! Score = {joueur.Score}");
                joueur.Score -= 10;
                if (joueur.Score < 0) joueur.Score = 0;
                joueur.CheckLevel();
            }

            Console.Write("Quitter ? (O/N) : ");
            if (Console.ReadKey().Key == ConsoleKey.O)
                break;
            Console.WriteLine();
        }
    }

    public void ShowLeaderboard(int limit = 5) {
        Console.WriteLine("\n===== LEADERBOARD =====\n");

        List<Player> topPlayers = server.GetTopPlayers(limit);

        if (topPlayers.Count == 0) {
            Console.WriteLine("Aucun joueur trouvé !");
            return;
        }

        int rank = 1;
        foreach (var p in topPlayers) {
            Console.WriteLine($"{rank,2}. {p.Nom,-15} | Score : {p.Score,5} | Niveau : {p.Niveau}");
            rank++;
        }

        Console.WriteLine("\n" + new string('=', 25));

        if (joueur != null) {                                                   // Always the case here (warning CS8604)
            bool synced = server.IsPlayerSynced(joueur);
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

    private void Save() {
        if (joueur == null) return;

        server.SavePlayer(joueur);
        Console.ReadKey();
    }

    private void Quit() {
        Save();                                                                 // Auto save before leaving game
    }

    private void AskName() {
        do {
            Console.Write("\nVotre nom : ");
            player_name = Console.ReadLine()?.Trim() ?? "";
            if (player_name.Length < min_username || player_name.Length > max_username)
                Console.WriteLine("Le pseudo doit contenir entre 3 et 15 caractères !");
        } while (player_name.Length < min_username || player_name.Length > max_username);
    }

    private string AskHiddenPassword(string prompt = "\nMot de passe : ") {
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
}
