using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Driver;
using YesSir.Backend.Entities;
using YesSir.Backend.Entities.Items;
using YesSir.Backend.Entities.Kingdoms;
using YesSir.Shared.Messages;
using YesSir.Shared.Queues;
using YesSir.Shared.Users;

namespace YesSir.Backend.Managers {
	public static class DatabaseManager {
		private static IMongoDatabase Database;

		public static IMongoCollection<UserInfo> Users;
		public static IMongoCollection<Kingdom> Kingdoms;
		public static IMongoCollection<Incoming> IncomingQueue;
		public static IMongoCollection<Outgoing> OutgoingQueue;

		public static  void Init() {
			MongoClient client = new MongoClient();

			Database = client.GetDatabase("yes_sir");
			BsonClassMap.RegisterClassMap<UserInfo>(cm => {
				cm.AutoMap();
				cm.MapMember(c => c.Id).SetElementName("_id").SetIdGenerator(CombGuidGenerator.Instance);
			});
			BsonClassMap.RegisterClassMap<MessageCallback>(cm => {
				cm.AutoMap();
				cm.SetIgnoreExtraElements(true);
			});
			BsonClassMap.RegisterClassMap<MessageInfo>(cm => {
				cm.AutoMap();
				cm.SetIgnoreExtraElements(true);
			});
			BsonClassMap.RegisterClassMap<Point>(cm => {
				cm.AutoMap();
				cm.SetDiscriminator("point");
				cm.SetIgnoreExtraElements(true);
			});
			BsonClassMap.RegisterClassMap<Kingdom>(cm => {
				cm.AutoMap();
				cm.SetDiscriminator("kingdom");
				cm.SetIgnoreExtraElements(true);
			});
			BsonClassMap.RegisterClassMap<Human>(cm => {
				cm.AutoMap();
				cm.SetDiscriminator("human");
				cm.MapMember(c => c.HumanId).SetElementName("_id").SetIdGenerator(CombGuidGenerator.Instance);
				cm.SetIgnoreExtraElements(true);
			});
			BsonClassMap.RegisterClassMap<Field>(cm => {
				cm.AutoMap();
				cm.SetDiscriminator("field");
				cm.UnmapMember(c => c.IsBusy);
				cm.SetIgnoreExtraElements(true);
			});
			BsonClassMap.RegisterClassMap<Building>(cm => {
				cm.AutoMap();
				cm.SetDiscriminator("building");
				cm.UnmapMember(c => c.IsBusy);
				cm.MapMember(c => c.Id).SetElementName("_id").SetIdGenerator(CombGuidGenerator.Instance);
				cm.SetIgnoreExtraElements(true);
			});
			BsonClassMap.RegisterClassMap<Item>(cm => {
				cm.AutoMap();
				cm.SetDiscriminator("item");
				cm.SetIgnoreExtraElements(true);
			});
			BsonClassMap.RegisterClassMap<Incoming>(cm => {
				cm.AutoMap();
				cm.MapMember(c => c.Id).SetElementName("_id").SetIdGenerator(CombGuidGenerator.Instance);
			});
			BsonClassMap.RegisterClassMap<Outgoing>(cm => {
				cm.AutoMap();
				cm.MapMember(c => c.Id).SetElementName("_id").SetIdGenerator(CombGuidGenerator.Instance);
			});

			Users = Database.GetCollection<UserInfo>("users");
			Kingdoms = Database.GetCollection<Kingdom>("kingdoms");
			IncomingQueue = Database.GetCollection<Incoming>("incoming");
			OutgoingQueue = Database.GetCollection<Outgoing>("outgoing");
		}
	}
}
