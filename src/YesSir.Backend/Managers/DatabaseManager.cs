using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.IdGenerators;
using MongoDB.Driver;
using System.Collections;
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
		public static IMongoCollection<Human> HumansOnJourney;
		private static IMongoCollection<Variable> Variables;

		private class Variable {
			public ObjectId Id;
			public string Name;
			public object Value;
		}

		public static void Init() {
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
			BsonClassMap.RegisterClassMap<Variable>(cm => {
				cm.AutoMap();
				cm.SetDiscriminator("variable");
				cm.MapMember(c => c.Id).SetElementName("_id").SetIdGenerator(CombGuidGenerator.Instance);
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
			HumansOnJourney = Database.GetCollection<Human>("humans");
			IncomingQueue = Database.GetCollection<Incoming>("incoming");
			OutgoingQueue = Database.GetCollection<Outgoing>("outgoing");
			Variables = Database.GetCollection<Variable>("vars");
		}

		public static object GetVariable(string name) {
			return GetVariable(name);
		}

		public static T GetVariable<T>(string name, T def = null) where T : class {
			var res = Variables.Find((d) => d.Name == name);
			return res.Count() > 0 
					? (res.Single()?.Value as T) ?? def
					: def;
		}

		public static void SetVariable(string name, object val) {
			var opts = new FindOneAndUpdateOptions<Variable, Variable>() { IsUpsert = true };
			var fnd = Builders<Variable>.Filter.Eq(v => v.Name, name);

			Variables.FindOneAndUpdateAsync(fnd, Builders<Variable>.Update.Set(v => v.Value, val), opts);
		}
	}
}
