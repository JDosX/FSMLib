using System;
using FSMLib.Compilation.Traversal;

namespace FSMLib.Compilation.Sanitization {
  internal class CharToFnScript : Sanitizer {
    public CharToFnScript() {
      SimpleDFA<char>.Node          head = SanitizationDFA.Head;
      SimpleDFA<char>.Node charBodyStart = new SimpleDFA<char>.Node(false);
      SimpleDFA<char>.Node   charBodyEnd = new SimpleDFA<char>.Node(false);
      SimpleDFA<char>.Node   escapeSlash = new SimpleDFA<char>.Node(false);
      SimpleDFA<char>.Node       charEnd = new SimpleDFA<char>.Node(true);

      // [s] == '
      head.SetTransition('\'', charBodyStart).OnTransition += (sender, args) => {
        Contents.Append(String.Format("[{0}] == \'", FSM<object>.CURRENT_ITEM_FNVARIABLE_NAME));
      };

      // Regex Body
      charBodyStart.SetTransition(CharUtils.AllCharsExcept(new char[] { '\\', '\'' }), charBodyEnd).OnTransition += (sender, args) => {
        Contents.Append(args.Element);
      };

      // Escape chars. Prevents an escaped single quotes from being recognised as the end of the string.
      charBodyStart.SetTransition('\\', escapeSlash);
      escapeSlash.SetTransition(CharUtils.AllChars(), charBodyEnd).OnTransition += (sender, args) => {
        Contents.Append(String.Format("\\{0}", args.Element));
      };

      // '
      charBodyEnd.SetTransition('\'', charEnd).OnTransition += (sender, args) => {
        Contents.Append("\'");
      };
    }
  }
}
