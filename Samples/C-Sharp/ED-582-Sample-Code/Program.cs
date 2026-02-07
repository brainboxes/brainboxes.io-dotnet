using Brainboxes.IO;
using System;


namespace ED_582_Sample_Code
{
    class Program
    {
        static ED038 ed038;
        static double ambientTemperature = 21.0; // ambient room temperature in Celsius
        static double deltaTemperature = 3.0 ; // ambient temperature range 

        static void Main(string[] args)
        {
            using (ed038 = new ED038(new TCPConnection("YOUR_DEVICE_IP"))) //IP address of ED-038
            {
                using (ED582 ed582 = new ED582(new TCPConnection("YOUR_DEVICE_IP")))//IP address of ED-582
                {
                    ed038.Connect();
                    ed582.Connect();                    

                    // Setting temperature unit of ED-582 to Celsius
                    ASCIIProtocol ap = (ASCIIProtocol)ed582.Protocol;
                    if (ap.TemperatureUnit != TemperatureUnit.Celsius)
                    {
                        Console.WriteLine("Setting temperature unit and restarting ED-582");
                        ap.SetTemperatureUnit(TemperatureUnit.Celsius);
                        // After changing the temperature unit we need to restart the device for the changes to take effect.
                        // Restart() will reconnect to the device
                        ed582.Restart();     
                    }
                    ed582.IOLines[0].Label = "Shop Floor";
                    ed038.IOLines[0].Label = "AC Unit A";

                    Console.WriteLine("Initialising...");
                    Console.WriteLine($"Current temperature is {ed582.IOLines[0].AValue} Celsius");

                    // attaching functions to event handlers to control the room temperature
                    AIOLineChangedEventHandler airConditionerEventHandler = AirConditionerEventFunction;

                    // This event handler will be triggered when the temperature changes more than the specified delta value away from the ambient temperature
                    // The aim of the system is to maintain an ambient temperature of 21 C
                    // monitor the temperature, and switch on/off the air conditioner whenever the temperature falls outside the target value by 3 C
                    // when the temperature hits 18 C, switch off air conditioner
                    // when the temperature hits 24 C, switch on air conditioner
                    ed582.IOLines[0].SubscribeToTargetRangeEvent(ref airConditionerEventHandler, ambientTemperature, deltaTemperature);
                
                    // Setting the outputs either on/off depending on the initial temperature
                    if (ed582.IOLines[0].Value >= (ambientTemperature + deltaTemperature))
                    {
                        ed038.Outputs[0].Value = 1;
                        Console.WriteLine("Shop Floor: Switched Air Conditioner On");
                    }
                    else if (ed582.IOLines[0].Value <= (ambientTemperature - deltaTemperature))
                    {
                        ed038.Outputs[0].Value = 0;
                        Console.WriteLine("Shop Floor: Switched Air Conditioner Off");
                    }                 

                    Console.WriteLine("Press any key to continue...");
                    Console.ReadKey();
                }
            }
        }

        // event handler function to control the air conditioner
        private static void AirConditionerEventFunction(IOLine line, EDDevice device, double value, AIOChangeTypes changeType)
        {
            if (changeType == AIOChangeTypes.Exit && value > (ambientTemperature + deltaTemperature))
            {
                // switch on AC
                Console.WriteLine("Shop Floor: Switching Air Conditioner On");
                ed038.Outputs[0].Value = 1;
            }

            else if (changeType == AIOChangeTypes.Exit && value < (ambientTemperature - deltaTemperature))
            {
                // switch off AC
                Console.WriteLine("Shop Floor: Switching Air Conditioner Off");
                ed038.Outputs[0].Value = 0;
            }
        }
        
    }

}
