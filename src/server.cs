using MongoDB.Driver;
using MongoDB.Bson;

namespace Gameplay;
public class ServerService {
    private string db_name = "KeySmasher";
    private readonly IMongoDatabase? _database;
    private IMongoCollection<User>? _users { get; }
    private IMongoCollection<Save>? _saves { get; }
    private IMongoCollection<Player>? _players { get; }
    private IMongoCollection<Enemy>? _enemies { get; }
    private ConsoleColor success = ConsoleColor.Green;                          // For success messages
    private readonly ReplaceOptions options = new ReplaceOptions { IsUpsert = true };   // Insert if doesn't exists

    // ? Will be updated when servers are fully online (allow offline mode)
    public ServerService() {
        var client = new MongoClient("mongodb://localhost:27017");
        try {
            _database = client.GetDatabase(db_name);
            MainProgram.WriteColoredMessage("Connection to database established !", success);
        } catch (Exception e) {
            throw new ArgumentException($"Error while connecting to database : {e.Message}");
        }

        var collections = _database.ListCollectionNames().ToList();             // Create tables if doesn't exists
        if (!collections.Contains("Users")) _database.CreateCollection("Users");
        if (!collections.Contains("Saves")) _database.CreateCollection("Saves");
        if (!collections.Contains("Players")) _database.CreateCollection("Players");
        if (!collections.Contains("Enemies")) _database.CreateCollection("Enemies");

        _users = _database.GetCollection<User>("Users");
        _saves = _database.GetCollection<Save>("Saves");
        _players = _database.GetCollection<Player>("Players");
        _enemies = _database.GetCollection<Enemy>("Enemies");
    }

    /* ----- USERS ----- */
    public User? GetUserByUsername(string username) {                           // Load user
        return _users.Find(u => u.Username == username).FirstOrDefault();
    }

    public void CreateUser(User user) {
        if (_users == null) return;
        _users.InsertOne(user);
        MainProgram.WriteColoredMessage("User created successfully !", success);
    }

    public void SaveUser(User user) {
        if (_users == null) return;
        var filter = Builders<User>.Filter.Eq(u => u.id, user.id);
        _users.ReplaceOne(filter, user, options);
    }

    /* ----- SAVES ----- */
    public Save? GetSaveByPlayerId(ObjectId playerId) {                         // Load save
        return _saves.Find(s => s.PlayerId == playerId).FirstOrDefault();
    }

    public void SaveGame(Save save) {
        if (_saves == null) return;
        var filter = Builders<Save>.Filter.Eq(p => p.PlayerId, save.PlayerId);
        _saves.ReplaceOne(filter, save, options);
        MainProgram.WriteColoredMessage($"Save updated !", success);
    }

    /* ----- PLAYERS ----- */
    public List<Player>? GetTopPlayers(int limit = 5) {                         // Get top 5 players by default
        if (_players == null) return null;
        return _players.Find(FilterDefinition<Player>.Empty)
            .SortByDescending(p => p.Score).Limit(limit).ToList();
    }

    public bool IsPlayerSynced(Player localPlayer) {                            // Only compare important data
        if (_players == null) return false;
        var dbPlayer = _players.Find(p => p.Name == localPlayer.Name).FirstOrDefault();
        if (dbPlayer == null) return false;
        return dbPlayer.Score == localPlayer.Score && dbPlayer.Level == localPlayer.Level;
    }

    public Player? GetPlayerByUserId(ObjectId userId) {                         // ? Get user by user id
        return _players.Find(p => p.UserId == userId).FirstOrDefault();
    }

    public Player? GetPlayerByName(string name) {                               // Load player
        return _players.Find(p => p.Name == name).FirstOrDefault();
    }

    public Player LoadPlayer(string nom, string password) {
        return _players.Find(p => p.Name == nom).FirstOrDefault();
    }

    public void SavePlayer(Player player) {
        if (_players == null) return;
        var filter = Builders<Player>.Filter.Eq(p => p.UserId, player.UserId);
        _players.ReplaceOne(filter, player, options);
        MainProgram.WriteColoredMessage($"Profile '{player.Name}' updated !", success);
    }

    /* ----- ENEMIES ----- */
    public Enemy? GetEnemyById(ObjectId enemyId) {                              // Load enemy
        return _enemies.Find(e => e.id == enemyId).FirstOrDefault();
    }

    public void SaveEnemy(Enemy enemy) {
        if (_enemies == null) return;
        var filter = Builders<Enemy>.Filter.Eq(p => p.id, enemy.id);
        _enemies.ReplaceOne(filter, enemy, options);
    }

    public void DeleteEnemy(ObjectId enemyId) {
        if (_enemies == null) return;
        var filter = Builders<Enemy>.Filter.Eq(e => e.id, enemyId);
        _enemies.DeleteOne(filter);
    }
}
