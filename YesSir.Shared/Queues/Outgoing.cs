using System;
using YesSir.Shared.Messages;
using YesSir.Shared.Users;

namespace YesSir.Shared.Queues {
	public class Outgoing {
		public Guid Id;
		public UserInfo UserInfo;
		public MessageCallback Message;
	}
}
