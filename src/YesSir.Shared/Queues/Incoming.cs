using System;
using YesSir.Shared.Messages;
using YesSir.Shared.Users;

namespace YesSir.Shared.Queues {
	public class Incoming {
		public Guid Id;
		public UserInfo UserInfo;
		public string Method;
		public MessageInfo Message;
		public bool IsWaiting;
	}
}
