Imports System.Text
Imports Mono.Cecil
Imports Mono.Cecil.Cil

'TODO: Look into using dnLib
Public Class AssemblyManager

    ReadOnly _showAll As Boolean = False

#Region "UI"
    Private Sub LoadNestedTypes(node As AssemblyItem, tDef As TypeDefinition, showAll As Boolean)
        For Each t As TypeDefinition In tDef.NestedTypes
            Dim sbName As New StringBuilder(t.Name)
            If t.HasGenericParameters Then
                sbName.Remove(sbName.Length - 2, 2)
                DecompileGenericParameters(t.GenericParameters, sbName)
            End If
            Dim n As New AssemblyItem With {
                .Key = t.Name,
                .Text = sbName.ToString,
                .Tag = t
            }
            node.Children.Add(n)
            LoadDefinitions(n, t, showAll)
            LoadNestedTypes(n, t, showAll)
        Next
    End Sub

    Private Shared Sub LoadDefinitions(node As AssemblyItem, tDef As TypeDefinition, showAll As Boolean)
        For Each mDef As MethodDefinition In tDef.Methods.Where(Function(x) showAll OrElse Not x.IsGetter AndAlso Not x.IsSetter AndAlso Not x.IsAddOn AndAlso Not x.IsRemoveOn)
            Dim n As New AssemblyItem With {
                .Key = mDef.Name,
                .Text = mDef.Name,
                .Tag = mDef
            }
            node.Children.Add(n)
        Next
        For Each pDef As PropertyDefinition In tDef.Properties
            Dim n As New AssemblyItem With {
                .Key = pDef.Name,
                .Text = pDef.Name,
                .Tag = pDef
            }
            node.Children.Add(n)
        Next
        For Each eDef As EventDefinition In tDef.Events
            Dim n As New AssemblyItem With {
                .Key = eDef.Name,
                .Text = eDef.Name,
                .Tag = eDef
            }
            node.Children.Add(n)
        Next
        For Each fDef As FieldDefinition In tDef.Fields
            Dim n As New AssemblyItem With {
                .Key = fDef.Name,
                .Text = fDef.Name,
                .Tag = fDef
            }
            node.Children.Add(n)
        Next
    End Sub

    Public Function LoadAssembly(path As String) As List(Of AssemblyItem)
        Dim results As New List(Of AssemblyItem)
        Dim modDef As ModuleDefinition = ModuleDefinition.ReadModule(path)
        For Each tDef As TypeDefinition In modDef.Types
            Dim names As String() = tDef.FullName.Split(".")
            Dim node As AssemblyItem = Nothing
            For i = 0 To names.Length - 2
                Dim name As String = names(i)
                If node Is Nothing Then
                    If results.Any(Function(x) x.Key = name) Then
                        node = results.First(Function(x) x.Key = name)
                    Else
                        node = New AssemblyItem With {
                            .Key = name,
                            .Text = name
                        }
                        results.Add(node)
                    End If
                Else
                    If node.Children.Any(Function(x) x.Key = name) Then
                        node = node.Children.First(Function(x) x.Key = name)
                    Else
                        node = New AssemblyItem With {
                            .Key = name,
                            .Text = name
                        }
                        node.Children.Add(node)
                    End If
                End If
            Next
            Dim sbName As New StringBuilder(names.Last)
            If tDef.HasGenericParameters Then
                sbName.Remove(sbName.Length - 2, 2)
                DecompileGenericParameters(tDef.GenericParameters, sbName)
            End If
            If node Is Nothing Then
                Dim n As New AssemblyItem With {
                    .Key = names.Last,
                    .Text = sbName.ToString,
                    .Tag = tDef
                }
                results.Add(n)
                node = n
            Else
                Dim n As New AssemblyItem With {
                    .Key = names.Last,
                    .Text = sbName.ToString,
                    .Tag = tDef
                }
                node.Children.Add(n)
                node = n
            End If
            LoadDefinitions(node, tDef, _showAll)
            LoadNestedTypes(node, tDef, _showAll)
        Next
        Return results
    End Function

