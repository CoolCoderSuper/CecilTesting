Imports System.IO
Imports System.Text
Imports Mono.Cecil
Imports Mono.Cecil.Cil

'TODO: Look into using dnLib
Public Class AssemblyManager

    Dim _stream As FileStream
    ReadOnly _showAll As Boolean = False

#Region "UI"
    Private Sub LoadNestedTypes(node As TreeNode, tDef As TypeDefinition, showAll As Boolean)
        For Each t As TypeDefinition In tDef.NestedTypes
            Dim sbName As New StringBuilder(t.Name)
            If t.HasGenericParameters Then
                sbName.Remove(sbName.Length - 2, 2)
                DecompileGenericParameters(t.GenericParameters, sbName)
            End If
            Dim n As TreeNode = node.Nodes.Add(t.Name, sbName.ToString)
            n.Tag = t
            LoadDefinitions(n, t, showAll)
            LoadNestedTypes(n, t, showAll)
        Next
    End Sub

    Private Shared Sub LoadDefinitions(node As TreeNode, tDef As TypeDefinition, showAll As Boolean)
        For Each mDef As MethodDefinition In tDef.Methods.Where(Function(x) showAll OrElse Not x.IsGetter AndAlso Not x.IsSetter AndAlso Not x.IsAddOn AndAlso Not x.IsRemoveOn)
            node.Nodes.Add(mDef.Name, mDef.Name).Tag = mDef
        Next
        For Each pDef As PropertyDefinition In tDef.Properties
            node.Nodes.Add(pDef.Name, pDef.Name).Tag = pDef
        Next
        For Each eDef As EventDefinition In tDef.Events
            node.Nodes.Add(eDef.Name, eDef.Name).Tag = eDef
        Next
        For Each fDef As FieldDefinition In tDef.Fields
            node.Nodes.Add(fDef.Name, fDef.Name).Tag = fDef
        Next
    End Sub

    Public Sub LoadAssembly(path As String, tv As TreeView)
        tv.Nodes.Clear()
        _stream = New FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)
        Dim modDef As ModuleDefinition = ModuleDefinition.ReadModule(_stream)
        For Each tDef As TypeDefinition In modDef.Types
            Dim names As String() = tDef.FullName.Split(".")
            Dim node As TreeNode = Nothing
            For i = 0 To names.Length - 2
                Dim name As String = names(i)
                If node Is Nothing Then
                    node = If(tv.Nodes.ContainsKey(name), tv.Nodes(name), tv.Nodes.Add(name, name))
                Else
                    node = If(node.Nodes.ContainsKey(name), node.Nodes(name), node.Nodes.Add(name, name))
                End If
            Next
            Dim sbName As New StringBuilder(names.Last)
            If tDef.HasGenericParameters Then
                sbName.Remove(sbName.Length - 2, 2)
                DecompileGenericParameters(tDef.GenericParameters, sbName)
            End If
            node = If(node Is Nothing, tv.Nodes.Add(names.Last, sbName.ToString), node.Nodes.Add(names.Last, sbName.ToString))
            node.Tag = tDef
            LoadDefinitions(node, tDef, _showAll)
            LoadNestedTypes(node, tDef, _showAll)
        Next
    End Sub

