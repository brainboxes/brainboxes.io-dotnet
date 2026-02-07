using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Brainboxes.IO;
using System.Threading;

namespace Brainboxes.IO.Samples.CSharp.ED.SimpleControlOutputs
{
    class Program
    {
        static void Main(string[] args)
        {
            string[] lcdEnconding = {
                                        "@01009f", //0
                                        "@010090", //1
                                        "@01003b", //2
                                        "@0100b9", //3
                                        "@0100b4", //4
                                        "@0100ad", //5
                                        "@0100af", //6
                                        "@010098", //7
                                        "@0100bf", //8
                                        "@0100bd"  //9
                                    };

            Console.WriteLine("counting 0 to 9");
            
            using (EDDevice ED527 = EDDevice.Create("YOUR_DEVICE_IP"))
            {
                for (int i = 0; i < 10; i++)
                {
                    Console.WriteLine("Count: " + (i % 10));
                    ED527.SendCommand(lcdEnconding[i%10]);
                    Thread.Sleep(1000);
                }
                Console.WriteLine("Fan on ...");
                ED527.Outputs[9].Value = 1;
                Thread.Sleep(1000);
                Console.WriteLine("Fan off ...");
                ED527.Outputs[9].Value = 0;

                Console.WriteLine("Buzzer on ...");
                ED527.Outputs[8].Value = 1;
                Thread.Sleep(250);
                Console.WriteLine("Buzzer off ...");
                ED527.Outputs[8].Value = 0;
            }


        }
    }
}
