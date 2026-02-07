Imports Brainboxes.IO


Public Class Form1

	''' <summary>
	''' The brainboxes Remote IO device
	''' </summary>
	Private EDDevice As EDDevice

	Private previousCount As Integer
	Private previousDateTime As DateTime
	Public Sub New()
        InitializeComponent()

    End Sub

    Private Sub ConnectButton_Click(sender As Object, e As EventArgs) Handles ConnectButton.Click
        If (Not String.IsNullOrWhiteSpace(IPTextBox.Text)) Then

            ' already a device make sure to disconnect
            If (EDDevice IsNot Nothing) Then
                EDDevice.Disconnect()
            End If


            'create a new device with that IP address, auto-connect
            EDDevice = EDDevice.Create(IPTextBox.Text)


			'this project does not use brackground polling, but to increase the polling frequency do the following
			EDDevice.IOLineCacheTimeout = 100 ' polling interval will be half the cache timeout in ms


			'update the count value
			CountLabel.Text = EDDevice.Inputs(0).Count

			' store the start time
			previousDateTime = DateTime.UtcNow
			'get the non-volatile count on the input (this will increment on a rising or falling edge depending how you configure the device)
			previousCount = EDDevice.Inputs(0).Count

			'start the timer to update the count periodically
			InputCountTimer.Start()

            'enable the reset button
            ResetButton.Enabled = True

        End If
    End Sub

	''' <summary>
	''' update the count every second
	''' </summary>
	''' <param name="sender"></param>
	''' <param name="e"></param>
	Private Sub InputCountTimer_Tick(sender As Object, e As EventArgs) Handles InputCountTimer.Tick
		'get the count
		Dim count = EDDevice.Inputs(0).Count
		Dim dt = DateTime.UtcNow
		CountLabel.Text = count

		'how many counts have elapsed since last count
		Dim change = count - previousCount

		'whats the rate
		Dim rate = change / (dt - previousDateTime).TotalSeconds

		perSecondLabel.Text = rate

		previousCount = count
		previousDateTime = dt

	End Sub

	''' <summary>
	''' When the reset button is clicked reset IO line 0
	''' </summary>
	''' <param name="sender"></param>
	''' <param name="e"></param>
	Private Sub ResetButton_Click(sender As Object, e As EventArgs) Handles ResetButton.Click
        EDDevice.Inputs(0).ClearCount()
        CountLabel.Text = EDDevice.Inputs(0).Count
    End Sub

    Private Sub Form1_FormClosed(sender As Object, e As FormClosedEventArgs) Handles MyBase.FormClosed
        ' close any open connections
        If (EDDevice IsNot Nothing) Then
            EDDevice.Disconnect()
        End If
    End Sub
End Class
