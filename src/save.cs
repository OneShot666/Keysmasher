using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace Gameplay;
public class Save {
    [BsonId]
    public ObjectId id { get; set; } = ObjectId.GenerateNewId();

    [BsonRepresentation(BsonType.ObjectId)]
    public ObjectId PlayerId { get; set; }

    [BsonRepresentation(BsonType.ObjectId)]
    public ObjectId? EnemyId { get; set; }                                      // Optional (if fighting)
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;                  // Time of save

    public Save(ObjectId playerId) {
        PlayerId = playerId;
    }

    public Save(ObjectId playerId, ObjectId? enemyId) {
        PlayerId = playerId;
        EnemyId = enemyId;
    }
}
