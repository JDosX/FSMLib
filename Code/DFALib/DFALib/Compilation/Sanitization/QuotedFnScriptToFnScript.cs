using System;
using FSMLib.Compilation.Traversal;

namespace FSMLib.Compilation.Sanitization {
  internal class QuotedFnScriptToFnScript : Sanitizer {
    public QuotedFnScriptToFnScript() {
      SimpleDFA<char>.Node head = SanitizationDFA.Head;
      SimpleDFA<char>.Node fnScriptBody = new SimpleDFA<char>.Node(false);
      SimpleDFA<char>.Node fnScriptEnd = new SimpleDFA<char>.Node(true);

      // Remove leading `
      head.SetTransition('`', fnScriptBody);

      // Regex Body
      fnScriptBody.SetTransition(CharUtils.AllCharsExcept('`'), fnScriptBody).OnTransition += (sender, args) => {
        Contents.Append(args.Element);
      };

      // Remove trailing `
      fnScriptBody.SetTransition('`', fnScriptEnd);
    }
  }
}
