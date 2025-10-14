using MongoDB.Driver;

namespace Gameplay;
public class ServerService {
    private string db_name = "KeySmasher";
    private readonly IMongoDatabase? _database;
    private readonly IMongoCollection<Player>? _players;

    // ? Will be updated when servers are fully online (allow offline mode)
    public ServerService() {
        var client = new MongoClient("mongodb://localhost:27017");
        try {
            _database = client.GetDatabase(db_name);
            MainProgram.WriteColoredMessage("Connection to database established !", ConsoleColor.Green);
        } catch (Exception e) {
            throw new ArgumentException($"Error while connecting to database : {e.Message}");
        }

        var collections = _database.ListCollectionNames().ToList();
        if (!collections.Contains("Players")) {
            _database.CreateCollection("Players");
            MainProgram.WriteColoredMessage("Collection 'Players' created successfully !", ConsoleColor.Green);
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
            MainProgram.WriteColoredMessage("No user found with this username.", ConsoleColor.Yellow);
            return new();
        } else if (!CryptoUtils.VerifyPassword(password, joueur.Salt, joueur.PasswordHash)) {
            MainProgram.WriteColoredMessage("Incorrect password !");
            return new();
        }

        MainProgram.WriteColoredMessage("Connection successful !", ConsoleColor.Green);
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
            MainProgram.WriteColoredMessage($"Profile '{joueur.name}' updated !", ConsoleColor.Green);
        } else {
            _players.InsertOne(joueur);
            MainProgram.WriteColoredMessage($"Profile '{joueur.name}' created !", ConsoleColor.Green);
        }
    }
}
