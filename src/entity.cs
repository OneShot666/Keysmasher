public abstract class Entity {
    public string name { get; set; } = "Entity";
    public int level { get; set; } = 1;
    public int hp { get; set; } = 100;
    public int maxHp { get; set; } = 100;
    public int atk { get; set; } = 5;
    public int def { get; set; } = 1;

    public override string ToString() {                                         // When display instances
        return $"Entity(Name='{name}', Level={level}, HP={hp}, Max HP={maxHp}, " +
            $"Attack={atk}, Defense={def})";
    }

    public virtual void Present() {
        Console.WriteLine($"Name      : {name}");
        Console.WriteLine($"Level     : {level}");
        Console.WriteLine($"HP        : {hp}/{maxHp}");
        Console.WriteLine($"Attack    : {atk}");
        Console.WriteLine($"Defense   : {def}");
    }

    public virtual int GetRandomDamage(int bonus = 0) {
        var rnd = new Random();
        return Math.Max(0, atk) + rnd.Next(0, bonus);
    }

    public virtual void TakeDamage(int brut_damage) {
        int real_damage = brut_damage - def;
        hp -= real_damage;
        Console.WriteLine($"'{name}' took {real_damage} damage.");
    }
}
