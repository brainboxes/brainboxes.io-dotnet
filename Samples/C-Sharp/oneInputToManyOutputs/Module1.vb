Imports Brainboxes.IO

Module Module1

    'the IP addresses need to be updated to match you devices IPs

    'this is the IP of the device which we will monitor the inputs from
    Dim inputDeviceIP As String = "YOUR_DEVICE_IP"
    'add as many device IPs as you wish to control the outputs of to this array
    Dim outDevicesIPs As String() = {"YOUR_DEVICE_IP"}
    'example of havinf 3 devices in the array
    'Dim outDevicesIPs As String() = {"YOUR_DEVICE_IP", "YOUR_DEVICE_IP", "YOUR_DEVICE_IP"}

    ' store the output devices in this array
    Dim outputDevices As List(Of EDDevice) = New List(Of EDDevice)
    ' store the input device in this variable
    Dim inputDevice As EDDevice

    Sub Main()

        Console.WriteLine("##################################### ")
        Console.WriteLine("One Input to Many Outputs Sample Code ")
        Console.WriteLine("##################################### ")
        Console.WriteLine()
        Try

            ' initialise and connect to each output device, then add them to the list
            For Each ip As String In outDevicesIPs
                Console.WriteLine("Connecting to output device at IP: " + ip)
                Dim outputDevice As EDDevice = EDDevice.Create(ip)
                Console.WriteLine("Connected to " + outputDevice.Protocol.DeviceName) 'you can set the device name on the webpage
                outputDevices.Add(outputDevice)
            Next

            'set up the device which we will monitor the inputs from
            Console.WriteLine("Connecting to input device at IP: " + inputDeviceIP)
            inputDevice = EDDevice.Create(inputDeviceIP)
            Console.WriteLine("Connected to " + inputDevice.Protocol.DeviceName)

            'when the io lines of the device change go to the input change function
            AddHandler inputDevice.IOLinesChanged, AddressOf InputDevice_IOLinesChange

            'set the initial state of the outputs of all the devices to match in inputs on the input device
            Console.WriteLine("Setting initial state of all output devices")
            InputDevice_IOLinesChange(inputDevice.Inputs.ToList(), inputDevice)

        Catch ex As Exception
            Console.WriteLine("An exception occurred.")
            Console.WriteLine(ex)
        Finally
            Console.WriteLine("Press any key to exit...")
            Console.ReadKey()

            Console.WriteLine("disconnecting from devices")

            If (inputDevice IsNot Nothing) Then
                inputDevice.Disconnect()
            End If

            For Each outputDevice As EDDevice In outputDevices
                outputDevice.Disconnect()
            Next
        End Try

    End Sub

    Private Sub InputDevice_IOLinesChange(lines As List(Of IOLine), device As EDDevice)

        'see if any of the changes are input lines
        Dim inputsChanged = lines.Where(Function(line) line.IODirection = IODirection.Input)

        'if any were inputs then switch the matching outputs of the output devices
        If (inputsChanged.Any()) Then
            Console.WriteLine(DateTime.Now.ToLocalTime() + " The following inputs have changed on: " + device.Protocol.DeviceName)
            For Each inputLine As IOLine In inputsChanged
                Console.WriteLine("   - Input Number" + inputLine.IONumber.ToString() + " new value: " + inputLine.Value.ToString())
            Next

            'loop through each output device
            For Each outputDevice As EDDevice In outputDevices
                Console.WriteLine("Setting corresponding outputs on " + outputDevice.Protocol.DeviceName)
                'loop through each input io line and set the matching output line value appropriately
                For Each inputLine As IOLine In inputsChanged
                    outputDevice.Outputs(inputLine.IONumber).Value = inputLine.Value
                Next
            Next

        End If
    End Sub
End Module
