using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;

namespace YesSir.Shared.Messages {
	public class MessageCallback {
		[JsonConverter(typeof(StringEnumConverter))]
		public ECharacter From;
		public string Text;

		public MessageCallback(string text) : this(text, ECharacter.Knight) {  }

		public MessageCallback(string text, ECharacter from) : this() {
			this.Text = text;
			this.From = from;
		}

		public MessageCallback() { }

		public string Format() {
			string res = From.ToString() + ":\n";
			foreach (string s in Text.Split('\n')) {
				res += " - " + s + "\n";
			}
			return res;
		}
	}

	public enum ECharacter {
		King = 0,
		Knight = 1,
		Admin = 2,
		Farmer = 3
	}
}