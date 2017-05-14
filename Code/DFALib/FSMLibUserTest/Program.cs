using System;
using System.IO;

using FSMLib;

namespace FSMLibUserTest {
  class MainClass {
    public static void Main(string[] args) {
      string str1 = "State1++fsm+**\"poopy butthole McGee\"'\\\''`[s] + \"`2`\" == 3`";
      string str2 = "'a'";


      FSM<string> fsm = FSM<string>.FromReader(new StringReader(str1));
    }
  }
}
