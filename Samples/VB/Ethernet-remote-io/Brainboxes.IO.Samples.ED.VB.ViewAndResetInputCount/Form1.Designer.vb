<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class Form1
    Inherits System.Windows.Forms.Form

    'Form overrides dispose to clean up the component list.
    <System.Diagnostics.DebuggerNonUserCode()> _
    Protected Overrides Sub Dispose(ByVal disposing As Boolean)
        Try
            If disposing AndAlso components IsNot Nothing Then
                components.Dispose()
            End If
        Finally
            MyBase.Dispose(disposing)
        End Try
    End Sub

    'Required by the Windows Form Designer
    Private components As System.ComponentModel.IContainer

    'NOTE: The following procedure is required by the Windows Form Designer
    'It can be modified using the Windows Form Designer.  
    'Do not modify it using the code editor.
    <System.Diagnostics.DebuggerStepThrough()> _
    Private Sub InitializeComponent()
		Me.components = New System.ComponentModel.Container()
		Me.Label1 = New System.Windows.Forms.Label()
		Me.ResetButton = New System.Windows.Forms.Button()
		Me.IPTextBox = New System.Windows.Forms.TextBox()
		Me.Label2 = New System.Windows.Forms.Label()
		Me.CountLabel = New System.Windows.Forms.Label()
		Me.ConnectButton = New System.Windows.Forms.Button()
		Me.InputCountTimer = New System.Windows.Forms.Timer(Me.components)
		Me.perSecondLabel = New System.Windows.Forms.Label()
		Me.Label4 = New System.Windows.Forms.Label()
		Me.SuspendLayout()
		'
		'Label1
		'
		Me.Label1.AutoSize = True
		Me.Label1.Location = New System.Drawing.Point(24, 56)
		Me.Label1.Margin = New System.Windows.Forms.Padding(6, 0, 6, 0)
		Me.Label1.Name = "Label1"
		Me.Label1.Size = New System.Drawing.Size(103, 25)
		Me.Label1.TabIndex = 0
		Me.Label1.Text = "Device IP"
		'
		'ResetButton
		'
		Me.ResetButton.Enabled = False
		Me.ResetButton.Location = New System.Drawing.Point(400, 129)
		Me.ResetButton.Margin = New System.Windows.Forms.Padding(6, 6, 6, 6)
		Me.ResetButton.Name = "ResetButton"
		Me.ResetButton.Size = New System.Drawing.Size(150, 44)
		Me.ResetButton.TabIndex = 1
		Me.ResetButton.Text = "RESET"
		Me.ResetButton.UseVisualStyleBackColor = True
		'
		'IPTextBox
		'
		Me.IPTextBox.Location = New System.Drawing.Point(174, 50)
		Me.IPTextBox.Margin = New System.Windows.Forms.Padding(6, 6, 6, 6)
		Me.IPTextBox.Name = "IPTextBox"
		Me.IPTextBox.Size = New System.Drawing.Size(196, 31)
		Me.IPTextBox.TabIndex = 2
		'
		'Label2
		'
		Me.Label2.AutoSize = True
		Me.Label2.Location = New System.Drawing.Point(24, 138)
		Me.Label2.Margin = New System.Windows.Forms.Padding(6, 0, 6, 0)
		Me.Label2.Name = "Label2"
		Me.Label2.Size = New System.Drawing.Size(59, 25)
		Me.Label2.TabIndex = 3
		Me.Label2.Text = "DIN0"
		'
		'CountLabel
		'
		Me.CountLabel.AutoSize = True
		Me.CountLabel.Location = New System.Drawing.Point(168, 138)
		Me.CountLabel.Margin = New System.Windows.Forms.Padding(6, 0, 6, 0)
		Me.CountLabel.Name = "CountLabel"
		Me.CountLabel.Size = New System.Drawing.Size(86, 25)
		Me.CountLabel.TabIndex = 4
		Me.CountLabel.Text = "COUNT"
		'
		'ConnectButton
		'
		Me.ConnectButton.Location = New System.Drawing.Point(400, 46)
		Me.ConnectButton.Margin = New System.Windows.Forms.Padding(6, 6, 6, 6)
		Me.ConnectButton.Name = "ConnectButton"
		Me.ConnectButton.Size = New System.Drawing.Size(150, 44)
		Me.ConnectButton.TabIndex = 5
		Me.ConnectButton.Text = "CONNECT"
		Me.ConnectButton.UseVisualStyleBackColor = True
		'
		'InputCountTimer
		'
		Me.InputCountTimer.Interval = 1000
		'
		'perSecondLabel
		'
		Me.perSecondLabel.AutoSize = True
		Me.perSecondLabel.Location = New System.Drawing.Point(395, 226)
		Me.perSecondLabel.Name = "perSecondLabel"
		Me.perSecondLabel.Size = New System.Drawing.Size(24, 25)
		Me.perSecondLabel.TabIndex = 6
		Me.perSecondLabel.Text = "0"
		'
		'Label4
		'
		Me.Label4.AutoSize = True
		Me.Label4.Location = New System.Drawing.Point(24, 226)
		Me.Label4.Name = "Label4"
		Me.Label4.Size = New System.Drawing.Size(261, 25)
		Me.Label4.TabIndex = 7
		Me.Label4.Text = "Falling Edges Per Second"
		'
		'Form1
		'
		Me.AutoScaleDimensions = New System.Drawing.SizeF(12.0!, 25.0!)
		Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
		Me.ClientSize = New System.Drawing.Size(574, 349)
		Me.Controls.Add(Me.Label4)
		Me.Controls.Add(Me.perSecondLabel)
		Me.Controls.Add(Me.ConnectButton)
		Me.Controls.Add(Me.CountLabel)
		Me.Controls.Add(Me.Label2)
		Me.Controls.Add(Me.IPTextBox)
		Me.Controls.Add(Me.ResetButton)
		Me.Controls.Add(Me.Label1)
		Me.Margin = New System.Windows.Forms.Padding(6, 6, 6, 6)
		Me.Name = "Form1"
		Me.Text = "Input Count Reset"
		Me.ResumeLayout(False)
		Me.PerformLayout()

	End Sub

	Friend WithEvents Label1 As Label
    Friend WithEvents ResetButton As Button
    Friend WithEvents IPTextBox As TextBox
    Friend WithEvents Label2 As Label
    Friend WithEvents CountLabel As Label
    Friend WithEvents ConnectButton As Button
    Friend WithEvents InputCountTimer As Timer
	Friend WithEvents perSecondLabel As Label
	Friend WithEvents Label4 As Label
End Class
