<Global.Microsoft.VisualBasic.CompilerServices.DesignerGenerated()> _
Partial Class EDConsole
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
        Me.sendCommandPanel = New System.Windows.Forms.Panel()
        Me.ASCIICommand = New System.Windows.Forms.TextBox()
        Me.sendCommandButton = New System.Windows.Forms.Button()
        Me.serialConnectButton = New System.Windows.Forms.Button()
        Me.comPortComboBox = New System.Windows.Forms.ComboBox()
        Me.label6 = New System.Windows.Forms.Label()
        Me.tcpConnectButton = New System.Windows.Forms.Button()
        Me.label9 = New System.Windows.Forms.Label()
        Me.label8 = New System.Windows.Forms.Label()
        Me.label7 = New System.Windows.Forms.Label()
        Me.ipAddressTextBox4 = New System.Windows.Forms.TextBox()
        Me.ipAddressTextBox3 = New System.Windows.Forms.TextBox()
        Me.ipAddressTextBox2 = New System.Windows.Forms.TextBox()
        Me.serialSettingsPanel = New System.Windows.Forms.Panel()
        Me.disconnectButton = New System.Windows.Forms.Button()
        Me.label5 = New System.Windows.Forms.Label()
        Me.label3 = New System.Windows.Forms.Label()
        Me.portNumberTextBox = New System.Windows.Forms.TextBox()
        Me.label1 = New System.Windows.Forms.Label()
        Me.ipAddressTextBox1 = New System.Windows.Forms.TextBox()
        Me.terminal = New System.Windows.Forms.TextBox()
        Me.tcpSettingsPanel = New System.Windows.Forms.Panel()
        Me.label2 = New System.Windows.Forms.Label()
        Me.sendCommandPanel.SuspendLayout()
        Me.serialSettingsPanel.SuspendLayout()
        Me.tcpSettingsPanel.SuspendLayout()
        Me.SuspendLayout()
        '
        'sendCommandPanel
        '
        Me.sendCommandPanel.Controls.Add(Me.ASCIICommand)
        Me.sendCommandPanel.Controls.Add(Me.sendCommandButton)
        Me.sendCommandPanel.Enabled = False
        Me.sendCommandPanel.Location = New System.Drawing.Point(26, 402)
        Me.sendCommandPanel.Name = "sendCommandPanel"
        Me.sendCommandPanel.Size = New System.Drawing.Size(540, 40)
        Me.sendCommandPanel.TabIndex = 103
        '
        'ASCIICommand
        '
        Me.ASCIICommand.AutoCompleteCustomSource.AddRange(New String() {"""$01C""", """@0101"""})
        Me.ASCIICommand.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource
        Me.ASCIICommand.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper
        Me.ASCIICommand.Font = New System.Drawing.Font("Microsoft Sans Serif", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.ASCIICommand.Location = New System.Drawing.Point(0, 3)
        Me.ASCIICommand.Name = "ASCIICommand"
        Me.ASCIICommand.Size = New System.Drawing.Size(300, 29)
        Me.ASCIICommand.TabIndex = 8
        Me.ASCIICommand.Text = "$01M"
        '
        'sendCommandButton
        '
        Me.sendCommandButton.Font = New System.Drawing.Font("Microsoft Sans Serif", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.sendCommandButton.Location = New System.Drawing.Point(306, 2)
        Me.sendCommandButton.Name = "sendCommandButton"
        Me.sendCommandButton.Size = New System.Drawing.Size(231, 32)
        Me.sendCommandButton.TabIndex = 9
        Me.sendCommandButton.Text = "Send Command"
        Me.sendCommandButton.UseVisualStyleBackColor = True
        '
        'serialConnectButton
        '
        Me.serialConnectButton.Font = New System.Drawing.Font("Microsoft Sans Serif", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.serialConnectButton.Location = New System.Drawing.Point(306, 18)
        Me.serialConnectButton.Name = "serialConnectButton"
        Me.serialConnectButton.Size = New System.Drawing.Size(231, 32)
        Me.serialConnectButton.TabIndex = 7
        Me.serialConnectButton.Text = "Connect Over Serial"
        Me.serialConnectButton.UseVisualStyleBackColor = True
        '
        'comPortComboBox
        '
        Me.comPortComboBox.FormattingEnabled = True
        Me.comPortComboBox.Items.AddRange(New Object() {"COM3"})
        Me.comPortComboBox.Location = New System.Drawing.Point(63, 27)
        Me.comPortComboBox.Name = "comPortComboBox"
        Me.comPortComboBox.Size = New System.Drawing.Size(121, 21)
        Me.comPortComboBox.TabIndex = 6
        '
        'label6
        '
        Me.label6.AutoSize = True
        Me.label6.Location = New System.Drawing.Point(6, 29)
        Me.label6.Name = "label6"
        Me.label6.Size = New System.Drawing.Size(53, 13)
        Me.label6.TabIndex = 6
        Me.label6.Text = "Com Port:"
        '
        'tcpConnectButton
        '
        Me.tcpConnectButton.Font = New System.Drawing.Font("Microsoft Sans Serif", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.tcpConnectButton.Location = New System.Drawing.Point(306, 20)
        Me.tcpConnectButton.Name = "tcpConnectButton"
        Me.tcpConnectButton.Size = New System.Drawing.Size(231, 32)
        Me.tcpConnectButton.TabIndex = 5
        Me.tcpConnectButton.Text = "Connect Over TCP"
        Me.tcpConnectButton.UseVisualStyleBackColor = True
        '
        'label9
        '
        Me.label9.AutoSize = True
        Me.label9.Location = New System.Drawing.Point(164, 32)
        Me.label9.Name = "label9"
        Me.label9.Size = New System.Drawing.Size(10, 13)
        Me.label9.TabIndex = 10
        Me.label9.Text = "."
        '
        'label8
        '
        Me.label8.AutoSize = True
        Me.label8.Location = New System.Drawing.Point(116, 32)
        Me.label8.Name = "label8"
        Me.label8.Size = New System.Drawing.Size(10, 13)
        Me.label8.TabIndex = 9
        Me.label8.Text = "."
        '
        'label7
        '
        Me.label7.AutoSize = True
        Me.label7.Location = New System.Drawing.Point(68, 32)
        Me.label7.Name = "label7"
        Me.label7.Size = New System.Drawing.Size(10, 13)
        Me.label7.TabIndex = 8
        Me.label7.Text = "."
        '
        'ipAddressTextBox4
        '
        Me.ipAddressTextBox4.Location = New System.Drawing.Point(180, 28)
        Me.ipAddressTextBox4.Name = "ipAddressTextBox4"
        Me.ipAddressTextBox4.Size = New System.Drawing.Size(29, 20)
        Me.ipAddressTextBox4.TabIndex = 3
        Me.ipAddressTextBox4.Text = "255"
        '
        'ipAddressTextBox3
        '
        Me.ipAddressTextBox3.Location = New System.Drawing.Point(132, 28)
        Me.ipAddressTextBox3.Name = "ipAddressTextBox3"
        Me.ipAddressTextBox3.Size = New System.Drawing.Size(29, 20)
        Me.ipAddressTextBox3.TabIndex = 2
        Me.ipAddressTextBox3.Text = "127"
        '
        'ipAddressTextBox2
        '
        Me.ipAddressTextBox2.Location = New System.Drawing.Point(84, 28)
        Me.ipAddressTextBox2.Name = "ipAddressTextBox2"
        Me.ipAddressTextBox2.Size = New System.Drawing.Size(29, 20)
        Me.ipAddressTextBox2.TabIndex = 1
        Me.ipAddressTextBox2.Text = "168"
        '
        'serialSettingsPanel
        '
        Me.serialSettingsPanel.Controls.Add(Me.serialConnectButton)
        Me.serialSettingsPanel.Controls.Add(Me.comPortComboBox)
        Me.serialSettingsPanel.Controls.Add(Me.label6)
        Me.serialSettingsPanel.Controls.Add(Me.label5)
        Me.serialSettingsPanel.Location = New System.Drawing.Point(26, 86)
        Me.serialSettingsPanel.Name = "serialSettingsPanel"
        Me.serialSettingsPanel.Size = New System.Drawing.Size(540, 60)
        Me.serialSettingsPanel.TabIndex = 102
        '
        'disconnectButton
        '
        Me.disconnectButton.Font = New System.Drawing.Font("Microsoft Sans Serif", 14.25!, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.disconnectButton.Location = New System.Drawing.Point(12, 24)
        Me.disconnectButton.Name = "disconnectButton"
        Me.disconnectButton.Size = New System.Drawing.Size(566, 132)
        Me.disconnectButton.TabIndex = 104
        Me.disconnectButton.Text = "Disconnect Brainboxes ED Device"
        Me.disconnectButton.UseVisualStyleBackColor = True
        Me.disconnectButton.Visible = False
        '
        'label5
        '
        Me.label5.AutoSize = True
        Me.label5.Font = New System.Drawing.Font("Microsoft Sans Serif", 12.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.label5.Location = New System.Drawing.Point(4, 3)
        Me.label5.Name = "label5"
        Me.label5.Size = New System.Drawing.Size(127, 20)
        Me.label5.TabIndex = 6
        Me.label5.Text = "Serial Settings"
        '
        'label3
        '
        Me.label3.AutoSize = True
        Me.label3.Font = New System.Drawing.Font("Microsoft Sans Serif", 12.0!, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, CType(0, Byte))
        Me.label3.Location = New System.Drawing.Point(3, 2)
        Me.label3.Name = "label3"
        Me.label3.Size = New System.Drawing.Size(114, 20)
        Me.label3.TabIndex = 4
        Me.label3.Text = "TCP Settings"
        '
        'portNumberTextBox
        '
        Me.portNumberTextBox.Location = New System.Drawing.Point(250, 28)
        Me.portNumberTextBox.Name = "portNumberTextBox"
        Me.portNumberTextBox.Size = New System.Drawing.Size(50, 20)
        Me.portNumberTextBox.TabIndex = 4
        Me.portNumberTextBox.Text = "9500"
        '
        'label1
        '
        Me.label1.AutoSize = True
        Me.label1.Location = New System.Drawing.Point(16, 32)
        Me.label1.Name = "label1"
        Me.label1.Size = New System.Drawing.Size(20, 13)
        Me.label1.TabIndex = 1
        Me.label1.Text = "IP:"
        '
        'ipAddressTextBox1
        '
        Me.ipAddressTextBox1.Location = New System.Drawing.Point(39, 28)
        Me.ipAddressTextBox1.Name = "ipAddressTextBox1"
        Me.ipAddressTextBox1.Size = New System.Drawing.Size(29, 20)
        Me.ipAddressTextBox1.TabIndex = 0
        Me.ipAddressTextBox1.Text = "192"
        '
        'terminal
        '
        Me.terminal.BackColor = System.Drawing.Color.Black
        Me.terminal.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle
        Me.terminal.ForeColor = System.Drawing.Color.White
        Me.terminal.Location = New System.Drawing.Point(26, 162)
        Me.terminal.Multiline = True
        Me.terminal.Name = "terminal"
        Me.terminal.ReadOnly = True
        Me.terminal.ScrollBars = System.Windows.Forms.ScrollBars.Vertical
        Me.terminal.Size = New System.Drawing.Size(540, 234)
        Me.terminal.TabIndex = 105
        '
        'tcpSettingsPanel
        '
        Me.tcpSettingsPanel.Controls.Add(Me.tcpConnectButton)
        Me.tcpSettingsPanel.Controls.Add(Me.label9)
        Me.tcpSettingsPanel.Controls.Add(Me.label8)
        Me.tcpSettingsPanel.Controls.Add(Me.label7)
        Me.tcpSettingsPanel.Controls.Add(Me.ipAddressTextBox4)
        Me.tcpSettingsPanel.Controls.Add(Me.ipAddressTextBox3)
        Me.tcpSettingsPanel.Controls.Add(Me.ipAddressTextBox2)
        Me.tcpSettingsPanel.Controls.Add(Me.label3)
        Me.tcpSettingsPanel.Controls.Add(Me.portNumberTextBox)
        Me.tcpSettingsPanel.Controls.Add(Me.label2)
        Me.tcpSettingsPanel.Controls.Add(Me.label1)
        Me.tcpSettingsPanel.Controls.Add(Me.ipAddressTextBox1)
        Me.tcpSettingsPanel.Location = New System.Drawing.Point(26, 19)
        Me.tcpSettingsPanel.Name = "tcpSettingsPanel"
        Me.tcpSettingsPanel.Size = New System.Drawing.Size(540, 61)
        Me.tcpSettingsPanel.TabIndex = 101
        '
        'label2
        '
        Me.label2.AutoSize = True
        Me.label2.Location = New System.Drawing.Point(215, 31)
        Me.label2.Name = "label2"
        Me.label2.Size = New System.Drawing.Size(29, 13)
        Me.label2.TabIndex = 2
        Me.label2.Text = "Port:"
        '
        'EDConsole
        '
        Me.AutoScaleDimensions = New System.Drawing.SizeF(6.0!, 13.0!)
        Me.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font
        Me.ClientSize = New System.Drawing.Size(590, 460)
        Me.Controls.Add(Me.sendCommandPanel)
        Me.Controls.Add(Me.serialSettingsPanel)
        Me.Controls.Add(Me.disconnectButton)
        Me.Controls.Add(Me.terminal)
        Me.Controls.Add(Me.tcpSettingsPanel)
        Me.Name = "EDConsole"
        Me.Text = "ED Console"
        Me.sendCommandPanel.ResumeLayout(False)
        Me.sendCommandPanel.PerformLayout()
        Me.serialSettingsPanel.ResumeLayout(False)
        Me.serialSettingsPanel.PerformLayout()
        Me.tcpSettingsPanel.ResumeLayout(False)
        Me.tcpSettingsPanel.PerformLayout()
        Me.ResumeLayout(False)
        Me.PerformLayout()

    End Sub
    Private WithEvents sendCommandPanel As System.Windows.Forms.Panel
    Private WithEvents ASCIICommand As System.Windows.Forms.TextBox
    Private WithEvents sendCommandButton As System.Windows.Forms.Button
    Private WithEvents serialConnectButton As System.Windows.Forms.Button
    Private WithEvents comPortComboBox As System.Windows.Forms.ComboBox
    Private WithEvents label6 As System.Windows.Forms.Label
    Private WithEvents tcpConnectButton As System.Windows.Forms.Button
    Private WithEvents label9 As System.Windows.Forms.Label
    Private WithEvents label8 As System.Windows.Forms.Label
    Private WithEvents label7 As System.Windows.Forms.Label
    Private WithEvents ipAddressTextBox4 As System.Windows.Forms.TextBox
    Private WithEvents ipAddressTextBox3 As System.Windows.Forms.TextBox
    Private WithEvents ipAddressTextBox2 As System.Windows.Forms.TextBox
    Private WithEvents serialSettingsPanel As System.Windows.Forms.Panel
    Private WithEvents label5 As System.Windows.Forms.Label
    Private WithEvents disconnectButton As System.Windows.Forms.Button
    Private WithEvents label3 As System.Windows.Forms.Label
    Private WithEvents portNumberTextBox As System.Windows.Forms.TextBox
    Private WithEvents label1 As System.Windows.Forms.Label
    Private WithEvents ipAddressTextBox1 As System.Windows.Forms.TextBox
    Private WithEvents terminal As System.Windows.Forms.TextBox
    Private WithEvents tcpSettingsPanel As System.Windows.Forms.Panel
    Private WithEvents label2 As System.Windows.Forms.Label

End Class
