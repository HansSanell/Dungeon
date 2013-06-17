using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;


namespace Dungeon
{
    public class App_Log
    {
        private string fn;
        private DateTime time = DateTime.Now;
        private string format = "MMM d HH:mm:ff";
        
        public App_Log(string filename)
        {
            fn = filename;

        }

        public void print(string message, int always = 1)
        {
            if (Config.DEBUG)
            {
                Trace.WriteLine(message);
            }
            if (always == 1)
            {
                using (System.IO.StreamWriter file = new System.IO.StreamWriter(fn, true))
                {
                    time = DateTime.Now;
                    file.WriteLine(string.Format("{0} {1}", time.ToString(format), message));
                }
            }
        }
    }
}
