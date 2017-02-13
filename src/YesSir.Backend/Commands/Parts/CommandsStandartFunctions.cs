using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

		public static object ParseInt(string arg) {
			return int.Parse(arg);
		}
	}
}
