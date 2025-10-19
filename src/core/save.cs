using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace Core;
public class Save {
    [BsonId]
    public ObjectId id { get; set; }

    [BsonRepresentation(BsonType.ObjectId)]
    public ObjectId PlayerId { get; set; }

    [BsonRepresentation(BsonType.ObjectId)]
    public ObjectId? EnemyId { get; set; }                                      // Optional (if fighting)
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;                  // Time of save

    public override string ToString() {
        return $"Save(PlayerId='{PlayerId}', EnemyId='{EnemyId}', Timestamp='{Timestamp}')";
    }

    public Save() { id = ObjectId.GenerateNewId(); }                            // For local saves

    public Save(ObjectId playerId) {
        id = ObjectId.GenerateNewId();
        PlayerId = playerId;
    }

    public Save(ObjectId playerId, ObjectId? enemyId) {
        id = ObjectId.GenerateNewId();
        PlayerId = playerId;
        EnemyId = enemyId;
    }
}
