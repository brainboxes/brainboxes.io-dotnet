'This file is part of the Visual Basic example/library code for communication with with 
'Brainboxes Ethernet-attached data acquisition and control products, and is 
'provided by Brainboxes Limited.  Examples in other programming languages are 
'also available.
'Visit http://www.brainboxes.com to see our range of Brainboxes Ethernet-
'attached data acquisition and control products, and to check for updates to
'Me code package.

'This is free and unencumbered software released into the public domain.

Imports Brainboxes.IO
Imports System.Diagnostics
Imports System.Text.RegularExpressions

Public Class EDConsole
    Private EDDevice As EDDevice

    Public Sub New()
        InitializeComponent()
        EDDevice = New EDDevice(Nothing, New ASCIIProtocol())
        WriteLine("Choose a method to connect to the Brainboxes ED Device")
    End Sub

    ''' <summary>
    ''' Add a message to the console (which is a Windows.Form.TextBox)
    ''' Scroll the text box down to the newly added message
    ''' </summary>
    ''' <param name="message"></param>
    ''' <remarks></remarks>
    Public Sub Write(ByVal message As String)
        Me.terminal.AppendText(message)
        'scroll to bottom
        Me.terminal.SelectionStart = Me.terminal.Text.Length
        Me.terminal.ScrollToCaret()
        Me.Refresh()
    End Sub


    ''' <summary>
    ''' Same as Write but adds a newline to the end of the string
    ''' </summary>
    ''' <param name="line"></param>
    Public Sub WriteLine(ByVal line As String)
        Me.Write(line & Environment.NewLine)
    End Sub

    ''' <summary>
    ''' Attempt to connect to the ED device using the settings
    ''' specified by the user
    ''' </summary>
    Protected Sub connect()
        Me.tcpSettingsPanel.Visible = False
        Me.serialSettingsPanel.Visible = False
        Try
            WriteLine("Connecting...")
            Me.EDDevice.Connect()
            WriteLine("Connected !")
            Me.sendCommandPanel.Enabled = True
            Me.disconnectButton.Visible = True
        Catch e As Exception
            WriteLine("Connection Error: " & e.Message)
            Me.tcpSettingsPanel.Visible = True
            Me.serialSettingsPanel.Visible = True
        End Try
    End Sub

    ''' <summary>
    ''' Disconnect from the ED device
    ''' </summary>
    Protected Sub disconnect()
        Me.sendCommandPanel.Enabled = False

        Try
            WriteLine("Disconnecting...")
            Me.EDDevice.Disconnect()
            WriteLine("Disconnected !")
        Catch e As Exception
            WriteLine("Disconnect Error: " & e.Message)
        Finally
            Me.tcpSettingsPanel.Visible = True
            Me.serialSettingsPanel.Visible = True
            Me.disconnectButton.Visible = False
        End Try
    End Sub

    ''' <summary>
    ''' Try to send the command in the ASCIICommand Text box to the
    ''' ED Device and display the response
    ''' </summary>
    Private Sub sendCommand()
        Try
            Me.sendCommandButton.Enabled = False
            WriteLine("TX <== " & Me.ASCIICommand.Text)
            Dim receiveCommand As String = Me.EDDevice.SendCommand(Me.ASCIICommand.Text)
            If receiveCommand Is Nothing Then
                WriteLine("This command does not have a response.")
            Else
                WriteLine("RX ==> " & receiveCommand)
            End If
        Catch e As Exception
            WriteLine("Send Command Error: " & e.Message)
        Finally
            Me.sendCommandButton.Enabled = True
        End Try
    End Sub

    ''' <summary>
    ''' If the user clicks the "send Command" button then attempt
    ''' to send command to the ED device
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub sendCommandButton_Click(sender As Object, e As EventArgs) Handles sendCommandButton.Click
        Me.sendCommand()
    End Sub


    ''' <summary>
    ''' If the user presses the enter key when in the ASCII command text box
    ''' then attempt to send the command
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub ASCIICommand_KeyPress(sender As Object, e As KeyPressEventArgs) Handles ASCIICommand.KeyPress
        If e.KeyChar = Microsoft.VisualBasic.ChrW(Keys.Return) Then
            Me.sendCommand()
        End If
    End Sub


    ''' <summary>
    ''' Attempt to connect to the ED device over TCP using the settings
    ''' supplied
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub tcpConnectButton_Click(sender As Object, e As EventArgs) Handles tcpConnectButton.Click
        WriteLine("")
        WriteLine("Connecting over TCP IP")
        Dim IPAddress As String = ipAddressTextBox1.Text & "." & ipAddressTextBox2.Text & "." & ipAddressTextBox3.Text & "." & ipAddressTextBox4.Text
        Dim port As Integer = Convert.ToInt32(portNumberTextBox.Text)
        WriteLine("Address: " & IPAddress & ":" & port.ToString())
        WriteLine("Default connection timeout if address not found: 20 seconds")
        Me.EDDevice.Connection = New TCPConnection(IPAddress, port)
        Me.connect()
    End Sub


    ''' <summary>
    ''' Attempt to connect over serial com port
    ''' note the ED device virtual com must be installed
    ''' using Brainboxes Boost.IO Manager
    ''' </summary>
    ''' <param name="sender"></param>
    ''' <param name="e"></param>
    Private Sub serialConnectButton_Click(sender As Object, e As EventArgs) Handles serialConnectButton.Click
        WriteLine("")
        WriteLine("Connecting over Serial")
        Dim comText As String = If(comPortComboBox.SelectedItem <> Nothing, comPortComboBox.SelectedItem.ToString(), comPortComboBox.Text)
        WriteLine(comText)
        Me.EDDevice.Connection = New SerialConnection(comText, 115200)
        Me.connect()
    End Sub

    Private Sub disconnectButton_Click(sender As Object, e As EventArgs) Handles disconnectButton.Click
        Me.disconnect()
    End Sub

    Private Sub comPortComboBox_DropDown(sender As Object, e As EventArgs) Handles comPortComboBox.DropDown
        'list all valid system com ports
        Me.comPortComboBox.DataSource = System.IO.Ports.SerialPort.GetPortNames()
    End Sub

    Private Sub comPortComboBox_KeyPress(sender As Object, e As KeyPressEventArgs) Handles comPortComboBox.KeyPress
        If e.KeyChar = Microsoft.VisualBasic.ChrW(Keys.Return) Then
            Me.serialConnectButton_Click(sender, e)
        End If
    End Sub
End Class
