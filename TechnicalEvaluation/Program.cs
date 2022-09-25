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
			//FinalForce measureTarget = new FinalForce();
			FinalFrequency measureTarget = new FinalFrequency();
			//FinalPreload measureTarget = new FinalPreload();
			measureTarget.Start();
		}
	}
}