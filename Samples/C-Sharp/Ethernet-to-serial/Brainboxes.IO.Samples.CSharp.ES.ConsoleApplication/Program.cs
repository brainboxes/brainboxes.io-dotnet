using System;
using System.Diagnostics;
using Brainboxes.IO;

namespace Brainboxes.IO.Samples.ES.ConsoleApplication
{
    class Program
    {

        static void Main(string[] args)
        {
            ESDevice es = new ES246("YOUR_DEVICE_IP");
            try
            {
                Console.WriteLine("Connecting to " + es);
                //connect method opens all serial ports of the device, an es-246 only has 1 port
                es.Connect();
                //alternatively just open the port you need
                //es.Ports[0].Connect();

                es.Ports[0].Label = "Serial Weighing scale"; //you can give each port a label, useful for debugging

                //you can use a different protocol to suit the device you are communicating with like this:
                es.Ports[0].Protocol = new DefaultSerialProtocol()
                {
                    TerminatingCharacters = "\r\n", //you can set multiple char line endings with the default protocol
                    Encoding = System.Text.Encoding.Unicode, //you can also change the character encoding
                };
                //alternatively create a class which implements ISerialProtocol

                Console.WriteLine("Sending Commands");

                //es-246 only has one port, note the ports are indexed from 0
                //the default protocol automatically appends carriage return to the end of a message and encodes data into ASCII
                es.Ports[0].Send("HELLO ... ");
                es.Ports[0].Send("HELLO AGAIN...");

                // 1 minutes in milliseconds
                int msToRx = 1 * 60 * 1000;

                Console.WriteLine("Waiting " + (msToRx / 1000) + " seconds for data to be received");
                Stopwatch sw = Stopwatch.StartNew();

                while (sw.ElapsedMilliseconds < msToRx)
                {
                    try
                    {
                        //write any received data out to the console
                        //note this is a blocking function, if you have many connections, or a UI
                        //you will need to do this in another thread
                        //the default protocol waits for the terminating character (by default a /r CARRIAGE RETURN) to determine the end of a message
                        //the default protocol removes the terminating character before returning the message as a string
                        string recievedData = es.Ports[0].Receive();
                        Console.WriteLine("Received DATA : " + recievedData);
                    }
                    catch (TimeoutException)
                    {
                        //if the receive function times out it throws an exception
                        //but it doesn't matter as we are just waiting for data in this example
                        Console.WriteLine("No data received within timeout");
                    }
                }
                sw.Stop();

                es.Ports[0].Send("GOODBYE!!!!!!!");
            }
            catch (Exception e)
            {
                Console.WriteLine("An error occurred");
                Console.WriteLine(e);
            }
            finally
            {
                Console.WriteLine("Press enter to exit...");
                Console.ReadKey();

                Console.WriteLine("Disconnecting");
                es.Disconnect();
            }

        }
    }
}
