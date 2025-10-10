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
            .SortByDescending(p => p.Score).Limit(limit).ToList();
    }

    public bool IsPlayerSynced(Player localPlayer) {                            // Only compare important data
        if (_players == null) return false;
        var dbPlayer = _players.Find(p => p.Nom == localPlayer.Nom).FirstOrDefault();
        if (dbPlayer == null) return false;
        return dbPlayer.Score == localPlayer.Score && dbPlayer.Niveau == localPlayer.Niveau;
    }

    public Player? FindPlayerByName(string name) {
        if (_players == null) return null;
        return _players.Find(p => p.Nom == name).FirstOrDefault();
    }

    public Player? LoadPlayer(string nom, string password) {                    // Load from db
        if (_players == null) return null;
        var joueur = _players.Find(p => p.Nom == nom).FirstOrDefault();
        if (joueur == null) {
            Console.WriteLine("Aucun joueur trouvé avec ce nom.");
            return null;
        }

        if (!CryptoUtils.VerifyPassword(password, joueur.Salt, joueur.PasswordHash)) {
            Console.WriteLine("Mot de passe incorrect !");
            return null;
        }

        Console.WriteLine("Connexion réussie !");
        return joueur;
    }

    public void SavePlayer(Player joueur) {                                     // Save in db
        if (_players == null) return;
        var existing = _players.Find(p => p.Nom == joueur.Nom).FirstOrDefault();
        if (existing != null) {
            joueur.id = existing.id;                                            // Keep the same id
            joueur.Score = (int) MathF.Max(joueur.Score, existing.Score);       // Keep best score and lvl
            joueur.Niveau = (int) MathF.Max(joueur.Niveau, existing.Niveau);
            _players.ReplaceOne(p => p.Nom == joueur.Nom, joueur);
            Console.WriteLine($"Profil '{joueur.Nom}' mis à jour !");
        } else {
            _players.InsertOne(joueur);
            Console.WriteLine($"Profil '{joueur.Nom}' créé !");
        }
    }
}
