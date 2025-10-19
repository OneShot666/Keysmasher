using MongoDB.Driver;
using MongoDB.Bson;
using Gameplay;
using Security;
using Items;
using Core;

namespace Services;
public class ServerService {
    public bool is_connected = false;                                           // If connected to database
    private string db_name = "KeySmasher";
    // private string db_name = "FakeDatabase";                                 // To test offline mode
    private readonly string save_folder = "Saves";
    private readonly string item_folder = Path.Combine("Data", "Items");
    private readonly IMongoDatabase? _database;
    private IMongoCollection<User>? _users { get; }
    private IMongoCollection<Save>? _saves { get; }
    private IMongoCollection<Player>? _players { get; }
    private IMongoCollection<Enemy>? _enemies { get; }
    private IMongoCollection<Item>? _items { get; }
    public List<Item>? _localItems { get; }                                     // L Add same thing for enemies
    private readonly ReplaceOptions options = new ReplaceOptions { IsUpsert = true };   // Insert if doesn't exists

    public ServerService() {                                                    // Try to connect at launch
        Directory.CreateDirectory(save_folder);                                 // Create directories if doesn't exists
        Directory.CreateDirectory(item_folder);

        Console.WriteLine("Connecting to database...");
        var collections = new List<string>();
        try {
            var client = new MongoClient("mongodb://localhost:27017");
            if (!DatabaseExists(client, db_name)) {                             // Try access collections
                Game.WriteColoredMessage("Connection to database failed !", Game.fail);
                return;
            }
            _database = client.GetDatabase(db_name);
            collections = _database.ListCollectionNames().ToList();
            is_connected = true;
            Game.WriteColoredMessage("Connection to database established !", Game.success);
        } catch (Exception e) {
            Game.WriteColoredMessage($"Error while connecting to server : {e.Message}", Game.fail);
        }

        CreateAllLocalItems();

        if (!is_connected || _database == null) return;                         // Offline mode (don't load db)
        if (!collections.Contains("Users")) _database.CreateCollection("Users");
        if (!collections.Contains("Saves")) _database.CreateCollection("Saves");
        if (!collections.Contains("Players")) _database.CreateCollection("Players");
        if (!collections.Contains("Enemies")) _database.CreateCollection("Enemies");
        if (!collections.Contains("Items")) _database.CreateCollection("Items");

        _users = _database.GetCollection<User>("Users");
        _saves = _database.GetCollection<Save>("Saves");
        _players = _database.GetCollection<Player>("Players");
        _enemies = _database.GetCollection<Enemy>("Enemies");
        _items = _database.GetCollection<Item>("Items");
    }

    public static bool DatabaseExists(IMongoClient client, string dbName) {     // Check it's the right database
        try {
            var databaseNames = client.ListDatabaseNames().ToList();
            return databaseNames.Contains(dbName);
        } catch { return false; }                                               // Unable to connect to server
    }

    /* ----- USERS ----- */
    public User? GetOnlineUserByUsername(string username) {                     // Load online user
        return is_connected ? _users.Find(u => u.Username == username).FirstOrDefault() : null;
    }

    public User? LoadLocalUser(string user_name) {
        if (user_name == null) return null;
        string path = Path.Combine(save_folder, $"{user_name}_user.json");
        if (!File.Exists(path)) return null;                                    // If local save doesn't exists
        string content = File.ReadAllText(path);
        return CryptoUtils.DecryptSave<User>(content);                          // Get user instance
    }

    public async Task SaveLocalUser(User? user) {
        if (user == null) return;                                               // Not connected
        string path = Path.Combine(save_folder, $"{user.Username}_user.json");
        string json = CryptoUtils.EncryptSave(user);
        await File.WriteAllTextAsync(path, json);
    }

    public async Task SaveOnlineUser(User user) {
        if (!is_connected || _users == null) return;
        var filter = Builders<User>.Filter.Eq(u => u.id, user.id);
        await _users.ReplaceOneAsync(filter, user, options);
    }

    /* ----- SAVES (or Game in function's name) ----- */
    public Save? GetOnlineGameByPlayerId(ObjectId playerId) {                   // Load save
        return is_connected ? _saves.Find(s => s.PlayerId == playerId).FirstOrDefault() : null;
    }

    public Save? LoadLocalSave(string user_name) {
        if (user_name == null) return null;
        string path = Path.Combine(save_folder, $"{user_name}_save.json");
        if (!File.Exists(path)) return null;                                    // If local save doesn't exists
        string content = File.ReadAllText(path);
        return CryptoUtils.DecryptSave<Save>(content);                          // Get save instance
    }

    public async Task SaveLocalGame(User? user, Save? save) {
        if (user == null || save == null) return;                               // Not connected
        string path = Path.Combine(save_folder, $"{user.Username}_save.json");
        string json = CryptoUtils.EncryptSave(save);
        await File.WriteAllTextAsync(path, json);
    }

    public async Task SaveOnlineGame(Save save) {
        if (!is_connected || _saves == null) return;
        var filter = Builders<Save>.Filter.Eq(p => p.PlayerId, save.PlayerId);
        Game.WriteColoredMessage("Save updated !", Game.success);               // !!!
        await _saves.ReplaceOneAsync(filter, save, options);
    }

