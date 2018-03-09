using System;
using System.Text;
using System.Collections.Generic;

using FSMLib.Compilation.Traversal;

namespace FSMLib.Compilation.Sanitization {

  // TODO: Use sanitizers when transforming an AST into machine code. Do not use in the parsing stage.
  internal abstract class Sanitizer {
    protected StringBuilder Contents;
    protected SimpleDFA<char> SanitizationDFA;

    public Sanitizer() {
      Contents = new StringBuilder();
      SanitizationDFA = new SimpleDFA<char>();
    }

    // TODO: Consider accepting a token instead, so the Sanitizer can throw a reasonable error with positional
    // information if the attempted sanitization process is unsuccessful.
    public string Sanitize(string input) {
      Contents.Clear();

      bool success = SanitizationDFA.Traverse(input.ToCharArray());
      if (!success) {
        // TODO: correct this error message.
        throw new ArgumentException("Input not a valid Regex Token");
      }

      return Contents.ToString();
    }
  }
}
