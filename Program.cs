// ===== TO DO LIST =====
// ... Check encrypt and decrypt functions -> play a bit
// E Password isn't recognized when loading local user
// E Message from Player() are displayed when loading online user -> Don't use CollectItem() function
// ! Do task in gameplay.cs -> Add profile menu + related options
// ! Save fight if player quit during combat -> add option to quit and resume fight
// ! Add rarity for items -> appears based on player's luck + auto-generated attributes
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
