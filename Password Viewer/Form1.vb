Imports System.Text.RegularExpressions

Public Class Form1
    Private Sub Form1_Load(sender As Object, e As EventArgs) Handles MyBase.Load
        ' Run the netsh command and populate ListBox with profile names
        PopulateProfileList()
    End Sub

    Sub PopulateProfileList()
        ' Run the netsh command and capture the output
        Dim output As String = RunNetshCommand("wlan show profiles")

        ' Extract profile names using regular expression
        Dim profileNames As List(Of String) = ExtractProfileNames(output)

        ' Populate ListBox with profile names
        ComboBox1.Items.AddRange(profileNames.ToArray())
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

    Function ExtractProfileNames(output As String) As List(Of String)
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

    Private Sub Button1_Click(sender As Object, e As EventArgs) Handles Button1.Click
        CheckKey()
    End Sub

    Sub CheckKey()
        Try
            ' Get the selected profile name from ComboBox
            If ComboBox1.SelectedItem IsNot Nothing Then
                Dim selectedProfile As String = ComboBox1.SelectedItem.ToString().Trim()
                Dim output As String = RunNetshCommand("wlan show profiles """ & selectedProfile & """ key=clear")

                ' Extract password using regular expression
                Dim password As String = ExtractPassword(output)

                ' Display the password in TextBox1
                TextBox1.Text = password
            Else
                MessageBox.Show("Please select a profile before checking the key.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
            End If
        Catch ex As Exception
            MessageBox.Show("An error occurred: " & ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error)
        End Try
    End Sub


    Function RunKeyCommand(command As String) As String
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
    Function ExtractPassword(output As String) As String
        ' Use a regular expression to match the Key Content line
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
            MessageBox.Show("Password copied to clipboard!", "App Made by NikGG", MessageBoxButtons.OK, MessageBoxIcon.Information)
        Else
            ' Notify the user if TextBox1 is empty
            MessageBox.Show("TextBox is empty. Nothing to copy.", "App Made by NikGG", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End If
    End Sub

End Class
