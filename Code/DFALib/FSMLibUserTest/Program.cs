using System;
using System.IO;

using FSMLib;

namespace FSMLibUserTest {
  class MainClass {
    public static void Main(string[] args) {
      string fsm1 =
        "fsm FSM1 {" +
        "  +State1 -> { /0.1.0.1./ -> State2 }" +
        "   State2 -> { \"am\" -> State3 }" +
        "   State3 -> { \"Sam\" -> State4 }" +
        "  *State4" +
        "}";

      FSM<string> fsm = FSM<string>.FromReader(new StringReader(fsm1));

      string[] stringCollection = new string[] { "0a1b0c1d", "am", "Sam" };

      bool success = fsm.Traverse(stringCollection);
      Console.WriteLine(success);

      Console.ReadKey();
    }
  }
}
