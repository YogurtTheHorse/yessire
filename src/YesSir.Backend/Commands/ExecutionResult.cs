using YesSir.Shared.Messages;

namespace YesSir.Backend.Commands {
	public struct ExecutionResult {
		public bool Successful,
					Applied;

		public int CommandLength;
		public MessageCallback Message;

		public ExecutionResult(bool succesful, MessageCallback msg) {
			Applied = true;
			Successful = succesful;
			CommandLength = 0;
			Message = msg;
		}

		public ExecutionResult(MessageCallback msg) : this (true, msg) { }
	}
}