#End Region

    'Public Shared Sub UpdateMethod(il As String, mDef As MethodDefinition)
    '    Dim ilProcessor As ILProcessor = mDef.Body.GetILProcessor()
    '    ilProcessor.Clear()
    '    Dim lines As String() = il.Split(vbCrLf)
    '    For Each line As String In lines
    '        Dim parts As String() = line.Split(" ")
    '        Dim offset As String = parts(0)
    '        Dim opCode As String = parts(1)
    '        Dim operand As String = String.Join(" "c, parts.Skip(2))
    '        Dim instruction As Instruction = CreateInstruction(opCode)
    '        ilProcessor.Append(instruction)
    '    Next
    'End Sub

    'Private Shared Function CreateInstruction(opCode As String) As Instruction
    '    Dim ins As Instruction
    '    Select Case opCode
    '        Case "add"
    '        Case "add.ovf"
    '        Case "add.ovf.un"
    '        Case "and"
    '        Case "arglist"
    '        Case "beq"
    '        Case "beq.s"
    '        Case "bge"
    '        Case "bge.s"
    '        Case "bge.un"
    '        Case "bge.un.s"
    '        Case "bgt"
    '        Case "bgt.s"
    '        Case "bgt.un"
    '        Case "bgt.un.s"
    '        Case "ble"
    '        Case "ble.s"
    '        Case "ble.un"
    '        Case "ble.un.s"
    '        Case "blt"
    '        Case "blt.s"
    '        Case "blt.un"
    '        Case "blt.un.s"
    '        Case "bne.un"
    '        Case "bne.un.s"
    '        Case "br"
    '        Case "br.s"
    '        Case "break"
    '        Case "brfalse"
    '        Case "brfalse.s"
    '        Case "brinst"
    '        Case "brinst.s"
    '        Case "brnull"
    '        Case "brnull.s"
    '        Case "brtrue"
    '        Case "brtrue.s"
    '        Case "brzero"
    '        Case "brzero.s"
    '        Case "call"
    '        Case "call.i"
    '        Case "callvirt"
    '        Case "castclass"
    '        Case "ceq"
    '        Case "cgt"
    '        Case "cgt.un"
    '        Case "ckfinite"
    '        Case "clt"
    '        Case "clt.un"
    '        Case "constrained."
    '        Case "conv.i"
    '        Case "conv.i1"
    '        Case "conv.i2"
    '        Case "conv.i4"
    '        Case "conv.i8"
    '        Case "conv.ovf.i"
    '        Case "conv.ovf.i.un"
    '        Case "conv.ovf.i1"
    '        Case "conv.ovf.i1.un"
    '        Case "conv.ovf.i2"
    '        Case "conv.ovf.i2.un"
    '        Case "conv.ovf.i4"
    '        Case "conv.ovf.i4.un"
    '        Case "conv.ovf.i8"
    '        Case "conv.ovf.i8.un"
    '        Case "conv.ovf.u"
    '        Case "conv.ovf.u.un"
    '        Case Else
    '            ins = Instruction.Create(OpCodes.Nop)
    '    End Select
    '    Return ins
    'End Function

    Public Function GetIL(mDef As MethodDefinition) As String
        Dim sb As New StringBuilder
        For Each i As Instruction In mDef.Body.Instructions
            sb.AppendLine(i.ToString())
        Next
        Return sb.ToString()
    End Function
    'TODO: Modifiers

    Public Function DecompileType(tDef As TypeDefinition) As String
        Dim sb As New StringBuilder
        Dim classDefiner As String
        With tDef
            If .IsEnum Then
                classDefiner = "Enum"
            ElseIf .IsInterface Then
                classDefiner = "Interface"
            ElseIf .IsValueType Then
                classDefiner = "Structure"
            Else
                classDefiner = "Class"
            End If
        End With
        If Not tDef.IsNested Then
            sb.AppendLine($"Namespace {tDef.Namespace}")
            sb.AppendLine()
        End If
        DecompileAttributes(tDef.CustomAttributes, sb)
        sb.Append($"{classDefiner} {tDef.Name}")
        If tDef.HasGenericParameters Then
            sb.Remove(sb.Length - 2, 2)
            DecompileGenericParameters(tDef.GenericParameters, sb)
        End If
        sb.AppendLine()
        If tDef.BaseType IsNot Nothing Then
            If Not {"System.Object", "System.Enum", "System.ValueType"}.Contains(tDef.BaseType.FullName) Then
                sb.AppendLine($"Inherits {GetFriendlyName(tDef.BaseType)}")
            End If
        End If
        If tDef.HasInterfaces Then
            sb.Append("Implements ")
            For Each i As InterfaceImplementation In tDef.Interfaces
                sb.Append($"{GetFriendlyName(i.InterfaceType)}, ")
            Next
            sb.Remove(sb.Length - 2, 2)
            sb.AppendLine()
        End If
        If tDef.IsEnum Then
            For Each fDef As FieldDefinition In tDef.Fields
                If fDef.Name <> "value__" Then
                    sb.AppendLine($"{fDef.Name} = {fDef.Constant}")
                End If
            Next
        Else
            sb.AppendLine()
            For Each mDef As MethodDefinition In tDef.Methods.Where(Function(x) _showAll OrElse Not x.IsGetter AndAlso Not x.IsSetter AndAlso Not x.IsAddOn AndAlso Not x.IsRemoveOn)
                sb.AppendLine(DecompileMethod(mDef))
            Next
            For Each pDef As PropertyDefinition In tDef.Properties
                sb.AppendLine(DecompileProperty(pDef))
            Next
            For Each fDef As FieldDefinition In tDef.Fields
                sb.AppendLine(DecompileField(fDef))
            Next
            For Each eDef As EventDefinition In tDef.Events
                sb.AppendLine(DecompileEvent(eDef))
            Next
            For Each tDef2 As TypeDefinition In tDef.NestedTypes
                sb.AppendLine(DecompileType(tDef2))
            Next
        End If
        sb.AppendLine($"End {classDefiner}")
        If Not tDef.IsNested Then
            sb.AppendLine()
            sb.AppendLine("End Namespace")
        End If
        Return sb.ToString
    End Function

    Public Function DecompileMethod(mDef As MethodDefinition) As String
        Dim sb As New StringBuilder
        DecompileAttributes(mDef.CustomAttributes, sb)
        Dim isSub As Boolean = mDef.ReturnType.FullName = "System.Void"
        Dim methodDefiner As String = If(isSub, "Sub", "Function")
        Dim methodName As String = If(mDef.IsConstructor, "New", mDef.Name)
        sb.Append($"{methodDefiner} {methodName}")
        If mDef.HasGenericParameters Then
            DecompileGenericParameters(mDef.GenericParameters, sb)
        End If
        If mDef.HasParameters Then
            sb.Append("("c)
            For Each p As ParameterDefinition In mDef.Parameters
                Dim paramType As String = GetFriendlyName(p.ParameterType)
                sb.Append($"{p.Name} As {paramType}, ")
            Next
            sb.Remove(sb.Length - 2, 2)
            sb.Append(")"c)
        End If
        If mDef.HasOverrides Then
            DecompileImplements(mDef.Overrides.Cast(Of MemberReference), sb, Nothing)
        End If
        If isSub Then
            sb.AppendLine()
        Else
            Dim methodType As String = GetFriendlyName(mDef.ReturnType)
            sb.AppendLine($" As {methodType}")
        End If
        If Not mDef.DeclaringType.IsInterface Then
            sb.Append(DecompileMethodBody(mDef.Body))
            sb.AppendLine($"End {methodDefiner}")
        End If
        Return sb.ToString()
    End Function

    Public Function DecompileMethodBody(mBody As MethodBody) As String
        Return ""
    End Function

    Public Function DecompileProperty(pDef As PropertyDefinition) As String
        Dim sb As New StringBuilder
        DecompileAttributes(pDef.CustomAttributes, sb)
        sb.Append($"Property {pDef.Name}")
        If pDef.HasParameters Then
            sb.Append("("c)
            For Each p As ParameterDefinition In pDef.Parameters
                Dim paramType As String = GetFriendlyName(p.ParameterType)
                sb.Append($"{p.Name} As {paramType}, ")
            Next
            sb.Remove(sb.Length - 2, 2)
            sb.Append(")"c)
        End If
        sb.Append($" As {GetFriendlyName(pDef.PropertyType)}")
        Dim checkMethod As MethodDefinition
        If pDef.GetMethod IsNot Nothing Then
            checkMethod = pDef.GetMethod
        ElseIf pDef.SetMethod IsNot Nothing Then
            checkMethod = pDef.SetMethod
        End If
        If checkMethod IsNot Nothing Then
            If checkMethod.HasOverrides Then
                DecompileImplements(checkMethod.Overrides.Cast(Of MemberReference), sb, pDef.Name)
            End If
        End If
        sb.AppendLine()
        If Not pDef.DeclaringType.IsInterface Then
            If pDef.GetMethod IsNot Nothing Then
                DecompileAttributes(pDef.GetMethod.CustomAttributes, sb)
                sb.AppendLine("Get")
                sb.AppendLine(DecompileMethodBody(pDef.GetMethod.Body))
                sb.AppendLine("End Get")
            End If
            If pDef.SetMethod IsNot Nothing Then
                DecompileAttributes(pDef.GetMethod.CustomAttributes, sb)
                sb.AppendLine("Set")
                sb.AppendLine(DecompileMethodBody(pDef.SetMethod.Body))
                sb.AppendLine("End Set")
            End If
            sb.AppendLine("End Property")
        End If
        Return sb.ToString()
    End Function

    Public Function DecompileField(fDef As FieldDefinition) As String
        Dim sb As New StringBuilder
        DecompileAttributes(fDef.CustomAttributes, sb)
        sb.AppendLine($"Dim {fDef.Name} As {GetFriendlyName(fDef.FieldType)}")
        Return sb.ToString()
    End Function

    Public Function DecompileEvent(eDef As EventDefinition) As String
        'TODO: Test
        Dim sb As New StringBuilder
        DecompileAttributes(eDef.CustomAttributes, sb)
        sb.Append($"Event {eDef.Name} As {GetFriendlyName(eDef.EventType)}")
        If eDef.AddMethod.HasOverrides Then
            DecompileImplements(eDef.AddMethod.Overrides.Cast(Of MemberReference), sb, eDef.Name)
        End If
        sb.AppendLine()
        If Not eDef.DeclaringType.IsInterface Then
            sb.AppendLine("AddHandler(value As EventHandler)")
            sb.AppendLine("RemoveHandler(value As EventHandler)")
            sb.AppendLine("RaiseEvent(sender As Object, e As EventArgs)")
            sb.AppendLine("End Event")
        End If
        Return "" 'sb.ToString()
    End Function

    Private Sub DecompileAttributes(customAttributes As Mono.Collections.Generic.Collection(Of CustomAttribute), sb As StringBuilder)
        'TODO: Calls to methods \ variables
        For Each attr As CustomAttribute In customAttributes
            sb.Append($"<{GetFriendlyName(attr.AttributeType)}")
            If attr.HasConstructorArguments OrElse attr.HasProperties Then sb.Append("("c)
            If attr.HasConstructorArguments Then
                For Each arg As CustomAttributeArgument In attr.ConstructorArguments
                    sb.Append($"{DecompileValue(arg.Value)}, ")
                Next
            End If
            If attr.HasProperties Then
                For Each prop As CustomAttributeNamedArgument In attr.Properties
                    sb.Append($"{prop.Name}:={DecompileValue(prop.Argument.Value)}, ")
                Next
            End If
            If attr.HasConstructorArguments OrElse attr.HasProperties Then
                sb.Remove(sb.Length - 2, 2)
                sb.Append(")"c)
            End If
            sb.Append(">"c)
            sb.AppendLine()
        Next
    End Sub

    Private Sub DecompileGenericParameters(genericParameters As Mono.Collections.Generic.Collection(Of GenericParameter), sb As StringBuilder)
        sb.Append($"(Of ")
        For Each gp As GenericParameter In genericParameters
            sb.Append($"{gp.Name}, ")
        Next
        sb.Remove(sb.Length - 2, 2)
        sb.Append(")"c)
    End Sub

    Private Function DecompileValue(val As Object) As String
        If IsNumeric(val) Then
            Return val.ToString()
        ElseIf TypeOf val Is String Then
            Return $"""{val}"""
        ElseIf TypeOf val Is Char Then
            Return $"""{val}""c"
        Else
            Return ""
        End If
    End Function

    Private Sub DecompileImplements([overrides] As IEnumerable(Of MemberReference), sb As StringBuilder, memName As String)
        For Each o As MemberReference In [overrides]
            If o.DeclaringType.Resolve.IsInterface Then
                sb.Append($" Implements {GetFriendlyName(o.DeclaringType)}.{If(memName, o.Name)}")
                Exit For
            End If
        Next
    End Sub

    Private Function GetFriendlyName(tRef As TypeReference) As String
        Dim sb As New StringBuilder(tRef.Namespace)
        Dim tDef As TypeDefinition = tRef.Resolve
        If tRef.IsGenericParameter Then
            sb.Append(tRef.Name)
        Else
            If ContainsBaseType("System.Attribute", tDef) Then
                sb.Append($".{tRef.Name.Remove(tRef.Name.Length - 9)}")
            Else
                If tDef.IsNested Then
                    Dim sbNamespace As New StringBuilder
                    Dim tDefParent As TypeDefinition = tDef.DeclaringType
                    While True
                        sbNamespace.Append($".{tDefParent.Name}")
                        If tDefParent.IsNested Then
                            tDefParent = tDefParent.DeclaringType
                        Else
                            Exit While
                        End If
                    End While
                    sbNamespace.Insert(0, tDefParent.Namespace)
                    sb.Append(sbNamespace)
                End If
                sb.Append($".{tRef.Name}")
            End If
        End If
        If tRef.IsGenericInstance Then
            sb.Remove(sb.Length - 2, 2)
            sb.Append($"(Of ")
            For Each gp As TypeReference In DirectCast(tRef, GenericInstanceType).GenericArguments
                sb.Append($"{GetFriendlyName(gp)}, ")
            Next
            sb.Remove(sb.Length - 2, 2)
            sb.Append(")"c)
        End If
        Return sb.ToString
    End Function

    Private Function ContainsBaseType(type As String, tDef As TypeDefinition) As Boolean
        If tDef.BaseType Is Nothing Then Return False
        Return tDef.BaseType.FullName = type OrElse ContainsBaseType(type, tDef.BaseType.Resolve)
    End Function
End Class