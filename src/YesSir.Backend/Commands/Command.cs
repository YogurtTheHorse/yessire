using System;
using System.Collections.Generic;
using YesSir.Backend.Entities.Dependencies;
using YesSir.Backend.Commands.Parts;
using YesSir.Backend.Entities.Kingdoms;
using YesSir.Shared.Messages;

namespace YesSir.Backend.Commands {
	public class Command {
		private IDependency[] Dependencies;
		private Func<Kingdom, Dictionary<string, object>, MessageCallback> Execute;
		private CommandPart Parser;

		public Command(IDependency[] dependecies, CommandPart parser, Func<Kingdom, Dictionary<string, object>, MessageCallback> func) {
			this.Dependencies = dependecies;
			this.Parser = parser;
			this.Execute = func;
		}

		public Tuple<int, MessageCallback> CheckAndExecute(string s, Kingdom kingdom) {
			Dictionary<string, object> parsed = null;
			int parsed_len = Parser.ParseCommand(s, ref parsed);
			if (parsed_len >= 0) {
				foreach (IDependency dep in Dependencies) {
					Tuple<bool, MessageCallback> res = dep.CheckKingdom(kingdom);
					if (!res.Item1) {
						return new Tuple<int, MessageCallback>(0, res.Item2);
					}
				}
				return new Tuple<int, MessageCallback>(parsed_len, Execute(kingdom, parsed));
			} else {
				return new Tuple<int, MessageCallback>(-1, new MessageCallback());
			}
		}
	}
}
