using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Xml;

namespace Brainboxes.IO.Tests
{
    /// <summary>
    /// Convenience class to manage all the connections provided in the App.config XML file
    /// also yield connections in iterator block in random order
    /// </summary>
    public class ConnectionPool
    {
        protected Dictionary<string, IConnection> _allConnections = new Dictionary<string, IConnection>();

        public Dictionary<string, IConnection> ValidConnections = new Dictionary<string, IConnection>();
        public Dictionary<string, IConnection> InvalidConnections = new Dictionary<string, IConnection>();


        public Dictionary<string, IConnection> ESConnections = new Dictionary<string, IConnection>();
        public Dictionary<string, IConnection> ES246Connections = new Dictionary<string, IConnection>();
        public Dictionary<string, IConnection> ES257Connections = new Dictionary<string, IConnection>();
        public Dictionary<string, IConnection> ES701Connections = new Dictionary<string, IConnection>();
        public Dictionary<string, IConnection> ES279Connections = new Dictionary<string, IConnection>();


        public Dictionary<string, IConnection> ED527Connections = new Dictionary<string, IConnection>();
        public Dictionary<string, IConnection> ED588Connections = new Dictionary<string, IConnection>();
        public Dictionary<string, IConnection> ED008Connections = new Dictionary<string, IConnection>();
        public Dictionary<string, IConnection> ED204Connections = new Dictionary<string, IConnection>();
        public Dictionary<string, IConnection> ED549Connections = new Dictionary<string, IConnection>();
        public Dictionary<string, IConnection> ED560Connections = new Dictionary<string, IConnection>();
        public Dictionary<string, IConnection> ED582Connections = new Dictionary<string, IConnection>();

        public Dictionary<string, IConnection> BB400Connections = new Dictionary<string, IConnection>();

        public Dictionary<string, IConnection> EDConnections = new Dictionary<string, IConnection>();

        public Dictionary<string, IConnection> ValidTCPConnections = new Dictionary<string, IConnection>();
        public Dictionary<string, IConnection> ValidSerialConnections = new Dictionary<string, IConnection>();

        public Dictionary<string, IConnection> InvalidTCPConnections = new Dictionary<string, IConnection>();
        public Dictionary<string, IConnection> InvalidSerialConnections = new Dictionary<string, IConnection>();

        public Dictionary<string, IConnection> DevicesWithDigitalIOConnections = new Dictionary<string, IConnection>();
        public Dictionary<string, IConnection> DevicesWithAnalogIOConnections = new Dictionary<string, IConnection>();

        /// <summary>
        /// Digital Inputs
        /// </summary>
        public Dictionary<string, IConnection> DevicesWithInputsConnections = new Dictionary<string, IConnection>();
        /// <summary>
        /// Digital Outputs
        /// </summary>
        public Dictionary<string, IConnection> DevicesWithOutputsConnections = new Dictionary<string, IConnection>();
        public Dictionary<string, IConnection> DevicesWithAnalogueInputsConnections = new Dictionary<string, IConnection>();
        public Dictionary<string, IConnection> DevicesWithAnalogueOutputsConnections = new Dictionary<string, IConnection>();



        public Dictionary<string, IConnection> DevicesWhichCanBeFactoryReset = new Dictionary<string, IConnection>();

