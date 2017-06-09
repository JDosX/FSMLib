using System;
using FSMLib.Compilation.Traversal;

namespace FSMLib.Compilation.Sanitization {
  internal class RegexRawToFnScript : Sanitizer {
    public RegexRawToFnScript() {
      SimpleDFA<char>.Node head = SanitizationDFA.Head;
      SimpleDFA<char>.Node regexBody = new SimpleDFA<char>.Node(false);
      SimpleDFA<char>.Node escapeSlash = new SimpleDFA<char>.Node(false);
      SimpleDFA<char>.Node regexEnd = new SimpleDFA<char>.Node(true);

      // RegexMatch([s], "
      head.SetTransition('/', regexBody).OnTransition += (sender, args) => {
        Contents.Append(String.Format("RegexMatch([{0}], \"", FSM<object>.CURRENT_ITEM_FNVARIABLE_NAME));
      };

      // Regex Body
      regexBody.SetTransition(CharUtils.AllCharsExcept(new char[] { '\\', '/' }), regexBody).OnTransition += (sender, args) => {
        Contents.Append(args.Element);
      };

      // The only escape character that needs to be transformed is /, as this is also used as the regex delimiter.
      // All other escape characters are replaced as they were.
      regexBody.SetTransition('\\', escapeSlash);
      escapeSlash.SetTransition('/', regexBody).OnTransition += (sender, args) => {
        Contents.Append(args.Element);
      };
      escapeSlash.SetTransition(CharUtils.AllCharsExcept('/'), regexBody).OnTransition += (sender, args) => {
        Contents.Append(String.Format("\\{0}", args.Element));
      };

      // ")
      regexBody.SetTransition('/', regexEnd).OnTransition += (sender, args) => {
        Contents.Append("\")");
      };
    }
  }
}
