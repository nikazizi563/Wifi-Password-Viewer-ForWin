Imports System.Net.NetworkInformation
Imports System.Text.RegularExpressions
Imports QRCoder
Public Class Form1

    Private Function IsConnectedToWifi() As Boolean
        Dim interfaces As NetworkInterface() = NetworkInterface.GetAllNetworkInterfaces()

        For Each iface As NetworkInterface In interfaces
            If iface.NetworkInterfaceType = NetworkInterfaceType.Wireless80211 AndAlso iface.OperationalStatus = OperationalStatus.Up Then
                Return True
            End If
        Next

        Return False
    End Function
    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        MessageBox.Show("This App will show your own wifi password" & vbCrLf & "It wont show someone else wifi password", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information)
        If IsConnectedToWifi() Then
            Try
                Dim selectedProfile As String = ExtractSSID()
                Dim selectedAuth As String = GetAuthenticationType(selectedProfile)
                Dim password As String = ExtractPassword(selectedProfile)
                Dim data As String = "WIFI:S:" & selectedProfile & ";T:" & selectedAuth & ";P:" & password & ";;"
                Dim qrGenerator As QRCodeGenerator = New QRCodeGenerator()
                Dim qrCodeData As QRCodeData = qrGenerator.CreateQrCode(data, QRCodeGenerator.ECCLevel.L)
                Dim qrCode As QRCode = New QRCode(qrCodeData)
                Dim qrCodeImage As Bitmap = qrCode.GetGraphic(8)

                PictureBox1.SizeMode = PictureBoxSizeMode.CenterImage
                PictureBox1.Image = qrCodeImage

                ' Display the password in TextBox1
                TextBox1.Text = password
                TextBox2.Text = selectedProfile
            Catch ex As Exception
                MessageBox.Show("An error occurred: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End Try
        Else
            MessageBox.Show("You are not connected to a WiFi network.")
        End If
    End Sub

    Function RunNetshCommand(command As String) As String
        Dim processInfo As New ProcessStartInfo("netsh", command)
        processInfo.RedirectStandardOutput = True
        processInfo.UseShellExecute = False
        processInfo.CreateNoWindow = True

        Dim process As New Process()
        process.StartInfo = processInfo
        process.Start()

        Dim output As String = process.StandardOutput.ReadToEnd()
        process.WaitForExit()

        Return output
    End Function

    Private Function ExtractProfileNames(output As String) As List(Of String)
        Dim profileNames As New List(Of String)

        ' Use a regular expression to match profile names
        Dim regex As New Regex("All User Profile\s+:\s+(.+)")
        Dim matches As MatchCollection = regex.Matches(output)

        ' Extract profile names from matches
        For Each match As Match In matches
            If match.Groups.Count > 1 Then
                Dim profileName As String = match.Groups(1).Value
                profileNames.Add(profileName)
            End If
        Next

        Return profileNames
    End Function

    Private Function ExtractSSID() As String
        Dim output As String = RunNetshCommand("wlan show interfaces")
        ' Use a regular expression to match the Key Content line
        Dim regexSSID As New Regex("SSID\s+:\s+(.+)", RegexOptions.IgnoreCase)
        Dim matchSSID As Match = regexSSID.Match(output)

        ' Check if there is a match
        If matchSSID.Success Then
            ' Check if the matched group contains the password
            If matchSSID.Groups.Count > 1 Then
                ' Return the password
                Return matchSSID.Groups(1).Value.Trim()
            Else
                ' Return a message indicating that the password was found but the group is missing
                Return "Password found, but group missing"
            End If
        Else
            ' Return a message indicating that the password was not found
            Return "Password not found"
        End If
    End Function

    Private Function GetAuthenticationType(ssid As String) As String
        Dim output As String = RunNetshCommand("wlan show interfaces")

        ' Use regular expressions to find the authentication type
        Dim regex As New Regex("Authentication\s+:\s+(.+)", RegexOptions.IgnoreCase)
        Dim match As Match = regex.Match(output)

        If match.Success Then
            Dim authenticationType As String = match.Groups(1).Value.Trim()

            ' Determine the simplified authentication type
            If authenticationType.Contains("WPA") Then
                Return "WPA"
            ElseIf authenticationType.Contains("WEP") Then
                Return "WEP"
            Else
                Return "None"
            End If
        Else
            ' Return "Unknown" if authentication information is not found
            Return "Unknown"
        End If
    End Function
    Private Function ExtractPassword(ssid As String) As String
        ' Use a regular expression to match the Key Content line
        Dim output As String = RunNetshCommand("wlan show profiles """ & ssid & """ key=clear")
        Dim regex As New Regex("Key Content\s+:\s+(.+)", RegexOptions.IgnoreCase)
        Dim match As Match = regex.Match(output)

        ' Check if there is a match
        If match.Success Then
            ' Check if the matched group contains the password
            If match.Groups.Count > 1 Then
                ' Return the password
                Return match.Groups(1).Value.Trim()
            Else
                ' Return a message indicating that the password was found but the group is missing
                Return "Password found, but group missing"
            End If
        Else
            ' Return a message indicating that the password was not found
            Return "Password not found"
        End If
    End Function

    Private Sub Button2_Click(sender As Object, e As EventArgs) Handles Button2.Click
        ' Check if TextBox2 has text before copying to clipboard
        If Not String.IsNullOrEmpty(TextBox1.Text) Then
            ' Copy text to clipboard
            Clipboard.SetText(TextBox1.Text)

            ' Optionally, provide feedback to the user
            MessageBox.Show("Password copied to clipboard!", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information)
        Else
            ' Notify the user if TextBox1 is empty
            MessageBox.Show("TextBox is empty. Nothing to copy.", "Info", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End If
    End Sub

End Class
