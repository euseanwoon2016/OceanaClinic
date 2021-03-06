﻿Imports MaterialDesignThemes.Wpf
Public Class AdminPage
    Dim msgQ As New SnackbarMessageQueue(TimeSpan.FromSeconds(3))
    Dim _users As ObservableUsers
    Public Sub New()

        ' This call is required by the designer.
        InitializeComponent()

        ' Add any initialization after the InitializeComponent() call.
        _users = Me.Resources("users")
        RefreshUsers()
        MySnackbar.MessageQueue = msgQ
        DataContext = Me
    End Sub
    Private Sub Window_Loaded(sender As Object, e As RoutedEventArgs)

    End Sub
    Public Sub RefreshUsers()
        _users.Clear()
        For Each user As User In gVars.dbAdmin.GetAllUsers()
            _users.Add(user)
        Next
    End Sub
    Private Sub btnLogout_Click(sender As Object, e As RoutedEventArgs) Handles btnLogout.Click
        Dim x As MainWindow = New MainWindow
        x.Show()
        Me.Close()
    End Sub
    Private Sub btnReload_Click(sender As Object, e As RoutedEventArgs) Handles btnReload.Click
        RefreshUsers()
    End Sub
    Private Sub txtSearch_TextChanged(sender As Object, e As TextChangedEventArgs) Handles txtSearch.TextChanged
        CollectionViewSource.GetDefaultView(dgUsers.ItemsSource).Refresh()
    End Sub
    Private Async Sub btnRemoveUser_Click(sender As Object, e As RoutedEventArgs) Handles btnRemoveUser.Click
        If (dgUsers.SelectedIndex = -1) Then
            Return
        End If
        Dim result As Boolean = Await DialogHost.Show(MultiDeleteDialogBox, "RootDialog")
        Dim selectedUsers As List(Of User) = UtilityConverter.SelectedItemsToListOfUsers(dgUsers.SelectedItems)
        If result = True Then
            If gVars.dbAdmin.RemoveUsers(selectedUsers) > 0 Then
                msgQ.Enqueue("Success! Removed " + selectedUsers.Count.ToString + " users!")
            Else
                msgQ.Enqueue("Failure! Failed to remove " + selectedUsers.Count.ToString + " users!")
            End If
            RefreshUsers()
        End If
    End Sub

    Private Async Sub btnEdit_Click(sender As Object, e As RoutedEventArgs) Handles btnEdit.Click
        If dgUsers.SelectedIndex = -1 Then
            Return
        End If
        Dim selectedUser As User = New User(dgUsers.SelectedValue)
        Dim result As Boolean = Await DialogHost.Show(New UserDetails(selectedUser), "RootDialog")
        If result = True Then
            If gVars.dbAdmin.UpdateUser(selectedUser) > 0 Then
                msgQ.Enqueue("Success! User of UserID(" + selectedUser.UserID.ToString + ") updated!")
            Else
                msgQ.Enqueue("Failure! User of UserID(" + selectedUser.UserID.ToString + ") failed to be updated!")
            End If
            RefreshUsers()
        End If
    End Sub

    Private Async Sub btnAddUser_Click(sender As Object, e As RoutedEventArgs) Handles btnAddUser.Click
        Dim inUser As User = New User()
        Dim result As Boolean = Await DialogHost.Show(New AddUser(inUser), "RootDialog")
        If result = True Then
            If gVars.dbAdmin.InsertNewUser(inUser) > 0 Then
                msgQ.Enqueue("Success! New user (" + inUser.Email + ") successfully created!")
            Else
                msgQ.Enqueue("Failure! Failed to create user (" + inUser.Email + ")!")
            End If
        End If

        RefreshUsers()
    End Sub

    Private Sub dgUsers_MouseDoubleClick(sender As Object, e As MouseButtonEventArgs) Handles dgUsers.MouseDoubleClick
        If dgUsers.SelectedIndex > -1 Then
            btnEdit_Click(sender, Nothing)
        End If
    End Sub

    Private Sub Window_PreviewKeyDown(sender As Object, e As KeyEventArgs)
        If e.Key = Key.Delete Then
            btnRemoveUser_Click(sender, Nothing)
        End If
    End Sub
    'https://docs.microsoft.com/en-us/dotnet/framework/wpf/controls/how-to-group-sort-and-filter-data-in-the-datagrid-control
    Public Sub CollectionViewSource_Filter(sender As Object, e As FilterEventArgs)
        Dim u As User = e.Item
        If u IsNot Nothing Then
            If (Not String.IsNullOrEmpty(txtSearch.Text)) Then
                Dim q As String = txtSearch.Text.ToLower
                If (u.UserID.ToString.Contains(q) Or u.Firstname.ToLower.Contains(q) Or u.Lastname.ToLower.Contains(q) Or
                u.Email.ToLower.Contains(q) Or u.UserGroup.ToString.ToLower.Contains(q)) Then
                    e.Accepted = True
                Else
                    e.Accepted = False
                End If
            Else
                e.Accepted = True
            End If
        End If
    End Sub
End Class


