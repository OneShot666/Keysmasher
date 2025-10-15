using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

public abstract class Entity {
    [BsonId]                                                                    // Mongo primary key type
    public ObjectId id { get; set; } = ObjectId.GenerateNewId();
    public string Name { get; set; } = "Entity";
    public int Level { get; set; } = 1;
    public int Hp { get; set; } = 100;
    public int MaxHp { get; set; } = 100;
    public int Attack { get; set; } = 5;
    public int Defense { get; set; } = 1;

    public override string ToString() {                                         // When display instances
        return $"Entity(Name='{Name}', Level={Level}, HP={Hp}, Max HP={MaxHp}, " +
            $"Attack={Attack}, Defense={Defense})";
    }

    public virtual void Present() {
        Console.WriteLine($"Name      : {Name}");
        Console.WriteLine($"Level     : {Level}");
        Console.WriteLine($"HP        : {Hp}/{MaxHp}");
        Console.WriteLine($"Attack    : {Attack}");
        Console.WriteLine($"Defense   : {Defense}");
    }

    public virtual int GetRandomDamage(int bonus = 0) {
        var rnd = new Random();
        return Math.Max(0, Attack) + rnd.Next(0, bonus);
    }

    public virtual void TakeDamage(int brut_damage) {
        int real_damage = brut_damage - Defense;
        Hp -= real_damage;
        Console.WriteLine($"'{Name}' took {real_damage} damage.");
    }
}
