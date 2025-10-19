using MongoDB.Bson.Serialization;
using Items;

namespace Services;
public static class MongoMappingService {
    private static bool _isRegistered = false;

    public static void RegisterClassMaps() {
        if (_isRegistered) return;                                              // Avoid double savings
        _isRegistered = true;

        BsonClassMap.RegisterClassMap<Item>(cm => {                             // Base abstract class
            cm.AutoMap();
            cm.SetIsRootClass(true);
            cm.SetDiscriminator("Item");
        });

        BsonClassMap.RegisterClassMap<Shield>(cm => {                           // Derived concrete classes
            cm.AutoMap();
            cm.SetDiscriminator("Shield");
        });

        BsonClassMap.RegisterClassMap<Potion>(cm => {
            cm.AutoMap();
            cm.SetDiscriminator("Potion");
        });

        BsonClassMap.RegisterClassMap<Amulet>(cm => {
            cm.AutoMap();
            cm.SetDiscriminator("Amulet");
        });

        BsonClassMap.RegisterClassMap<Axe>(cm => {
            cm.AutoMap();
            cm.SetDiscriminator("Axe");
        });

        BsonClassMap.RegisterClassMap<Spear>(cm => {
            cm.AutoMap();
            cm.SetDiscriminator("Spear");
        });

        BsonClassMap.RegisterClassMap<Sword>(cm => {
            cm.AutoMap();
            cm.SetDiscriminator("Sword");
        });
    }
}
