using System;
using System.IO;

using FSMLib;

namespace FSMLibUserTest {
  class MainClass {
    public static void Main(string[] args) {
      string fsm1 =
        "fsm FSM1 {" +
        "  +*State1 -> { 'c' -> State1, State2, State3 }" +
        "}";

      FSM<string> fsm = FSM<string>.FromReader(new StringReader(fsm1));
    }
  }
}
