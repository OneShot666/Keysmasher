using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace Core;
public class User {
    [BsonId]
    public ObjectId id { get; set; }
    public string Username { get; set; }
    public string PasswordHash { get; set; }
    public string Salt { get; set; }

    public override string ToString() {
        return $"User(Username='{Username}')";
    }

    public User() {                                                             // For local saves
        id = ObjectId.GenerateNewId();
        Username = "";
        PasswordHash = "";
        Salt = "";
    }

    public User(string name, string password, string salt) {
        id = ObjectId.GenerateNewId();
        Username = name;
        PasswordHash = password;
        Salt = salt;
    }
}