        /// <summary>
        /// Initialise the connection pool by reading the app.config and adding the devices to the relevant pools
        /// </summary>
        public ConnectionPool()
        {
            // Need to keep any invalid entries from the config file
            AddConnectionTo(InvalidConnections, "invalid");
            AddConnectionTo(InvalidTCPConnections, "invalidIPAddress");
            AddConnectionTo(InvalidSerialConnections, "invalidComPort");

#if !DEBUG
            string dutIpAddress = Environment.GetEnvironmentVariable("BB_DUTIP", EnvironmentVariableTarget.Machine);
            string dutProductCode = Environment.GetEnvironmentVariable("BB_DUTMODEL", EnvironmentVariableTarget.Machine);
            // If running the test from DevOps, it will pass which tests to run for which DUT
            //      The DUT product code is stored in 'BB_DUTMODEL' so a if statement or switch could be used on that variable here
            //      For the time being, I am adding the same IP address to all the valid dictionaries
            //      Your test plan in DevOps should not run tests which the DUT does not or should not support
            AddDutTo(ValidConnections, dutIpAddress, dutProductCode);
            AddDutTo(ED527Connections, dutIpAddress, dutProductCode);
            AddDutTo(ED588Connections, dutIpAddress, dutProductCode);
            AddDutTo(ED560Connections, dutIpAddress, dutProductCode);
            AddDutTo(ED549Connections, dutIpAddress, dutProductCode);
            AddDutTo(ED008Connections, dutIpAddress, dutProductCode);
            AddDutTo(ED204Connections, dutIpAddress, dutProductCode);
            AddDutTo(BB400Connections, dutIpAddress, dutProductCode);
            AddDutTo(EDConnections, dutIpAddress, dutProductCode);
            AddDutTo(ValidTCPConnections, dutIpAddress, dutProductCode);
            AddDutTo(ValidSerialConnections, dutIpAddress, dutProductCode);
            AddDutTo(DevicesWithOutputsConnections, dutIpAddress, dutProductCode);
            AddDutTo(DevicesWithDigitalIOConnections, dutIpAddress, dutProductCode);
            AddDutTo(DevicesWithAnalogueOutputsConnections, dutIpAddress, dutProductCode);
            AddDutTo(DevicesWithInputsConnections, dutIpAddress, dutProductCode);
            AddDutTo(DevicesWithAnalogueInputsConnections, dutIpAddress, dutProductCode);
            AddDutTo(DevicesWhichCanBeFactoryReset, dutIpAddress, dutProductCode);
#else
            AddConnectionTo(ValidConnections, "E");
            AddConnectionTo(ValidConnections, "B");

            AddConnectionTo(ESConnections, "ES");
            AddConnectionTo(ES246Connections, "ES246");
            AddConnectionTo(ES257Connections, "ES257");
            AddConnectionTo(ES701Connections, "ES701");

            AddConnectionTo(ED527Connections, "ED527");

            AddConnectionTo(ED588Connections, "ED588");
            AddConnectionTo(ED560Connections, "ED560");
            AddConnectionTo(ED549Connections, "ED549");
            AddConnectionTo(ED588Connections, "eDAM8050ComPort");
            AddConnectionTo(ED582Connections, "ED582");


            AddConnectionTo(ED008Connections, "ED008");
            AddConnectionTo(ED204Connections, "ED204");

            AddConnectionTo(BB400Connections, "BB400");

            AddConnectionTo(EDConnections, "ED");
            AddConnectionTo(EDConnections, "BB");
            AddConnectionTo(EDConnections, "eDAM8050ComPort");

            AddConnectionTo(ValidTCPConnections, "BB400IPAddress");
            AddConnectionTo(ValidTCPConnections, "ED527IPAddress");
            AddConnectionTo(ValidTCPConnections, "ED516IPAddress");
            AddConnectionTo(ValidTCPConnections, "ED588IPAddress");
            AddConnectionTo(ValidTCPConnections, "ED008IPAddress");
            AddConnectionTo(ValidTCPConnections, "ED560IPAddress");
            AddConnectionTo(ValidTCPConnections, "ED549IPAddress");
            AddConnectionTo(ValidTCPConnections, "ED204IPAddress");

            AddConnectionTo(ValidSerialConnections, "ED527ComPort");
            AddConnectionTo(ValidSerialConnections, "ED516ComPort");
            AddConnectionTo(ValidSerialConnections, "ED588ComPort");
            AddConnectionTo(ValidSerialConnections, "ED008ComPort");
            AddConnectionTo(ValidSerialConnections, "eDAM8050ComPort");
            AddConnectionTo(ValidSerialConnections, "ED560ComPort");
            AddConnectionTo(ValidSerialConnections, "ED549ComPort");

            AddConnectionTo(DevicesWithOutputsConnections, "ED527");
            AddConnectionTo(DevicesWithOutputsConnections, "ED588");
            AddConnectionTo(DevicesWithOutputsConnections, "ED008");
            AddConnectionTo(DevicesWithOutputsConnections, "ED204");
            AddConnectionTo(DevicesWithOutputsConnections, "BB400");
            //AddConnectionTo(DevicesWithOutputsConnections, "eDAM8050ComPort");
            

            AddConnectionTo(DevicesWithDigitalIOConnections, "BB400");
            AddConnectionTo(DevicesWithDigitalIOConnections, "ED004");
            AddConnectionTo(DevicesWithDigitalIOConnections, "ED008");
            AddConnectionTo(DevicesWithDigitalIOConnections, "ED204");
            AddConnectionTo(DevicesWithDigitalIOConnections, "ED516");
            AddConnectionTo(DevicesWithDigitalIOConnections, "ED527");
            AddConnectionTo(DevicesWithDigitalIOConnections, "ED538");
            AddConnectionTo(DevicesWithDigitalIOConnections, "ED588");


            AddConnectionTo(DevicesWithAnalogueOutputsConnections, "ED560");

            AddConnectionTo(DevicesWithInputsConnections, "ED588");
            AddConnectionTo(DevicesWithInputsConnections, "ED516");
            AddConnectionTo(DevicesWithInputsConnections, "eDAM8050ComPort");
            AddConnectionTo(DevicesWithInputsConnections, "ED008");
            AddConnectionTo(DevicesWithInputsConnections, "ED204");
            AddConnectionTo(DevicesWithInputsConnections, "BB400");

            AddConnectionTo(DevicesWithAnalogueInputsConnections, "ED549");

            AddConnectionTo(DevicesWithAnalogueInputsConnections, "ED582");
            AddConnectionTo(DevicesWithAnalogueInputsConnections, "ED593");
            
            //eDAM modules cannot be factory reset without a jumper 
            AddConnectionTo(DevicesWhichCanBeFactoryReset, "ED");
            AddConnectionTo(DevicesWhichCanBeFactoryReset, "BB");
#endif
        }

