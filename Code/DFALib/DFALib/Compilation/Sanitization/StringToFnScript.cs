using System;
using FSMLib.Compilation.Traversal;

namespace FSMLib.Compilation.Sanitization {
  internal class StringToFnScript : Sanitizer {
    public StringToFnScript() {
      SimpleDFA<char>.Node        head = SanitizationDFA.Head;
      SimpleDFA<char>.Node  stringBody = new SimpleDFA<char>.Node(false);
      SimpleDFA<char>.Node escapeSlash = new SimpleDFA<char>.Node(false);
      SimpleDFA<char>.Node   stringEnd = new SimpleDFA<char>.Node(true);

      // [s] == "
      head.SetTransition('"', stringBody).OnTransition += (sender, args) => {
        Contents.Append(String.Format("[{0}] == \"", FSM<object>.CURRENT_ITEM_FNVARIABLE_NAME));
      };

      // Regex Body
      stringBody.SetTransition(CharUtils.AllCharsExcept(new char[] { '\\', '"' }), stringBody).OnTransition += (sender, args) => {
        Contents.Append(args.Element);
      };

      // Escape chars. Prevents an escaped double quotes from being recognised as the end of the string.
      stringBody.SetTransition('\\', escapeSlash);
      escapeSlash.SetTransition(CharUtils.AllChars(), stringBody).OnTransition += (sender, args) => {
        Contents.Append(String.Format("\\{0}", args.Element));
      };

      // "
      stringBody.SetTransition('"', stringEnd).OnTransition += (sender, args) => {
        Contents.Append("\"");
      };
    }
  }
}
