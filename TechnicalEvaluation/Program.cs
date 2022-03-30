using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Management;
using System.Threading;

namespace IMADA_Force_Measure
{
    class Program
	{
		static void Main()
        {
			FinalForce function = new FinalForce();
			function.Start();
		}
	}
}
