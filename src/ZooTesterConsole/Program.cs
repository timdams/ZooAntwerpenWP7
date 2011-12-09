using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Text.RegularExpressions;

namespace ZooTesterConsole
{


    class Program
    {
        static void Main(string[] args)
        {
            ZooSchedule totalSchedule = new ZooSchedule();
           
            totalSchedule= ZooParser.GetTotalSchedule(Languages.Dutch,DateTime.Now);
        }


    }


    
}
