using System;
using System.Collections.Generic;
using YesSir.Backend.Entities.Kingdoms;
using YesSir.Shared.Messages;

namespace YesSir.Backend.Commands {
	public class ExecutionResult {
		public bool Successful,
					Applied;

		public int CommandLength;
		public string Text;
		public MessageCallback Message;
		public Dictionary<string, object> Parsed;
		public Func<Kingdom, Dictionary<string, object>, ExecutionResult> ExecuteFunc;
		public bool HumanBusy = false;

		public ExecutionResult() { }

		public ExecutionResult(bool succesful, MessageCallback msg) {
			Applied = true;
			Successful = succesful;
			CommandLength = 0;
			Message = msg;
			Parsed = new Dictionary<string, object>();
			ExecuteFunc = (k, p) => new ExecutionResult();
			Text = "";
		}

		public ExecutionResult(MessageCallback msg) : this(true, msg) { }

		public ExecutionResult Execute(Kingdom k) {
			ExecutionResult res = this.ExecuteFunc(k, Parsed);
			res.Applied = Applied;
			res.CommandLength = CommandLength;

			return res;
		}
	}
}
