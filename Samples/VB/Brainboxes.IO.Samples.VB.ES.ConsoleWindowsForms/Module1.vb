Module Module1

    Sub Main()

        Dim ES As ESDevice = New ES246("YOUR_DEVICE_IP")
        Try
            Console.WriteLine("Connecting to " + ES.ToString())
            'connect method opens all serial ports of the device, an es-246 only has 1 port
            ES.Connect()
            'alternatively just open the port you need
            'ES.Ports(0).Connect()

            ES.Ports(0).Label = "Serial Weighing scale" 'you can give Each port a label, useful For debugging

            Console.WriteLine("Sending Commands")

            'es-246 only has one port, note the ports are indexed from 0
            'the default protocol automatically appends carriage return to the end of a message And encodes data into ASCII
            ES.Ports(0).Send("HELLO ... ")
            ES.Ports(0).Send("HELLO AGAIN...")

            'you can use a different protocol to suit the device you are communicating with Like this
            Dim protocol As DefaultSerialProtocol = New DefaultSerialProtocol()
            protocol.TerminatingCharacters = "\r\n" 'you can Set multiple Char line endings With the Default protocol
            protocol.Encoding = System.Text.Encoding.Unicode 'you can also change the character encoding
            ES.Ports(0).Protocol = protocol
            'alternatively create a class which implements ISerialProtocol

            ' 1 minutes in milliseconds
            Dim msToRx As Integer = 1 * 60 * 1000

            Console.WriteLine("Waiting " + (msToRx / 1000) + " seconds for data to be received")
            Dim sw As Stopwatch = Stopwatch.StartNew()

            While (sw.ElapsedMilliseconds < msToRx)
                Try
                    'write any received data out to the console
                    'note this Is a blocking function, if you have many connections, Or a UI
                    'you will need to do this in another thread
                    'the default protocol waits for the terminating character (by default a /r CARRIAGE RETURN) to determine the end of a message
                    'the default protocol removes the terminating character before returning the message as a string
                    Dim recievedData As String = ES.Ports(0).Receive()
                    Console.WriteLine("Received DATA : " + recievedData)
                Catch ex As TimeoutException
                    'if the receive function times out it throws an exception
                    'but it doesn't matter as we are just waiting for data in this example
                    Console.WriteLine("No data received within timeout")
                End Try

            End While

            sw.Stop()

            ES.Ports(0).Send("GOODBYE!!!!!!!")
        Catch e As Exception
            Console.WriteLine("An error occurred")
            Console.WriteLine(e)
        Finally
            Console.WriteLine("Press enter to exit...")
            Console.ReadKey()

            Console.WriteLine("Disconnecting")
            ES.Disconnect()
        End Try

    End Sub

End Module