#End Region

    Public Function GetIL(mDef As MethodDefinition) As String
        Dim sb As New StringBuilder
        For Each i As Instruction In mDef.Body.Instructions
            sb.AppendLine(i.ToString())
        Next
        Return sb.ToString()
    End Function
    'TODO: Modifiers
    'TODO: Events

    Public Function DecompileTypeAsString(tDef As TypeDefinition) As String
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
                sb.AppendLine(DecompileMethodAsString(mDef))
            Next
            For Each pDef As PropertyDefinition In tDef.Properties
                sb.AppendLine(DecompilePropertyAsString(pDef))
            Next
            For Each fDef As FieldDefinition In tDef.Fields
                sb.AppendLine(DecompileFieldAsString(fDef))
            Next
            For Each eDef As EventDefinition In tDef.Events
                sb.AppendLine(DecompileEventAsString(eDef))
            Next
            For Each tDef2 As TypeDefinition In tDef.NestedTypes
                sb.AppendLine(DecompileTypeAsString(tDef2))
            Next
        End If
        sb.AppendLine($"End {classDefiner}")
        If Not tDef.IsNested Then
            sb.AppendLine()
            sb.AppendLine("End Namespace")
        End If
        Return sb.ToString
    End Function

    Public Function DecompileTypeAsSyntaxTree(tDef As TypeDefinition) As Microsoft.CodeAnalysis.SyntaxTree
        Return Microsoft.CodeAnalysis.VisualBasic.SyntaxFactory.ParseSyntaxTree(DecompileTypeAsString(tDef))
    End Function

    Public Function DecompileMethodAsString(mDef As MethodDefinition) As String
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
            sb.Append(DecompileMethodBodyAsString(mDef.Body))
            sb.AppendLine($"End {methodDefiner}")
        End If
        Return sb.ToString()
    End Function

    Public Function DecompileMethodAsSyntaxTree(mDef As MethodDefinition) As Microsoft.CodeAnalysis.SyntaxTree
        Return Microsoft.CodeAnalysis.VisualBasic.SyntaxFactory.ParseSyntaxTree(DecompileMethodAsString(mDef))
    End Function

    Public Function DecompileMethodBodyAsString(mBody As MethodBody) As String
        Return ""
    End Function

    Public Function DecompileMethodBodyAsSyntaxTree(mBody As MethodBody) As Microsoft.CodeAnalysis.SyntaxTree
        Return Microsoft.CodeAnalysis.VisualBasic.SyntaxFactory.ParseSyntaxTree(DecompileMethodBodyAsString(mBody))
    End Function

    Public Function DecompilePropertyAsString(pDef As PropertyDefinition) As String
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
                sb.AppendLine(DecompileMethodBodyAsString(pDef.GetMethod.Body))
                sb.AppendLine("End Get")
            End If
            If pDef.SetMethod IsNot Nothing Then
                DecompileAttributes(pDef.GetMethod.CustomAttributes, sb)
                sb.AppendLine("Set")
                sb.AppendLine(DecompileMethodBodyAsString(pDef.SetMethod.Body))
                sb.AppendLine("End Set")
            End If
            sb.AppendLine("End Property")
        End If
        Return sb.ToString()
    End Function

    Public Function DecompilePropertyAsSyntaxTree(pDef As PropertyDefinition) As Microsoft.CodeAnalysis.SyntaxTree
        Return Microsoft.CodeAnalysis.VisualBasic.SyntaxFactory.ParseSyntaxTree(DecompilePropertyAsString(pDef))
    End Function

    Public Function DecompileFieldAsString(fDef As FieldDefinition) As String
        Dim sb As New StringBuilder
        DecompileAttributes(fDef.CustomAttributes, sb)
        sb.AppendLine($"Dim {fDef.Name} As {GetFriendlyName(fDef.FieldType)}")
        Return sb.ToString()
    End Function

    Public Function DecompileFieldAsSyntaxTree(fDef As FieldDefinition) As Microsoft.CodeAnalysis.SyntaxTree
        Return Microsoft.CodeAnalysis.VisualBasic.SyntaxFactory.ParseSyntaxTree(DecompileFieldAsString(fDef))
    End Function

    Public Function DecompileEventAsString(eDef As EventDefinition) As String
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

    Public Function DecompileEventAsSyntaxTree(eDef As EventDefinition) As Microsoft.CodeAnalysis.SyntaxTree
        Return Microsoft.CodeAnalysis.VisualBasic.SyntaxFactory.ParseSyntaxTree(DecompileEventAsString(eDef))
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