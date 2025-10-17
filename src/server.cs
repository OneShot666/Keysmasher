using MongoDB.Driver;
using MongoDB.Bson;

namespace Gameplay;
public class ServerService {
    private bool is_connected = false;
    private string db_name = "KeySmasher";
    // private string db_name = "FakeDatabase";
    private readonly IMongoDatabase? _database;
    private IMongoCollection<User>? _users { get; }
    private IMongoCollection<Save>? _saves { get; }
    private IMongoCollection<Player>? _players { get; }
    private IMongoCollection<Enemy>? _enemies { get; }
    private ConsoleColor success = ConsoleColor.Green;                          // For success messages
    private readonly ReplaceOptions options = new ReplaceOptions { IsUpsert = true };   // Insert if doesn't exists

    public ServerService() {
        Console.WriteLine("Connecting to database...");
        var collections = new List<string>();
        try {
            var client = new MongoClient("mongodb://localhost:27017");
            if (!DatabaseExists(client, db_name)) {                             // Try access collections
                MainProgram.WriteColoredMessage("Connection to database failed !");
                return;
            }
            _database = client.GetDatabase(db_name);
            collections = _database.ListCollectionNames().ToList();
            is_connected = true;
            MainProgram.WriteColoredMessage("Connection to database established !", success);
        } catch (Exception e) {
            MainProgram.WriteColoredMessage($"Error while connecting to server : {e.Message}");
        }

        if (!is_connected || _database == null) return;                         // Offline mode (don't load db)
        if (!collections.Contains("Users")) _database.CreateCollection("Users");
        if (!collections.Contains("Saves")) _database.CreateCollection("Saves");
        if (!collections.Contains("Players")) _database.CreateCollection("Players");
        if (!collections.Contains("Enemies")) _database.CreateCollection("Enemies");

        _users = _database.GetCollection<User>("Users");
        _saves = _database.GetCollection<Save>("Saves");
        _players = _database.GetCollection<Player>("Players");
        _enemies = _database.GetCollection<Enemy>("Enemies");
    }

    public static bool DatabaseExists(IMongoClient client, string dbName) {
        try {
            var databaseNames = client.ListDatabaseNames().ToList();
            return databaseNames.Contains(dbName);
        } catch { return false; }                                               // Unable to connect to server
    }

    /* ----- USERS ----- */
    public User? GetUserById(ObjectId userId) {                                 // Load user (to find player's user)
        return is_connected ? _users.Find(p => p.id == userId).FirstOrDefault() : null;
    }

    public User? GetUserByUsername(string username) {                           // Load user
        return is_connected ? _users.Find(u => u.Username == username).FirstOrDefault() : null;
    }

    public void CreateUser(User user) {
        if (is_connected || _users == null) return;
        _users.InsertOne(user);
        MainProgram.WriteColoredMessage("User created successfully !", success);
    }

    public void SaveUser(User user) {
        if (is_connected || _users == null) return;
        var filter = Builders<User>.Filter.Eq(u => u.id, user.id);
        _users.ReplaceOne(filter, user, options);
    }

    /* ----- SAVES ----- */
    public Save? GetSaveByPlayerId(ObjectId playerId) {                         // Load save
        return is_connected ? _saves.Find(s => s.PlayerId == playerId).FirstOrDefault() : null;
    }

    public void SaveGame(Save save) {
        if (is_connected || _saves == null) return;
        var filter = Builders<Save>.Filter.Eq(p => p.PlayerId, save.PlayerId);
        _saves.ReplaceOne(filter, save, options);
        MainProgram.WriteColoredMessage($"Save updated !", success);
    }

    /* ----- PLAYERS ----- */
    public List<Player>? GetTopPlayers(int limit = 5) {                         // Get top 5 players by default
        if (is_connected || _players == null) return null;
        return _players.Find(FilterDefinition<Player>.Empty)
            .SortByDescending(p => p.Score).Limit(limit).ToList();
    }

    public bool IsPlayerSynced(Player localPlayer) {                            // Only compare important data
        if (is_connected || _players == null) return false;
        var dbPlayer = _players.Find(p => p.Name == localPlayer.Name).FirstOrDefault();
        if (dbPlayer == null) return false;
        return dbPlayer.Score == localPlayer.Score && dbPlayer.Level == localPlayer.Level;
    }

    public Player? GetPlayerByName(string name) {                               // Load player
        return is_connected ? _players.Find(p => p.Name == name).FirstOrDefault() : null;
    }

    public void SavePlayer(Player player) {
        if (is_connected || _players == null) return;
        var filter = Builders<Player>.Filter.Eq(p => p.UserId, player.UserId);
        _players.ReplaceOne(filter, player, options);
        MainProgram.WriteColoredMessage($"Profile '{player.Name}' updated !", success);
    }

    /* ----- ENEMIES ----- */
    public Enemy? GetEnemyById(ObjectId enemyId) {                              // Load enemy
        return is_connected ? _enemies.Find(e => e.id == enemyId).FirstOrDefault() : null;
    }

    public void SaveEnemy(Enemy enemy) {
        if (is_connected || _enemies == null) return;
        var filter = Builders<Enemy>.Filter.Eq(p => p.id, enemy.id);
        _enemies.ReplaceOne(filter, enemy, options);
    }

    public void DeleteEnemy(ObjectId enemyId) {
        if (is_connected || _enemies == null) return;
        var filter = Builders<Enemy>.Filter.Eq(e => e.id, enemyId);
        _enemies.DeleteOne(filter);
    }
}
