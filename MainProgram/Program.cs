using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics.Contracts;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using abb.egm;

namespace EgmMyo
{
    class Program
    { // listen on an assigned port for inbound messages
        public static int IpPortNumber = 6510; //6510
        public static bool exitProg = false;
        static void Main()
        {
            string line = null;
            Sensor s = new Sensor();
            MyoProgram m = new MyoProgram(s);
        }
    }



}
