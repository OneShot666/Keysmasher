using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace Gameplay;
public class User {
    [BsonId]
    public ObjectId id { get; set; } = ObjectId.GenerateNewId();
    public string Username { get; set; }
    public string PasswordHash { get; set; }
    public string Salt { get; set; }

    public User() {                                                             // For local saves
        Username = "";
        PasswordHash = "";
        Salt = "";
    }

    public User(string name, string password, string salt) {
        Username = name;
        PasswordHash = password;
        Salt = salt;
    }
}
