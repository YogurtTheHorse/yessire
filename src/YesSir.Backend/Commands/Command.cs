using System;
using System.Collections.Generic;
using YesSir.Backend.Entities.Dependencies;
using YesSir.Backend.Commands.Parts;
using YesSir.Backend.Entities.Kingdoms;
using YesSir.Shared.Messages;

namespace YesSir.Backend.Commands {
	public class Command {
		private IDependency[] Dependencies;
		private Func<Kingdom, Dictionary<string, object>, ExecutionResult> ExecuteFunc;
		private CommandPart Parser;

		public Command(IDependency[] dependecies, CommandPart parser, Func<Kingdom, Dictionary<string, object>, ExecutionResult> func) {
			this.Dependencies = dependecies;
			this.Parser = parser;
			this.ExecuteFunc = func;
		}

		public ExecutionResult Check(string s, Kingdom kingdom) {
			Dictionary<string, object> parsed = null;
			int parsed_len = Parser.ParseCommand(s, ref parsed);
			if (parsed_len >= 0) {
				foreach (IDependency dep in Dependencies) {
					Tuple<bool, MessageCallback> res = dep.CheckKingdom(kingdom);
					if (!res.Item1) {
						return new ExecutionResult() {
							Successful = false,
							Applied = true,
							CommandLength = parsed_len,
							Message = res.Item2
						};
					}
				}
				return new ExecutionResult() {
					Applied = true,
					Successful = true,
					CommandLength = parsed_len,
					Parsed = parsed,
					ExecuteFunc = ExecuteFunc
				};	
			} else {
				return new ExecutionResult() {
					Successful = false,
					Applied = false,
					CommandLength = 0,
					Message = new MessageCallback()
				};
			}
		}
	}
}
