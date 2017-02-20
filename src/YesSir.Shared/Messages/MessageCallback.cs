using System;

namespace YesSir.Shared.Messages {
	public class MessageCallback {
		public string Text;
		public ECharacter From;

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