using System;
using System.Text.RegularExpressions;
using FunctionScript;

namespace FSMLib.Compilation {
  internal class FnMethod_RegexMatch : FnMethod<bool> {
    [FnArg] protected FnObject<string> Input;
    [FnArg] protected FnObject<string> Pattern;

    public override bool GetValue() {
      return Regex.Match(Input.GetValue(), Pattern.GetValue()).Success;
    }
  }

  internal static class FnScriptExtender {
    static FnScriptExtender() {
      FnScriptResources.AddSwitch("RegexMatch");
      FnScriptResources.AddMethodPointerToSwitch("RegexMatch", new FnMethod_RegexMatch());
    }
  }
}
