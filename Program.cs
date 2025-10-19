// ===== TO DO LIST =====
// ... Check save functions are asynchrone and db is updated correctly
// ! [server.cs] When clean db, also remove enemy with negative hp
// ! [gameplay.cs] Add 'Rest at the campfire' option to recover some HP between fights
// ! [gameplay.cs] Add profile menu + related options (equip/unequip items)
// ! Save fight if player quit during combat -> add option to quit and resume fight
// L Add 'Maps' table in db and locations (x, y) in Entity class to add visual -> Add entity.Move() function
// L Add rarity for items -> appears based on player's luck + auto-generated attributes
// L Upgrade API -> in KeySmasherAPI
// L Upgrade local website -> in KeySmasherWeb
// ? Add API and backend online server -> make Render work

// ===== COMMANDS =====
// - Command to run project :
// dotnet run
// - Command to create an executable :
// dotnet build
// - Command to transform project into standalone app :
// dotnet publish -c Release -r win-x64 --self-contained true /p:PublishSingleFile=true
namespace Gameplay;                                                             // Avoid ambiguity with api
public class MainProgram {                                                      // Manage server and saves
    public static void Main(string[] _) {
        new Game();
    }
}
