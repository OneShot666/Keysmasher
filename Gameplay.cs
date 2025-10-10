using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.IO;

namespace Keysmasher
{
    internal class Gameplay
    {
class Program
    {
        static void Gameplay()
        {
            Console.Title = "Mini Console RPG";

            Player player = LoadOrCreatePlayer();
            GameLoop(player);
        }

        static Player LoadOrCreatePlayer()
        {
            const string saveFile = "playersave.json";
            if (File.Exists(saveFile))
            {
                try
                {
                    var json = File.ReadAllText(saveFile);
                    var loaded = JsonSerializer.Deserialize<Player>(json);
                    if (loaded != null)
                    {
                        Console.WriteLine($"Chargé : {loaded.Name} (niveau {loaded.Level}) depuis {saveFile}");
                        return loaded;
                    }
                }
                catch
                {
                    // ignore and create new
                }
            }

            // Création d'un joueur par défaut
            var p = new Player
            {
                Name = "Héros",
                Level = 1,
                MaxHp = 100,
                Hp = 100,
                Atk = 12,
                Def = 5,
                Gold = 50,
                Inventory = new List<string> { "Potion" }
            };

            Console.WriteLine("Nouveau joueur créé : Héros");
            return p;
        }

        static void GameLoop(Player player)
        {
            bool running = true;
            while (running)
            {
                Console.WriteLine("\n----Actions-----");
                Console.WriteLine("1 Attack");
                Console.WriteLine("2 Defend");
                Console.WriteLine("3 Heal");
                Console.WriteLine("4 Inventory");
                Console.WriteLine("5 Back to Menu");
                Console.WriteLine("6 Profile");
                Console.WriteLine("7 Save");
                Console.WriteLine("8 Quit");
                Console.Write("Select: ");

                if (!int.TryParse(Console.ReadLine(), out int GameMenu))
                {
                    Console.WriteLine("Entrée invalide.");
                    continue;
                }

                switch (GameMenu)
                {
                    case 1:
                        StartCombat(player);
                        break;
                    case 2:
                        Console.WriteLine("Tu prends une posture défensive. (Utilité en combat uniquement)");
                        // we could toggle a defend flag for next combat round — for simplicity keep informative
                        break;
                    case 3:
                        UseHeal(player);
                        break;
                    case 4:
                        ShowInventory(player);
                        break;
                    case 5:
                        Console.WriteLine("Retour au menu principal (déjà ici).");
                        break;
                    case 6:
                        ShowProfile(player);
                        break;
                    case 7:
                        SavePlayer(player);
                        break;
                    case 8:
                        Console.WriteLine("Goodbye");
                        running = false;
                        break;
                    default:
                        Console.WriteLine("Choix inconnu.");
                        break;
                }
            }
        }

        static void StartCombat(Player player)
        {
            var enemy = Enemy.GenerateForLevel(player.Level);
            Console.WriteLine($"\nUn {enemy.Name} apparaît ! (HP {enemy.Hp})");

            bool playerDefending = false;
            var rnd = new Random();

            while (enemy.Hp > 0 && player.Hp > 0)
            {
                Console.WriteLine($"\nTon HP: {player.Hp}/{player.MaxHp} | Ennemi HP: {enemy.Hp}");
                Console.WriteLine("1 Attack  2 Defend  3 Heal  4 Flee");
                Console.Write("Action: ");
                var input = Console.ReadLine();

                switch (input)
                {
                    case "1":
                        int dmg = Math.Max(0, player.Atk - enemy.Def);
                        // add small variance
                        dmg += rnd.Next(0, 4);
                        enemy.Hp -= dmg;
                        Console.WriteLine($"Tu attaques et infliges {dmg} dégâts.");
                        break;
                    case "2":
                        playerDefending = true;
                        Console.WriteLine("Tu défends (moins de dégâts reçus au prochain tour).");
                        break;
                    case "3":
                        if (player.Inventory.Contains("Potion"))
                        {
                            HealWithPotion(player);
                        }
                        else
                        {
                            Console.WriteLine("Tu n'as pas de potion !");
                        }
                        break;
                    case "4":
                        if (rnd.NextDouble() < 0.5)
                        {
                            Console.WriteLine("Tu prends la fuite ! (Réussi)");
                            return;
                        }
                        else
                        {
                            Console.WriteLine("La fuite a échoué !");
                        }
                        break;
                    default:
                        Console.WriteLine("Action non reconnue.");
                        break;
                }

                if (enemy.Hp <= 0)
                {
                    Console.WriteLine($"Tu as vaincu le {enemy.Name} !");
                    int goldGain = enemy.Level * 5;
                    player.Gold += goldGain;
                    Console.WriteLine($"Tu récupères {goldGain} pièces d'or. Total or: {player.Gold}");
                    MaybeLoot(player);
                    // give xp and maybe level up (simple)
                    player.GainXp(20);
                    return;
                }

                // Enemy turn
                int enemyDmg = Math.Max(0, enemy.Atk - (playerDefending ? player.Def * 2 : player.Def));
                // minor variance
                enemyDmg += rnd.Next(0, 3);
                player.Hp -= enemyDmg;
                Console.WriteLine($"{enemy.Name} attaque et inflige {enemyDmg} dégâts.");

                if (player.Hp <= 0)
                {
                    Console.WriteLine("Tu es mort... Respawn au village avec la moitié des PV.");
                    player.Hp = Math.Max(1, player.MaxHp / 2);
                    player.Gold = Math.Max(0, player.Gold - 10);
                    return;
                }

                // reset defend
                playerDefending = false;
            }
        }

