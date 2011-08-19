using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Test_Windows_Game
{
    class JavaScriptContext : Noesis.Javascript.JavascriptContext 
    {
        public void RunVoid(string script) {
            Run(script + ";0;");
        }
    }
}
