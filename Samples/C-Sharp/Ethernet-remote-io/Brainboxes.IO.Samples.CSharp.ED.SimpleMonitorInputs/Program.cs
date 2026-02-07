using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Brainboxes.IO;

namespace SystemotronicExample2Inputs
{
    class Program
    {
        static void Main(string[] args)
        {
            using (EDDevice ED008 = EDDevice.Create("YOUR_DEVICE_IP"))
            {
                Console.WriteLine("turning light on");

                int numberOfCans = 0;

                ED008.Inputs[0].IOLineFallingEdge += (line, device, changeType) => 
                {
                    numberOfCans++;
                    Console.WriteLine( ""+numberOfCans + " CAN(s) DETECTED" );
                    //Store information in database
                    DateTime canTime = DateTime.UtcNow;

                };

                ED008.Inputs[2].IOLineRisingEdge += (line, device, changeType) =>
                {
                    Console.WriteLine("Switch ON");
                };

                ED008.Inputs[2].IOLineFallingEdge += (line, device, changeType) =>
                {
                    Console.WriteLine("Switch OFF");
                };

                Console.WriteLine("Press any key to exit...");
                Console.ReadKey();
            }
        }
    }
}
