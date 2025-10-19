using MongoDB.Bson;
using Services;
using Items;
using Core;

// . Add option on 'Show profile' choice to equip/unequip items
// ? Make StartCombat a [Services] class ?
namespace Gameplay;
public class Gameplay {                                                         // Manage player
    public bool running = false;
    public Save? save;
    public Player? player;
    public string player_name = "";
    public Enemy? enemy;

    public void StartGame(Game program, Gameplay gameplay) {
        if (player == null) return;

        bool running = true;
        while (running) {
            Console.WriteLine("\n===== ACTIONS =====");
            Console.WriteLine("1 - Start a fight");
            Console.WriteLine("2 - Heal" + (player.hasPotion ? "" : " (potion required)"));
            Console.WriteLine("3 - Open inventory");
            Console.WriteLine("4 - Show profile");
            Console.WriteLine("5 - Save");
            Console.WriteLine("6 - Back to Menu");
            int choice = AskIntChoice();

            switch (choice) {
                case 1: StartCombat(gameplay.save); break;
                case 2: player.UsePotion(); break;
                case 3: player.ShowInventory(); break;
                case 4: player.Present(); break;
                case 5: program.Save(); break;
                case 6: running = false; break;
                default:
                    Game.WriteColoredMessage("Incorrect choice !", Game.fail);
                    break;
            }
            Console.ReadKey();
        }
    }

    public async Task SetPlayerItemsAsync(ServerService server) {
        if (player == null) return;
        var items = await server.GetPlayerItemsAsync(player);
        player.Inventory.SetItems(items);
    }

    public static int AskIntChoice(string question = "\nChoice : ") {           // Ask until user choose an integer
        int choice;
        string? input;
        do {
            Console.Write(question);
            input = Console.ReadLine();
        } while (!int.TryParse(input, out choice));                             // Check integer entered
        return choice;
    }

    public void StartCombat(Save? save) {
        if (player == null) return;
        enemy = Enemy.GenerateByLevel(player.Level);
        if (save != null) save.EnemyId = enemy.id;                              // Add enemy id in save
        Console.WriteLine($"\nA '{enemy.Name}' appears ! (HP {enemy.Hp})");

        while (enemy.Hp > 0 && player.Hp > 0) {
            Thread.Sleep(1000);                                                 // Wait a second
            Console.WriteLine($"\n{player.Name} HP: {player.Hp}/{player.MaxHp}" +
                $" | {enemy.Name} HP: {enemy.Hp}/{enemy.MaxHp}");
            Console.WriteLine("1 - Attack");
            Console.WriteLine("2 - Defend");
            Console.WriteLine("3 - Heal");
            Console.WriteLine("4 - Flee");
            int action = AskIntChoice("Action : ");
            Console.WriteLine();

            switch (action) {
                case 1: PlayerAttack(); break;
                case 2: player.UseDefensivePosition(); break;
                case 3: player.UsePotion(); break;
                case 4: PlayerFlee(); break;
                default:
                    Game.WriteColoredMessage("Incorrect action !", Game.fail);
                    continue;
            }

            if (enemy.Hp <= 0) {                                                // Check enemy still alive
                Game.WriteColoredMessage($"You defeated '{enemy.Name}' !", Game.success);
                if (save != null) save.EnemyId = ObjectId.Empty;                // Remove enemy id if defeated
                SpawnLoot();
                player.LootEnemy(enemy);
                return;
            }

            // Enemy turn
            int enemyDmg = enemy.GetRandomDamage(3);
            Console.WriteLine($"{enemy.Name} attack and inflict {enemyDmg} damage.");
            player.TakeDamage(enemyDmg);
            if (player.Hp <= 0 && save != null) save.EnemyId = ObjectId.Empty;  // Remove enemy id if player died
        }
    }

    public void PlayerAttack() {
        if (player == null || enemy == null) return;
        int dmg = player.GetRandomDamage(4);
        Console.WriteLine($"You attack and inflict {dmg} damage.");
        enemy.TakeDamage(dmg);
    }

    public void PlayerFlee() {
        if (player == null || enemy == null) return;
        var rnd = new Random();
        if (rnd.NextDouble() < 0.5) {                                           // fifty-fifty chance to flee
            Game.WriteColoredMessage("You escaped the combat !", Game.success);
            enemy = null;
        } else Game.WriteColoredMessage("The escape failed!", Game.fail);
    }

    public void SpawnLoot() {                                                   // L Move in Player.SpawnLoot()
        if (player == null) return;
        var rnd = new Random();

        double potionChance = 0.25;                                             // 25 % chance to drop a potion
        if (rnd.NextDouble() < potionChance + player.GetPercentLuck()){
            Potion potion = (Potion)ItemStorageService.GetItemsByType<Potion>()![0];
            player.CollectItem(potion);
            Console.WriteLine($"You find a '{potion.Name}' on the enemy !");
        }

        double amuletChance = 0.1;                                              // 10 % chance to drop an amulet
        if (rnd.NextDouble() < amuletChance + player.GetPercentLuck()) {
            Amulet amulet = (Amulet)ItemStorageService.GetItemsByType<Amulet>()![0];
            player.CollectItem(amulet);
            Console.WriteLine($"You find a '{amulet.Name}' on the enemy !");
        }
    }
}
