// ===== TO DO LIST =====
// ... Check encrypt and decrypt functions
// ! Save fight if player quit during combat -> add option to quit and resume fight
// ! Do tasks in player.cs
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