        static void MaybeLoot(Player player)
        {
            var rnd = new Random();
            if (rnd.NextDouble() < 0.4)
            {
                Console.WriteLine("Tu trouves une Potion sur l'ennemi !");
                player.Inventory.Add("Potion");
            }
        }

        static void UseHeal(Player player)
        {
            if (player.Inventory.Contains("Potion"))
            {
                HealWithPotion(player);
            }
            else
            {
                Console.WriteLine("Pas de potion disponible.");
            }
        }

        static void HealWithPotion(Player player)
        {
            const int healAmount = 50;
            player.Inventory.Remove("Potion");
            player.Hp = Math.Min(player.MaxHp, player.Hp + healAmount);
            Console.WriteLine($"Tu utilises une Potion et récupères {healAmount} HP. HP: {player.Hp}/{player.MaxHp}");
        }

        static void ShowInventory(Player player)
        {
            Console.WriteLine("\n-- Inventaire --");
            if (player.Inventory.Count == 0)
            {
                Console.WriteLine("Vide");
                return;
            }

            var counts = new Dictionary<string, int>();
            foreach (var it in player.Inventory)
            {
                if (!counts.ContainsKey(it)) counts[it] = 0;
                counts[it]++;
            }

            foreach (var kv in counts)
            {
                Console.WriteLine($"{kv.Key} x{kv.Value}");
            }
        }

        static void ShowProfile(Player player)
        {
            Console.WriteLine("\n-- Profile --");
            Console.WriteLine($"Nom: {player.Name}");
            Console.WriteLine($"Niveau: {player.Level} (XP: {player.Xp}/{player.XpToLevel})");
            Console.WriteLine($"HP: {player.Hp}/{player.MaxHp}");
            Console.WriteLine($"ATK: {player.Atk}");
            Console.WriteLine($"DEF: {player.Def}");
            Console.WriteLine($"Or: {player.Gold}");
            Console.WriteLine($"Inventaire: {string.Join(", ", player.Inventory)}");
        }

        static void SavePlayer(Player player)
        {
            const string saveFile = "playersave.json";
            try
            {
                var json = JsonSerializer.Serialize(player, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(saveFile, json);
                Console.WriteLine($"Sauvegarde dans {saveFile}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erreur sauvegarde: {ex.Message}");
            }
        }
    }

    public class Player
    {
        public string Name { get; set; } = "Heros";
        public int Level { get; set; } = 1;
        public int Xp { get; set; } = 0;
        public int XpToLevel { get; set; } = 100;
        public int MaxHp { get; set; } = 100;
        public int Hp { get; set; } = 100;
        public int Atk { get; set; } = 12;
        public int Def { get; set; } = 5;
        public int Gold { get; set; } = 0;
        public List<string> Inventory { get; set; } = new List<string>();

        public void GainXp(int amount)
        {
            Xp += amount;
            Console.WriteLine($"Gagné {amount} XP !");
            while (Xp >= XpToLevel)
            {
                Xp -= XpToLevel;
                Level++;
                MaxHp += 10;
                Atk += 2;
                Def += 1;
                Hp = MaxHp;
                XpToLevel = (int)(XpToLevel * 1.2);
                Console.WriteLine($"Niveau supérieur ! Nouveau niveau: {Level}. PV restaurés.");
            }
        }
    }

    public class Enemy
    {
        public string Name { get; set; } = "Gobelin";
        public int Level { get; set; } = 1;
        public int Hp { get; set; } = 30;
        public int Atk { get; set; } = 8;
        public int Def { get; set; } = 2;

        public static Enemy GenerateForLevel(int playerLevel)
        {
            var rnd = new Random();
            int lvl = Math.Max(1, playerLevel + rnd.Next(-1, 2)); // playerLevel-1..playerLevel+1
            lvl = Math.Max(1, lvl);
            return new Enemy
            {
                Level = lvl,
                Name = lvl <= 1 ? "Gobelin" : (lvl == 2 ? "Bandit" : "Orc"),
                Hp = 20 + lvl * 15,
                Atk = 6 + lvl * 3,
                Def = 1 + lvl
            };
        }
    }

}
}
