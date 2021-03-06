﻿Imports MaterialDesignThemes.Wpf
Public Class ReceptionistPage
    Dim msgQ As New SnackbarMessageQueue(TimeSpan.FromSeconds(3))
    Dim _patients As ObservablePatients
    Dim MainPage As Receptionist
    Sub New(_mainPage As Receptionist)

        ' This call is required by the designer.
        InitializeComponent()
        ' Add any initialization after the InitializeComponent() call.
        _patients = Me.Resources("patients")
        MainPage = _mainPage
        RefreshPatients()
        MySnackbar.MessageQueue = msgQ
        DataContext = Me
    End Sub
    Public Sub btnLogout_Click(sender As Object, e As RoutedEventArgs) Handles btnLogout.Click
        Dim x As MainWindow = New MainWindow
        x.Show()
        Window.GetWindow(Me).Close()
    End Sub
    Public Sub RefreshPatients()
        _patients.Clear()
        For Each patient As Patient In gVars.dbReception.GetAllPatients()
            _patients.Add(patient)
        Next
    End Sub
    Private Sub txtSearch_TextChanged(sender As Object, e As TextChangedEventArgs) Handles txtSearch.TextChanged
        CollectionViewSource.GetDefaultView(dgPatients.ItemsSource).Refresh()
    End Sub

    Private Sub btnReload_Click(sender As Object, e As RoutedEventArgs) Handles btnReload.Click
        RefreshPatients()
    End Sub
    'https://docs.microsoft.com/en-us/dotnet/framework/wpf/controls/how-to-group-sort-and-filter-data-in-the-datagrid-control
    Public Sub CollectionViewSource_Filter(sender As Object, e As FilterEventArgs)
        Dim p As Patient = e.Item
        If p IsNot Nothing Then
            If (Not String.IsNullOrEmpty(txtSearch.Text)) Then
                Dim q As String = txtSearch.Text.ToLower
                If (p.PatientId.ToString.Contains(q) Or p.Firstname.ToLower.Contains(q) Or p.Lastname.ToLower.Contains(q) Or
                p.Identity.Contains(q) Or p.Mobile.Contains(q) Or p.Address.ToLower.Contains(q) Or p.Email.ToLower.Contains(q) Or
                p.Weight.ToString.Contains(q) Or p.Height.ToString.Contains(q) Or p.BloodType.ToString.ToLower.Contains(q) Or
                p.Allergies.ToLower.Contains(q)) Then
                    e.Accepted = True
                Else
                    e.Accepted = False
                End If
            Else
                e.Accepted = True
            End If
        End If
    End Sub

    Private Sub dgPatients_MouseDoubleClick(sender As Object, e As MouseButtonEventArgs) Handles dgPatients.MouseDoubleClick
        btnUpdatePatient_Click(sender, Nothing)
    End Sub
    Private Async Sub btnAddPatient_Click(sender As Object, e As RoutedEventArgs) Handles btnAddPatient.Click
        Dim inPatient As Patient = New Patient()
        Dim result As Boolean = Await DialogHost.Show(New AddPatient(inPatient), "RootDialog")
        If result = True Then
            If gVars.dbReception.InsertNewPatient(inPatient) > 0 Then
                msgQ.Enqueue("Success! New patient (" + inPatient.Identity + ") profile created!")
            Else
                msgQ.Enqueue("Failure! New patient (" + inPatient.Identity + ") failed to be created!")
            End If
            RefreshPatients()
        End If
    End Sub

    Private Async Sub btnUpdatePatient_Click(sender As Object, e As RoutedEventArgs) Handles btnUpdatePatient.Click
        If dgPatients.SelectedIndex = -1 Then
            Return
        End If
        Dim selectedPatient As Patient = New Patient(dgPatients.SelectedValue)
        Dim result As Boolean = Await DialogHost.Show(New PatientDetails(selectedPatient), "RootDialog")
        If result = True Then
            If gVars.dbReception.UpdatePatient(selectedPatient) > 0 Then
                msgQ.Enqueue("Success! Patient of PatientID(" + selectedPatient.PatientId.ToString + ") updated!")
            Else
                msgQ.Enqueue("Failure! Patient of PatientID(" + selectedPatient.PatientId.ToString + ") failed to be updated!")
            End If
            RefreshPatients()
        End If
    End Sub

    Private Sub btnBilling_Click(sender As Object, e As RoutedEventArgs) Handles btnBilling.Click
        Dim billPg As BillingPage
        Dim patId = 0
        If dgPatients.SelectedIndex <> -1 Then
            Dim selectedPatient As Patient = New Patient(dgPatients.SelectedValue)
            patId = selectedPatient.PatientId
        End If

        If billPg Is Nothing Then
            billPg = New BillingPage(MainPage, patId)
        End If
        MainPage.ReceptionistControl.Content = billPg
    End Sub
End Class