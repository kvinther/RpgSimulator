using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NLua;

namespace Scenario
{
    public class Game
    {
        private Lua _lua;

        public Game()
        {
            _lua = new Lua();
            InitializeLua();
        }

        private void InitializeLua()
        {
            _lua.RegisterFunction("add", this, this.GetType().GetMethod("Add"));
        }

        public int Add(int n, int m)
        {
            return n + m;
        }
    }
}
