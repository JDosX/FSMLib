using System;

namespace FSMLib.Compilation.Tokenizing {
  internal interface ITokenProvider {
    Token NextToken(BufferedTextReader reader);
  }
}
