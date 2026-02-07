using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Brainboxes.IO.Tests
{
    public static class EDDeviceExtensionMethods
    {
        /// <summary>
        /// Change the devices protocol
        /// </summary>
        /// <param name="device"></param>
        /// <param name="protocol"></param>
        public static void ChangeIOProtocol(this EDDevice device, IIOProtocol protocol)
        {
            TCPConnection tcp = device.Connection as TCPConnection;
            if (tcp == null)
            {
                throw new InvalidOperationException("Device must have a TCP connection");
            }
            if (device.Protocol == protocol)
            {
                return;
            }
            bool wasConnected = device.IsConnected;
            device.Disconnect();
            if(SetProtocolForTCPConnection(tcp, protocol is ASCIIProtocol))
            {
                device.Protocol = protocol;
            }

            if (wasConnected)
            {
                device.Connect();
            }
        }

        internal static bool SetProtocolForTCPConnection(TCPConnection con, bool toASCII = true)
        {
            string newProtocol = toASCII ? "1" : "2";
            int newPortNumber = toASCII ? TCPConnection.DEFAULT_ASCII_PORT : TCPConnection.DEFAULT_MODBUSTCP_PORT;
            string url = "http://" + con.IP + "/proto.cgi?currpro=" + newProtocol + "&enablepenet=1&penet=1&enableps1=1&ps1=1&mtcpport="+TCPConnection.DEFAULT_MODBUSTCP_PORT+ "&mmaxc=8&midle=0&msid=0&mdf=0&dAddr=1&dtcpport=" + TCPConnection.DEFAULT_ASCII_PORT + "&didle=0&dcu=0&dcm=0&ddf=0&dms=0&dcs=0&diA=1&doA=1&dbaud=6&dcmdto=200&iotenable=0&iotrip1=192&iotrip2=168&iotrip3=0&iotrip4=0&iotdport=9500&iotmport=502&iotrmadd=1&iotrdadd=1&iotsafedisconn=1";
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(20);
                    HttpResponseMessage response = client.GetAsync(url).GetAwaiter().GetResult();
                    if (response.IsSuccessStatusCode)
                    {
                        con.Port = newPortNumber;
                        return true;
                    }
                }
            }
            catch(HttpRequestException e)
            {
                throw new Exception("Unable to change protocol Are you sure all active connections to the device are closed", e);
            }
            return false;
        }

        public static IEnumerable<IConnection> AsEachProtocol(this IEnumerable<IConnection> conns)
        {
            IEnumerable<SerialConnection> serialCons = conns.OfType<SerialConnection>();

            //filter non tcp connections, they can only be ASCII
            IEnumerable<TCPConnection> tcpConns = conns.OfType<TCPConnection>();

            //to make the tests run quicker loop twice,
            //once with the current protocol, then switch the protocol and run the next connection
            foreach (TCPConnection con in tcpConns)
            {
                XmlDocument doc;
                Type deviceType = EDDevice.DeviceTypeFromIP(con.IP, out doc);

                if(deviceType == typeof(BB400))
                {
                    yield return con;
                }
                else
                {
                    string currentProtocol = doc.GetElementsByTagName("currpro")[0].InnerText;
                    con.Port = currentProtocol.StartsWith("Modbus") ? TCPConnection.DEFAULT_MODBUSTCP_PORT : TCPConnection.DEFAULT_ASCII_PORT;
                    //return as the current protocol
                    yield return con;

                    //then switch protocols and return again in second loop
                    SetProtocolForTCPConnection(con, currentProtocol.StartsWith("Modbus"));
                }
            }
            foreach(SerialConnection con in serialCons)
            {
                yield return con;
            }
            foreach(TCPConnection con in tcpConns)
            {
                System.Threading.Thread.Sleep(10000); //ouch
                yield return con;
            }
        }

        public static IEnumerable<IConnection> AsASCIIProtocol(this IEnumerable<IConnection> conns)
        {
            foreach (IConnection con in conns)
            {
                TCPConnection tcp = con as TCPConnection;
                if (tcp == null)
                {
                    yield return con; //its serial connection therefore ASCII
                }
                else
                {
                    XmlDocument doc;
                    Type deviceType = EDDevice.DeviceTypeFromIP(tcp.IP, out doc);

                    if(deviceType == typeof(BB400))
                    {
                        yield return tcp;
                    }
                    else
                    {
                        string currentProtocol = doc.GetElementsByTagName("currpro")[0].InnerText;

                        if(currentProtocol.StartsWith("ASCII"))
                        {
                            tcp.Port = TCPConnection.DEFAULT_ASCII_PORT;
                            yield return tcp;
                        }
                        else
                        {
                            SetProtocolForTCPConnection(tcp, true);
                            System.Threading.Thread.Sleep(10000); //ouch what a hack
                            yield return tcp;
                        }
                    }
                }
            }
        }

        public static IEnumerable<TCPConnection> AsModbusTCP(this IEnumerable<IConnection> conns)
        {
            // filter non TCP connections, they can only be ASCII
            IEnumerable<TCPConnection> tcpConns = conns.OfType<TCPConnection>();

            foreach (TCPConnection con in tcpConns)
            {

                XmlDocument doc;
                EDDevice.DeviceTypeFromIP(con.IP, out doc);

                string currentProtocol = doc.GetElementsByTagName("currpro")[0].InnerText;

                if (currentProtocol.StartsWith("Modbus"))
                {
                    con.Port = TCPConnection.DEFAULT_MODBUSTCP_PORT;
                    yield return con;
                }
                else
                {
                    SetProtocolForTCPConnection(con, false);
                    System.Threading.Thread.Sleep(10000); //ouch what a hack
                    yield return con;
                }
            }
        }
    }
}
