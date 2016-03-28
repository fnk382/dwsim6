'    Copyright 2008-2014 Daniel Wagner O. de Medeiros
'
'    This file is part of DWSIM.
'
'    DWSIM is free software: you can redistribute it and/or modify
'    it under the terms of the GNU General Public License as published by
'    the Free Software Foundation, either version 3 of the License, or
'    (at your option) any later version.
'
'    DWSIM is distributed in the hope that it will be useful,
'    but WITHOUT ANY WARRANTY; without even the implied warranty of
'    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
'    GNU General Public License for more details.
'
'    You should have received a copy of the GNU General Public License
'    along with DWSIM.  If not, see <http://www.gnu.org/licenses/>.

Imports System.Linq

Public Class FormHYD

    Inherits WeifenLuo.WinFormsUI.Docking.DockContent

    Public m_aux As DWSIM.Utilities.HYD.AuxMethods

    Dim mat As DWSIM.SimulationObjects.Streams.MaterialStream
    Dim Frm As FormFlowsheet

    Public su As New SystemsOfUnits.Units
    Public cv As New SystemsOfUnits.Converter
    Public nf As String

    Dim resPC, resTC As Object
    Dim tipoPC, tipoTC As String, nomesglobal() As String

    Private Sub FormHYD_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

        Me.Text = DWSIM.App.GetLocalString("DWSIMUtilitriosCondi")

        Me.TabText = Me.Text

        If Not Me.DockHandler Is Nothing OrElse Not Me.DockHandler.FloatPane Is Nothing Then
            ' set the bounds of this form's FloatWindow to our desired position and size
            If Me.DockState = WeifenLuo.WinFormsUI.Docking.DockState.Float Then
                Dim floatWin = Me.DockHandler.FloatPane.FloatWindow
                If Not floatWin Is Nothing Then
                    floatWin.SetBounds(floatWin.Location.X, floatWin.Location.Y, 544, 443)
                End If
            End If
        End If

        Me.ComboBox1.SelectedIndex = 0

        GroupBox1.Enabled = False

        Me.m_aux = New DWSIM.Utilities.HYD.AuxMethods

        Me.Frm = My.Application.ActiveSimulation

        Me.su = Frm.Options.SelectedUnitSystem
        Me.nf = Frm.Options.NumberFormat

        Me.ComboBox3.Items.Clear()
        For Each mat2 In Me.Frm.Collections.FlowsheetObjectCollection.Values
            If mat2.GraphicObject.Calculated Then Me.ComboBox3.Items.Add(mat2.GraphicObject.Tag.ToString)
        Next

        If Me.ComboBox3.Items.Count > 0 Then Me.ComboBox3.SelectedIndex = 0

    End Sub

    Private Sub Button1_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles Button1.Click

        If Not Me.ComboBox3.SelectedItem Is Nothing Then

            Dim gobj As GraphicObject = FormFlowsheet.SearchSurfaceObjectsByTag(Me.ComboBox3.SelectedItem, Frm.FormSurface.FlowsheetDesignSurface)
            Me.mat = Frm.Collections.FlowsheetObjectCollection(gobj.Name)

            mat.PropertyPackage.CurrentMaterialStream = mat

            If mat.PropertyPackage.RET_VCAS().Contains("7732-18-5") Then

                Dim unif As New PropertyPackages.UNIFACPropertyPackage

                unif.CurrentMaterialStream = mat

                Dim n As Integer = mat.Phases(0).Compounds.Count - 1

                Dim Vz(n), T, P As Double
                Dim nomes(mat.Phases(0).Compounds.Count - 1) As String
                Dim comp As BaseClasses.Compound
                Dim i As Integer = 0
                For Each comp In mat.Phases(0).Compounds.Values
                    Vz(i) = comp.MoleFraction.GetValueOrDefault
                    nomes(i) = comp.Name
                    i += 1
                Next
                nomesglobal = nomes
                T = mat.Phases(0).Properties.temperature
                P = mat.Phases(0).Properties.pressure

                Dim pform(1) As Object, tform(1) As Object, PH As Double, TH As Double

                If ComboBox1.SelectedIndex = 0 Then

                    Dim hid As New DWSIM.Utilities.HYD.vdwP_PP(mat)
                    pform = hid.HYD_vdwP2(T, Vz, m_aux.RetornarIDsParaCalculoDeHidratos(nomes), CheckBox1.Checked)
                    tform = hid.HYD_vdwP2T(P, Vz, m_aux.RetornarIDsParaCalculoDeHidratos(nomes), CheckBox1.Checked)

                    'verificar qual estrutura se forma primeiro
                    If pform(0) <= pform(1) Then
                        tipoTC = "sI"
                        PH = pform(0)
                    Else
                        tipoTC = "sII"
                        PH = pform(1)
                    End If
                    'MsgBox(tform(0) & " " & tform(1))
                    'MsgBox(pform(0) & " " & pform(1))

                    If tform(0) >= tform(1) Then
                        tipoPC = "sI"
                        TH = tform(0)
                    Else
                        tipoPC = "sII"
                        TH = tform(1)
                    End If

                    resPC = hid.DET_HYD_vdwP(tipoPC, P, TH, Vz, m_aux.RetornarIDsParaCalculoDeHidratos(nomes), CheckBox1.Checked)
                    resTC = hid.DET_HYD_vdwP(tipoTC, PH, T, Vz, m_aux.RetornarIDsParaCalculoDeHidratos(nomes), CheckBox1.Checked)

                ElseIf ComboBox1.SelectedIndex = 1 Then

                    Dim hid As New DWSIM.Utilities.HYD.KlaudaSandler(mat)
                    pform = hid.HYD_KS2(T, Vz, m_aux.RetornarIDsParaCalculoDeHidratos(nomes), CheckBox1.Checked)
                    tform = hid.HYD_KS2T(P, Vz, m_aux.RetornarIDsParaCalculoDeHidratos(nomes), CheckBox1.Checked)

                    'verificar qual estrutura se forma primeiro
                    If pform(0) <= pform(1) Then
                        tipoTC = "sI"
                        PH = pform(0)
                    Else
                        tipoTC = "sII"
                        PH = pform(1)
                    End If
                    'MsgBox(tform(0) & " " & tform(1))
                    'MsgBox(pform(0) & " " & pform(1))
                    If tform(0) >= tform(1) Then
                        tipoPC = "sI"
                        TH = tform(0)
                    Else
                        tipoPC = "sII"
                        TH = tform(1)
                    End If

                    resPC = hid.DET_HYD_KS(tipoPC, P, TH, Vz, m_aux.RetornarIDsParaCalculoDeHidratos(nomes), CheckBox1.Checked)
                    resTC = hid.DET_HYD_KS(tipoTC, PH, T, Vz, m_aux.RetornarIDsParaCalculoDeHidratos(nomes), CheckBox1.Checked)

                ElseIf ComboBox1.SelectedIndex = 3 Then

                    Dim hid As New DWSIM.Utilities.HYD.KlaudaSandlerMOD(mat)
                    pform = hid.HYD_KS2(T, Vz, m_aux.RetornarIDsParaCalculoDeHidratos(nomes), CheckBox1.Checked)
                    tform = hid.HYD_KS2T(P, Vz, m_aux.RetornarIDsParaCalculoDeHidratos(nomes), CheckBox1.Checked)

                    'verificar qual estrutura se forma primeiro
                    If pform(0) <= pform(1) Then
                        tipoTC = "sI"
                        PH = pform(0)
                    Else
                        tipoTC = "sII"
                        PH = pform(1)
                    End If
                    'MsgBox(tform(0) & " " & tform(1))
                    'MsgBox(pform(0) & " " & pform(1))
                    If tform(0) >= tform(1) Then
                        tipoPC = "sI"
                        TH = tform(0)
                    Else
                        tipoPC = "sII"
                        TH = tform(1)
                    End If

                    If TH > 0 Then resPC = hid.DET_HYD_KS(tipoPC, P, TH, Vz, m_aux.RetornarIDsParaCalculoDeHidratos(nomes), CheckBox1.Checked)
                    resTC = hid.DET_HYD_KS(tipoTC, PH, T, Vz, m_aux.RetornarIDsParaCalculoDeHidratos(nomes), CheckBox1.Checked)

                ElseIf ComboBox1.SelectedIndex = 2 Then

                    Dim hid As New DWSIM.Utilities.HYD.ChenGuo(mat)
                    pform = hid.HYD_CG2(T, Vz, m_aux.RetornarIDsParaCalculoDeHidratos(nomes), CheckBox1.Checked)
                    tform = hid.HYD_CG2T(P, Vz, m_aux.RetornarIDsParaCalculoDeHidratos(nomes), CheckBox1.Checked)


                    'verificar qual estrutura se forma primeiro
                    If pform(0) <= pform(1) Then
                        tipoTC = "sI"
                        PH = pform(0)
                    Else
                        tipoTC = "sII"
                        PH = pform(1)
                    End If
                    'MsgBox(tform(0) & " " & tform(1))
                    'MsgBox(pform(0) & " " & pform(1))
                    If tform(0) >= tform(1) Then
                        tipoPC = "sI"
                        TH = tform(0)
                    Else
                        tipoPC = "sII"
                        TH = tform(1)
                    End If

                    resPC = hid.DET_HYD_CG(tipoPC, P, TH, Vz, m_aux.RetornarIDsParaCalculoDeHidratos(nomes), CheckBox1.Checked)
                    resTC = hid.DET_HYD_CG(tipoTC, PH, T, Vz, m_aux.RetornarIDsParaCalculoDeHidratos(nomes), CheckBox1.Checked)

                End If

                'unidades
                Dim uP, uT As String
                uP = su.pressure
                uT = su.temperature

                Label5.Text = uP
                Label15.Text = uP
                Label1.Text = uT
                Label16.Text = uT

                Dim PhasesTC As String = ""
                If PH > 600 * 101325 Then
                    Label8.Text = DWSIM.App.GetLocalString("ND")
                    Me.KryptonButton2.Enabled = False
                    PhasesTC = DWSIM.App.GetLocalString("ND")
                Else
                    Label8.Text = Format(SystemsOfUnits.Converter.ConvertFromSI(su.pressure, PH), nf)
                    Me.KryptonButton2.Enabled = True
                    If CheckBox1.Checked Then
                        PhasesTC = DWSIM.App.GetLocalString("VaporAndHydrate") & " (" & tipoTC & ")"
                    Else
                        If Math.Abs(T - resTC(0)) < 0.1 Or T = resTC(0) Then
                            PhasesTC = DWSIM.App.GetLocalString("SlidoGeloLquidoguaGs1") & tipoTC & ")"
                        ElseIf T < resTC(0) Then
                            PhasesTC = DWSIM.App.GetLocalString("SlidoGeloGseHidrato1") & tipoTC & ")"
                        ElseIf T > resTC(0) Then
                            PhasesTC = DWSIM.App.GetLocalString("LquidoguaGseHidrato") & tipoTC & ")"
                        End If
                    End If
                End If
                Dim PhasesPC As String = ""
                If TH < 0 Then
                    Label14.Text = DWSIM.App.GetLocalString("ND")
                    Me.KryptonButton3.Enabled = False
                    PhasesPC = DWSIM.App.GetLocalString("ND")
                Else
                    Label14.Text = Format(SystemsOfUnits.Converter.ConvertFromSI(su.temperature, TH), nf)
                    Me.KryptonButton3.Enabled = True
                    If CheckBox1.Checked Then
                        PhasesPC = DWSIM.App.GetLocalString("VaporAndHydrate") & " (" & tipoPC & ")"
                    Else
                        PhasesPC = DWSIM.App.GetLocalString("SlidoGeloGseHidrato1") & tipoPC & ")"
                        If TH > resPC(0) Then PhasesPC = DWSIM.App.GetLocalString("LquidoguaGseHidrato") & tipoPC & ")"
                        If Math.Abs(TH - resPC(0)) < 0.01 Then PhasesPC = DWSIM.App.GetLocalString("SlidoGeloLquidoguaGs1") & tipoPC & ")"
                    End If
                End If
                Label17.Text = Format(SystemsOfUnits.Converter.ConvertFromSI(su.pressure, P), nf)
                Label9.Text = Format(SystemsOfUnits.Converter.ConvertFromSI(su.temperature, T), nf)
                Label12.Text = PhasesPC
                Label10.Text = PhasesTC

                'lógica para verificar se forma hidrato ou não
                If T <= TH Then
                    Label21.Text = DWSIM.App.GetLocalString("Sim")
                    Label20.Text = tipoTC
                ElseIf P >= PH Then
                    Label21.Text = DWSIM.App.GetLocalString("Sim")
                    Label20.Text = tipoPC
                Else
                    Label21.Text = DWSIM.App.GetLocalString("No")
                    Label20.Text = DWSIM.App.GetLocalString("NA")
                End If

                GroupBox1.Enabled = True

                unif.CurrentMaterialStream = Nothing
                unif = Nothing

            Else

                MessageBox.Show(DWSIM.App.GetLocalString("Noexisteguanacorrent"), DWSIM.App.GetLocalString("Erro"), MessageBoxButtons.OK, MessageBoxIcon.Error)

            End If


            Else

                Me.mat = Nothing
                Me.LblSelected.Text = ""

            End If

    End Sub

    Private Sub KryptonButton2_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles KryptonButton2.Click

        Dim frmdet As New FormHYD_DET
        With frmdet
            .res = resTC
            .P = SystemsOfUnits.Converter.ConvertToSI(su.pressure, Label8.Text)
            .T = SystemsOfUnits.Converter.ConvertToSI(su.temperature, Label9.Text)
            If Label10.ToString.Contains("sII") Then .sI = False
            .model = ComboBox1.SelectedIndex
            .nomes = nomesglobal
            .ShowDialog(Me)
        End With


    End Sub

    Private Sub KryptonButton3_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles KryptonButton3.Click

        Dim frmdet As New FormHYD_DET
        With frmdet
            .res = resPC
            .P = SystemsOfUnits.Converter.ConvertToSI(su.pressure, Label17.Text)
            .T = SystemsOfUnits.Converter.ConvertToSI(su.temperature, Label14.Text)
            If Label12.ToString.Contains("sII") Then .sI = False
            .model = ComboBox1.SelectedIndex
            .nomes = nomesglobal
            .ShowDialog(Me)
        End With
    End Sub

    Private Sub FormHYD_HelpRequested(sender As System.Object, hlpevent As System.Windows.Forms.HelpEventArgs) Handles MyBase.HelpRequested
        DWSIM.App.HelpRequested("UT_HydrateDissociation.htm")
    End Sub

    Private Sub ComboBox1_SelectedIndexChanged(sender As Object, e As EventArgs) Handles ComboBox1.SelectedIndexChanged
        If ComboBox1.SelectedIndex = 2 Then CheckBox1.Enabled = False Else CheckBox1.Enabled = True
    End Sub

    Private Sub FloatToolStripMenuItem_Click(sender As Object, e As EventArgs) Handles FloatToolStripMenuItem.Click, DocumentToolStripMenuItem.Click,
                                                                 DockLeftToolStripMenuItem.Click, DockLeftAutoHideToolStripMenuItem.Click,
                                                                 DockRightAutoHideToolStripMenuItem.Click, DockRightToolStripMenuItem.Click,
                                                                 DockTopAutoHideToolStripMenuItem.Click, DockTopToolStripMenuItem.Click,
                                                                 DockBottomAutoHideToolStripMenuItem.Click, DockBottomToolStripMenuItem.Click

        For Each ts As ToolStripMenuItem In dckMenu.Items
            ts.Checked = False
        Next

        sender.Checked = True

        Select Case sender.Name
            Case "FloatToolStripMenuItem"
                Me.DockState = WeifenLuo.WinFormsUI.Docking.DockState.Float
            Case "DocumentToolStripMenuItem"
                Me.DockState = WeifenLuo.WinFormsUI.Docking.DockState.Document
            Case "DockLeftToolStripMenuItem"
                Me.DockState = WeifenLuo.WinFormsUI.Docking.DockState.DockLeft
            Case "DockLeftAutoHideToolStripMenuItem"
                Me.DockState = WeifenLuo.WinFormsUI.Docking.DockState.DockLeftAutoHide
            Case "DockRightAutoHideToolStripMenuItem"
                Me.DockState = WeifenLuo.WinFormsUI.Docking.DockState.DockRightAutoHide
            Case "DockRightToolStripMenuItem"
                Me.DockState = WeifenLuo.WinFormsUI.Docking.DockState.DockRight
            Case "DockBottomAutoHideToolStripMenuItem"
                Me.DockState = WeifenLuo.WinFormsUI.Docking.DockState.DockBottomAutoHide
            Case "DockBottomToolStripMenuItem"
                Me.DockState = WeifenLuo.WinFormsUI.Docking.DockState.DockBottom
            Case "DockTopAutoHideToolStripMenuItem"
                Me.DockState = WeifenLuo.WinFormsUI.Docking.DockState.DockTopAutoHide
            Case "DockTopToolStripMenuItem"
                Me.DockState = WeifenLuo.WinFormsUI.Docking.DockState.DockTop
            Case "HiddenToolStripMenuItem"
                Me.DockState = WeifenLuo.WinFormsUI.Docking.DockState.Hidden
        End Select

    End Sub

End Class