    /* ----- LEADERBOARD ----- */
    public List<Player>? GetTopPlayers(int limit = 5) {                         // Get top 5 players by default
        if (!is_connected || _players == null) return null;
        return _players.Find(FilterDefinition<Player>.Empty)
            .SortByDescending(p => p.Score).Limit(limit).ToList();
    }

    /* ----- PLAYERS ----- */
    public bool IsPlayerSynced(Player localPlayer) {                            // Only compare important data
        if (!is_connected || _players == null) return false;
        var dbPlayer = _players.Find(p => p.Name == localPlayer.Name).FirstOrDefault();
        if (dbPlayer == null) return false;
        return dbPlayer.Score == localPlayer.Score && dbPlayer.Level == localPlayer.Level;
    }

    public async Task<List<Item>> GetPlayerItemsAsync(Player player) {
        var filter = Builders<Item>.Filter.In(i => i.id, player.Inventory._ItemsIDs);
        var items = await _items.Find(filter).ToListAsync();
        return items;
    }

    public Player? GetOnlinePlayerByName(string name) {                         // Load player
        return is_connected ? _players.Find(p => p.Name == name).FirstOrDefault() : null;
    }

    public Player? LoadLocalPlayer(string user_name) {
        if (user_name == null) return null;
        string path = Path.Combine(save_folder, $"{user_name}_player.json");
        if (!File.Exists(path)) return null;                                    // If local save doesn't exists
        string content = File.ReadAllText(path);
        return CryptoUtils.DecryptSave<Player>(content);                        // Get player instance
    }

    public async Task SaveLocalPlayer(User? user, Player? player) {
        if (user == null || player == null) return;                             // Not connected
        string path = Path.Combine(save_folder, $"{user.Username}_player.json");
        string json = CryptoUtils.EncryptSave(player);
        await File.WriteAllTextAsync(path, json);
    }

    public async Task SaveOnlinePlayer(Player player) {
        if (!is_connected || _players == null) return;
        var filter = Builders<Player>.Filter.Eq(p => p.UserId, player.UserId);
        await _players.ReplaceOneAsync(filter, player, options);
    }

    /* ----- ENEMIES ----- */
    public Enemy? GetOnlineEnemyById(ObjectId enemyId) {                        // Load enemy
        return is_connected ? _enemies.Find(e => e.id == enemyId).FirstOrDefault() : null;
    }

    public Enemy? LoadLocalEnemy(string user_name) {
        if (user_name == null) return null;
        string path = Path.Combine(save_folder, $"{user_name}_enemy.json");
        if (!File.Exists(path)) return null;                                    // If local save doesn't exists
        string content = File.ReadAllText(path);
        return CryptoUtils.DecryptSave<Enemy>(content);                         // Get enemy instance
    }

    public async Task SaveLocalEnemy(User? user, Enemy? enemy) {
        if (user == null || enemy == null) return;                              // Not connected
        string path = Path.Combine(save_folder, $"{user.Username}_enemy.json");
        string json = CryptoUtils.EncryptSave(enemy);
        await File.WriteAllTextAsync(path, json);
    }

    public async Task SaveOnlineEnemy(Enemy enemy) {
        if (!is_connected || _enemies == null) return;
        var filter = Builders<Enemy>.Filter.Eq(e => e.id, enemy.id);
        await _enemies.ReplaceOneAsync(filter, enemy, options);
    }

    public async Task CleanOrphanEnemiesAsync() {
        if (_enemies == null) return;
        try {
            var usedEnemyIds = await _saves.Find(save => save.EnemyId != ObjectId.Empty)
                .Project(save => save.EnemyId).ToListAsync();                   // Get all enemies in saves
            var filter = Builders<Enemy>.Filter.Nin(e => e.id, usedEnemyIds);   // 'Nin': Not in
            var result = await _enemies.DeleteManyAsync(filter);                // Delete all at once
            Game.WriteColoredMessage($"Database cleaned !", Game.success);
        } catch (Exception ex) {
            Game.WriteColoredMessage($"Error while cleaning database : {ex.Message}", Game.fail);
        }
    }

    /* ----- ITEMS ----- */
    public Item? GetOnlineItemById(ObjectId itemId) {                          // Load item
        return is_connected ? _items.Find(i => i.id == itemId).FirstOrDefault() : null;
    }

    public void CreateAllLocalItems() {
        foreach (Item item in ItemStorageService.AllItems) SaveLocalItem(item);        // Save all items locally
    }

    public Item? LoadLocalItem(string item_name) {
        if (item_name == null) return null;
        string path = Path.Combine(item_folder, $"{item_name}.json");
        if (!File.Exists(path)) return null;                                    // If local save doesn't exists
        string content = File.ReadAllText(path);
        return CryptoUtils.DecryptSave<Item>(content);                          // Get item instance
    }

    public void SaveLocalItem(Item? item) {
        if (item == null) return;
        string path = Path.Combine(item_folder, $"{item.Name}.json");
        if (File.Exists(path)) return;                                          // Don't overwrite existing items
        string json = CryptoUtils.EncryptSave(item);
        File.WriteAllText(path, json);
    }

    public void SaveOnlineItem(Item item) {
        if (!is_connected || _items == null) return;
        var filter = Builders<Item>.Filter.Eq(i => i.id, item.id);
        _items.ReplaceOne(filter, item, options);
    }
}
