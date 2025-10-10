using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace Gameplay;
public class Player {
    [BsonId]                                                                    // Mongo primary key type
    public ObjectId id { get; set; } = ObjectId.GenerateNewId();
    public string Nom { get; set; } = "";
    public int Niveau { get; set; }
    public int Score { get; set; }
    private int maxXP = 100;

    public string? PasswordHash { get; set; } = "";
    public string? Salt { get; set; } = "";

    public override string ToString() {                                         // When display instances
        return $"Player(Nom='{Nom}', Niveau={Niveau}, Score={Score})";
    }

    public void Present() {
        Console.WriteLine($"\nVotre profile : ");
        Console.WriteLine($"Nom : {Nom}");
        Console.WriteLine($"Niveau : lvl {Niveau}");
        Console.WriteLine($"Score : {Score} pts");
    }

    public void CheckLevel() {
        if (Score % maxXP == 0) Niveau = (Score / maxXP) + 1;
    }
}
