using MongoDB.Driver;

namespace Gameplay;
public class ServerService {
    private string db_name = "KeySmasher";
    private readonly IMongoDatabase? _database;
    private readonly IMongoCollection<Player>? _players;

    // ! Will be updated when servers are fully online (allow offline mode)
    public ServerService() {
        var client = new MongoClient("mongodb://localhost:27017");
        try {
            _database = client.GetDatabase(db_name);
            Console.WriteLine("Connection to database established !");
        } catch (Exception e) {
            throw new ArgumentException($"Error while connecting to database : {e.Message}");
        }

        var collections = _database.ListCollectionNames().ToList();
        if (!collections.Contains("Players")) {
            _database.CreateCollection("Players");
            Console.WriteLine("Collection 'Players' created successfully !");
        }

        _players = _database.GetCollection<Player>("Players");
    }

    public List<Player>? GetTopPlayers(int limit = 5) {                         // Get top 5 players by default
        if (_players == null) return null;
        return _players.Find(FilterDefinition<Player>.Empty)
            .SortByDescending(p => p.score).Limit(limit).ToList();
    }

    public bool IsPlayerSynced(Player localPlayer) {                            // Only compare important data
        if (_players == null) return false;
        var dbPlayer = _players.Find(p => p.name == localPlayer.name).FirstOrDefault();
        if (dbPlayer == null) return false;
        return dbPlayer.score == localPlayer.score && dbPlayer.level == localPlayer.level;
    }

    public Player? FindPlayerByName(string name) {
        if (_players == null) return null;
        return _players.Find(p => p.name == name).FirstOrDefault();
    }

    public Player LoadPlayer(string nom, string password) {                    // Load from db
        if (_players == null) return new();

        var joueur = _players.Find(p => p.name == nom).FirstOrDefault();
        if (joueur == null) {                                                   // Player not found
            Console.WriteLine("Aucun joueur trouvé avec ce nom.");
            return new();
        } else if (!CryptoUtils.VerifyPassword(password, joueur.Salt, joueur.PasswordHash)) {
            Console.WriteLine("Mot de passe incorrect !");
            return new();
        }

        Console.WriteLine("Connexion réussie !");
        return joueur;
    }

    public void SavePlayer(Player joueur) {                                     // Save in db
        if (_players == null) return;
        var existing = _players.Find(p => p.name == joueur.name).FirstOrDefault();
        if (existing != null) {
            joueur.id = existing.id;                                            // Keep the same id
            joueur.score = (int) MathF.Max(joueur.score, existing.score);       // Keep best score and lvl
            joueur.level = (int) MathF.Max(joueur.level, existing.level);
            _players.ReplaceOne(p => p.name == joueur.name, joueur);
            Console.WriteLine($"Profil '{joueur.name}' mis à jour !");
        } else {
            _players.InsertOne(joueur);
            Console.WriteLine($"Profil '{joueur.name}' créé !");
        }
    }
}
