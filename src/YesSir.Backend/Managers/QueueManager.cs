using MongoDB.Driver;
using System.Collections.Generic;
using YesSir.Shared.Queues;

namespace YesSir.Backend.Managers {
	public static class QueueManager {
		public static void Push(Incoming inc) {
			DatabaseManager.IncomingQueue.InsertOneAsync(inc);
		}
		
		public static void Push(Outgoing outg) {
			DatabaseManager.OutgoingQueue.InsertOneAsync(outg);
		}

		public static List<Outgoing> GetOutgoing(string userType, string userId="all") {
			FilterDefinition<Outgoing> filter = Builders<Outgoing>.Filter.Eq("UserInfo.Type", userType);
			if (userId != "all") {
				filter = filter & Builders<Outgoing>.Filter.Eq("UserInfo.ThirdPartyId", userId);
			}
			var cursor = DatabaseManager.OutgoingQueue.Find(filter);
			if (cursor.Count() > 0) {
				List<Outgoing> res = cursor.ToList();
				DatabaseManager.OutgoingQueue.DeleteMany(cursor.Filter);

				return res;
			} else {
				return new List<Outgoing>();
			}
		}

		public static Incoming GetNextIncoming() {
				return DatabaseManager.IncomingQueue.FindOneAndDelete(i => i.IsWaiting);
		}
	}
}
