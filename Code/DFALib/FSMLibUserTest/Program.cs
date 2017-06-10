using System;
using System.IO;

using FSMLib;

namespace FSMLibUserTest {
  class MainClass {
    public static void Main(string[] args) {
      string fsm1 =
        "fsm FSM1 {" +
        "  +State1 -> { `[s] < 'z'` -> State2 }" +
        "   State2 -> { 'a' -> State3 }" +
        "   State3 -> { 'd' -> State4 }" +
        "  *State4" +
        "}";

      FSM<char> fsm = FSM<char>.FromReader(new StringReader(fsm1));

      bool success = fsm.Traverse("zad".ToCharArray());
      Console.WriteLine(success);

      Console.ReadKey();
    }
  }
}
