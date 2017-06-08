using System;

using FSMLib.Compilation.Traversal;

namespace FSMLib.Compilation.Sanitization {
  internal class RegexRawToFnScript : Sanitizer {
    public RegexRawToFnScript() {
      SimpleDFA<char>.Node head = SanitizationDFA.Head;
      SimpleDFA<char>.Node regexBody = new SimpleDFA<char>.Node(false);
      SimpleDFA<char>.Node escapeSlash = new SimpleDFA<char>.Node(false);
      SimpleDFA<char>.Node regexEnd = new SimpleDFA<char>.Node(true);

      head.TryAddConnection('/', regexBody).OnTransition += (sender, args) => {
        Contents.Append("RegexMatch([s], ");
      };

      // Parsing escape characters
      regexBody.TryAddConnection('\\', escapeSlash);

      // TODO: The below doesn't work because each char gets a unique transition, and that's bubkiss because
      // we need to append the event to all the transitions. We need to make it such that when you add connections,
      // each input char gets the same transiton. So essentially:
      // HashSet<char> --- maps to ---> Transiton --- composed of ---> (Destination, Event Handler)
      escapeSlash.TryAddConnections(AllChars, regexBody).OnTransition += (sender, args) => {
        Contents.Append("\\" + args.Element);
      }
    }
  }
}
