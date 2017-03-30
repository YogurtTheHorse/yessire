using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YesSir.Backend.Entities.Kingdoms;
using YesSir.Backend.Managers;

namespace YesSir.Backend.Commands.Parts {
	public static class CommandsStandartFunctions {
		public static readonly Func<string, Tuple<bool, int>> CheckInt = (s) => {
			string f = s.Split(' ')[0];

			int test;
			if (int.TryParse(f, out test)) {
				return new Tuple<bool, int>(true, f.Length);
			} else {
				return new Tuple<bool, int>(false, 0);
			}
		};

		public static readonly CommandPart Fill = new CommandPart("_", (s) => new Tuple<bool, int>(true, s.Length), (s) => null);

		public static object ParseInt(string arg) {
			return int.Parse(arg);
		}
		
		public static Tuple<bool, int> CheckKingdom(string s) {
			foreach (Tuple<string, Guid> k in KingdomsManager.GetKingdomsNames()) {
				if (s.Contains(k.Item1.ToLower())) {
					return new Tuple<bool, int>(true, s.IndexOf(k.Item1.ToLower()) + k.Item1.Length);
				}
			}

			return new Tuple<bool, int>(false, 0);
		}

		public static object ParseKingomd(string s) {
			foreach (Tuple<string, Guid> k in KingdomsManager.GetKingdomsNames()) {
				if (s.Contains(k.Item1.ToLower())) {
					return k.Item2.ToString();
				}
			}

			return null;
		}
	}
}
