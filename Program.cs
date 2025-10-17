// ===== TO DO LIST =====
// ... Check encrypt and decrypt functions
// . Doing tasks in player.cs (items update)
// ! Add 'Items' table in database + functions
// ! Add rarity for items -> appears based on player's luck + auto-generated attributes
// ! Save fight if player quit during combat -> add option to quit and resume fight
// L Upgrade API -> in KeySmasherAPI
// L Upgrade local website -> in KeySmasherWeb
// ? Add API and backend online server -> make Render work

// ===== COMMANDS =====
// - Command run project :
// dotnet run
// - Command to transform project into app :
// dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true
namespace Gameplay;                                                             // Avoid ambiguity with api
public class MainProgram {                                                      // Manage server and saves
    public void Main(string[] _) {
        Game game = new Game();
        game.Main();
    }
}
