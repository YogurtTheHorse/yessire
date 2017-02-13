using System;
using System.Collections.Generic;
using YesSir.Backend.Commands.Dependencies;
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

		public Tuple<bool, MessageCallback> CheckAndExecute(string s, Kingdom kingdom) {
			Dictionary<string, object> parsed = null;
			
			if (Parser.ParseCommand(s, ref parsed)) {
				foreach (IDependency dep in Dependencies) {
					Tuple<bool, MessageCallback> res = dep.CheckKingdom(kingdom);
					if (!res.Item1) {
						return new Tuple<bool, MessageCallback>(true, res.Item2);
					}
				}
				return new Tuple<bool, MessageCallback>(true, Execute(kingdom, parsed));
			} else {
				return new Tuple<bool, MessageCallback>(false, new MessageCallback());
			}
		}
	}
}