        private void AddDutTo(Dictionary<string, IConnection> dict, string dutIp, string dutModel)
        {
            dict.Add(dutModel, Connection.Create(dutIp));
        }

        private void AddConnectionTo(Dictionary<string, IConnection> dict, string appSetting)
        {
            IEnumerable<string> keys = ConfigurationManager.AppSettings.AllKeys.Where(k => k.StartsWith(appSetting));
            foreach (string key in keys)
            {
                string value = ConfigurationManager.AppSettings[key];
                //don't add values which are not available
                if (String.IsNullOrWhiteSpace(value)) continue;
                IConnection c;
                //connections keyed by value in the allConnections dictionary, this means we can have 2 settings with the same value using the same connection resource
                if (_allConnections.ContainsKey(key))
                {
                    //get the existing connection
                    c = _allConnections[key];
                }
                else
                {
                    if(key.StartsWith("ES"))
                    {
                        c = Connection.Create(value,9001);
                    }
                    else
                    {
                        c = Connection.Create(value);
                    }
                    _allConnections.Add(key, c);
                }
                //connections keyed by appSetting name in the specific dictionary
                dict.Add(key, c);
            }
        }

        public void CloseAll()
        {
            foreach (KeyValuePair<string, IConnection> c in _allConnections)
            {
                try
                {
                    c.Value.Disconnect();
                }
                catch (Exception e)
                {
                    Debug.WriteLine("Failed to disconnect " + c.Key + ": " + e.Message);
                }
            }
        }
    }

    public static class LINQExtension
    {
        /// <summary>
        /// allow interaction over the connections by yielding in random order, skipping the test if there are no connections available
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="conns"></param>
        /// <returns></returns>
        public static IEnumerable<T> RandomTestOrder<T>(this IEnumerable<KeyValuePair<string, T>> conns)
        {
            // Eager check - runs immediately, not deferred like iterator methods
            // This ensures Assert.Inconclusive is thrown before Parallel.ForEach starts,
            // avoiding it being wrapped in AggregateException
            if (conns == null || !conns.Any<KeyValuePair<string, T>>())
            {
                Debug.WriteLine("No Valid Connections in app.config");
                Assert.Inconclusive("No Valid Connections in app.config");
            }

            return RandomTestOrderIterator(conns);
        }

        private static IEnumerable<T> RandomTestOrderIterator<T>(IEnumerable<KeyValuePair<string, T>> conns)
        {
            Random r = new Random();
            IOrderedEnumerable<KeyValuePair<string, T>> conListRandomOrder = conns.OrderBy(c => r.Next());
            foreach (KeyValuePair<string, T> kvp in conListRandomOrder)
            {
                Console.WriteLine("");
                Console.WriteLine("============================================");
                Console.WriteLine("========== " + kvp.Key + " = " + kvp.Value.ToString() + " ========");
                Console.WriteLine("============================================");
                //handy for debugging test to put this in debug as well
                Debug.WriteLine("");
                Debug.WriteLine("============================================");
                Debug.WriteLine("========== " + kvp.Key + " = " + kvp.Value.ToString() + " ========");
                Debug.WriteLine("============================================");

                yield return kvp.Value;
            }
        }

        /// <summary>
        /// Return the information required to use the EDDevice Factory Method
        /// </summary>
        /// <param name="conn"></param>
        /// <param name="ipAddress"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        public static Boolean CreateConnectionData(this IConnection conn, out string connString, out int data)
        {
            SerialConnection s = conn as SerialConnection;
            if (s != null)
            {
                connString = s.PortName;
                data = s.BaudRate;
                return true;
            }
            
            TCPConnection tcp = conn as TCPConnection;
            if(tcp != null)
            {
                XmlDocument xmlDoc;
                Type deviceType = EDDevice.DeviceTypeFromIP(tcp.IP, out xmlDoc);


                if (deviceType == typeof(BB400))
                {
                    data = 9500;
                }
                else
                {
                    //query XML doc for protocol and port number
                    string protocol = xmlDoc.GetElementsByTagName("currpro")[0].InnerText;
                    data = Convert.ToInt32(xmlDoc.GetElementsByTagName(protocol == "ASCII" ? "dtcpport" : "mtcpport")[0].InnerText);
                }

                connString = tcp.IP;
                return true;
            }
            
            //cannot find way to get connection info
            connString = "unknown";
            data = 0;
            return false;
        }

    }
}
