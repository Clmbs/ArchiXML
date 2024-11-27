Imports System
Imports System.ComponentModel.Design
Imports System.Data
Imports System.Data.Common
Imports System.IO
Imports System.Net.Sockets
'Imports System.Net.WebRequestMethods
Imports System.Reflection
Imports System.Security.Cryptography
Imports System.Text
Imports System.Transactions
Imports System.Xml
Imports System.Xml.Linq
Imports System.Xml.Serialization
Imports Microsoft.SqlServer
Imports MySql.Data
Imports MySql.Data.MySqlClient
Imports Mysqlx.Crud
Imports Mysqlx.XDevAPI.Common
Imports Mysqlx.XDevAPI.Relational
Imports Org.BouncyCastle
Imports Org.BouncyCastle.Asn1
Imports Org.BouncyCastle.Asn1.X509.Qualified
Imports Org.BouncyCastle.Bcpg
Imports Org.BouncyCastle.Crypto.Agreement
Imports Org.BouncyCastle.Math.EC.Custom
Imports Org.BouncyCastle.Ocsp
'Imports File = System.Net.WebRequestMethods.File

Module DataManagement
    'Public connectionString As String
    Sub Connect_archi()
        Exit Sub
        Using connection As New MySqlConnection(connectionString)
            Try
                connection.Open()
            Catch ex As MySqlException
                Console.WriteLine("Error1: " & ex.Message)
            End Try
        End Using

    End Sub
    Public output

    Sub RunSQL(query As String, p1 As String, p2 As String, p3 As String, p4 As String, p5 As String,
               p6 As String, p7 As String, p8 As String, p9 As String, p10 As String)
        If Not ConnectionState.Open Then Connect_archi()

        Using connection As New MySqlConnection(connectionString)
            Try
                connection.Open()
                'Dim query As String = "SELECT * FROM models WHERE name != @value"
                'Dim query As String = "INSERT INTO your_table_name (column1, column2) VALUES (@value1, @value2)"
                'Dim query As String = "UPDATE your_table_name SET column1 = @new_value WHERE column2 = @condition"

                Using cmd As New MySqlCommand(query, connection)
                    ' Use parameters to prevent SQL injection
                    cmd.Parameters.AddWithValue("@p1", p1)
                    cmd.Parameters.AddWithValue("@p2", p2)
                    cmd.Parameters.AddWithValue("@p3", p3)
                    cmd.Parameters.AddWithValue("@p4", p4)
                    cmd.Parameters.AddWithValue("@p5", p5)
                    cmd.Parameters.AddWithValue("@p6", p6)
                    cmd.Parameters.AddWithValue("@p7", p7)
                    cmd.Parameters.AddWithValue("@p8", p8)
                    cmd.Parameters.AddWithValue("@p9", p9)
                    cmd.Parameters.AddWithValue("@p10", p10)

                    Select Case Left(UCase(query), 6)
                        Case "SELECT"
                            Dim output As String = ""
                            Using reader As MySqlDataReader = cmd.ExecuteReader()
                                While reader.Read()
                                    ' Access your data here
                                    output &= reader("id").ToString()
                                    Console.WriteLine("first output: " & output)
                                End While
                            End Using
                        Case Else
                            cmd.ExecuteNonQuery()
                    End Select

                End Using

            Catch ex As MySqlException
                Console.WriteLine($" Error in RunSQL: '{ex.Message}', failed Query: '{query}'. Script aborted.")
                Environment.Exit(1)
            End Try
            connection.Close()
        End Using

    End Sub
    Sub Insert_Staging_Transaction2(qry As String, data As List(Of Dictionary(Of String, String)), caller As String)
        If Not ConnectionState.Open Then Connect_archi()
        Dim batchSize As Integer = 1000
        Using connection As New MySqlConnection(connectionString)
            Try
                connection.Open()
                Dim transaction As MySqlTransaction = connection.BeginTransaction()

                Try
                    ' Define a reasonable batch size (tweak as necessary)
                    Dim recordCount As Integer = 0
                    Dim batchQuery As New StringBuilder(qry)
                    Dim isFirstRecord As Boolean = True
                    Dim paramCounter As Integer = 1

                    Using cmd As New MySqlCommand("", connection, transaction)
                        ' Insert each record within the transaction
                        For Each record As Dictionary(Of String, String) In data
                            If recordCount >= batchSize Then
                                ' Execute the current batch if the size exceeds the limit
                                Console.WriteLine("1000")
                                cmd.CommandText = batchQuery.ToString()
                                cmd.ExecuteNonQuery()

                                ' Reset for next batch
                                batchQuery = New StringBuilder(qry)
                                isFirstRecord = True
                                recordCount = 0
                                cmd.Parameters.Clear()  ' Clear parameters for the new batch
                                Console.WriteLine()
                                paramCounter = 1
                            End If

                            ' Add the new record to the batch
                            If Not isFirstRecord Then
                                batchQuery.Append(", ")
                            End If
                            batchQuery.AppendFormat("( @mid{0}, @mdt{0}, @oid{0}, @tbl{0}, @otp{0}, @nam{0}, @doc{0}, @src{0}, @tgt{0}, @stp{0}, @pid{0}, @rid{0} )", paramCounter)

                            ' Add parameters for the record
                            cmd.Parameters.AddWithValue($"@mid{paramCounter}", record("mid"))
                            cmd.Parameters.AddWithValue($"@mdt{paramCounter}", record("mdt"))
                            cmd.Parameters.AddWithValue($"@oid{paramCounter}", record("oid"))
                            cmd.Parameters.AddWithValue($"@tbl{paramCounter}", record("tbl"))
                            cmd.Parameters.AddWithValue($"@otp{paramCounter}", record("otp"))
                            cmd.Parameters.AddWithValue($"@nam{paramCounter}", record("nam"))
                            cmd.Parameters.AddWithValue($"@doc{paramCounter}", record("doc"))
                            cmd.Parameters.AddWithValue($"@src{paramCounter}", record("src"))
                            cmd.Parameters.AddWithValue($"@tgt{paramCounter}", record("tgt"))
                            cmd.Parameters.AddWithValue($"@stp{paramCounter}", record("stp"))
                            cmd.Parameters.AddWithValue($"@pid{paramCounter}", record("pid"))
                            cmd.Parameters.AddWithValue($"@rid{paramCounter}", record("rid"))

                            isFirstRecord = False
                            paramCounter += 1
                            recordCount += 1
                        Next

                        ' Execute any remaining records in the batch
                        If recordCount > 0 Then
                            cmd.CommandText = batchQuery.ToString()
                            cmd.ExecuteNonQuery()
                        End If

                        ' Commit the transaction
                        transaction.Commit()
                        Console.WriteLine("Transaction committed successfully.")
                    End Using
                Catch ex As Exception
                    ' Rollback if something goes wrong
                    transaction.Rollback()
                    Console.WriteLine("Transaction rolled back due to an error: " & ex.Message)
                    Console.WriteLine("MySQL Error Message: " & ex.Message)
                    Console.WriteLine("Error Code: " & ex.ToString)  ' Useful for identifying specific MySQL errors
                    Console.WriteLine("Stack Trace: " & ex.StackTrace)
                End Try
            Catch ex As MySqlException
                Console.WriteLine("Error with " & caller & ": " & ex.Message)
            End Try
        End Using
    End Sub
    Sub Insert_Staging_Transaction(qry As String, data As List(Of Dictionary(Of String, String)), caller As String)
        If Not ConnectionState.Open Then Connect_archi()

        Using connection As New MySqlConnection(connectionString)
            Try
                connection.Open()
                Dim transaction As MySqlTransaction = connection.BeginTransaction()
                Dim totalRecords As Integer = data.Count


                Try
                    ' Insert each record within the transaction
                    Dim minperc As Integer = 0
                    For r As Integer = 0 To totalRecords - 1
                        Dim record As Dictionary(Of String, String) = data(r)

                        Dim percentage As Decimal = CInt((r + 1) * 100 / totalRecords)

                        If percentage >= minperc Then
                            Console.Write(".") 'Console.Write(r & ": " & $"{percentage}% ...")
                            minperc += 5
                        End If

                        Using cmd As New MySqlCommand(qry, connection, transaction)
                            cmd.Parameters.AddWithValue("@mid", record("mid"))
                            cmd.Parameters.AddWithValue("@mdt", record("mdt"))
                            cmd.Parameters.AddWithValue("@oid", record("oid"))
                            cmd.Parameters.AddWithValue("@tbl", record("tbl"))
                            cmd.Parameters.AddWithValue("@otp", record("otp"))
                            cmd.Parameters.AddWithValue("@nam", record("nam"))
                            cmd.Parameters.AddWithValue("@doc", record("doc"))
                            cmd.Parameters.AddWithValue("@src", record("src"))
                            cmd.Parameters.AddWithValue("@tgt", record("tgt"))
                            cmd.Parameters.AddWithValue("@stp", record("stp"))
                            cmd.Parameters.AddWithValue("@pid", record("pid"))
                            cmd.Parameters.AddWithValue("@rid", record("rid"))

                            cmd.ExecuteNonQuery()
                        End Using
                    Next r

                    ' Commit the transaction
                    transaction.Commit()
                    Console.WriteLine("completed successfully.")
                Catch ex As Exception
                    ' Rollback if something goes wrong
                    transaction.Rollback()
                    Console.WriteLine("Transaction rolled back due to an error: " & ex.Message)
                End Try
            Catch ex As MySqlException
                Console.WriteLine("Error with " & caller & ": " & ex.Message)
            End Try
        End Using
    End Sub
    Sub ExecuteSqlTransaction(sqlStatements As List(Of String), caller As String)
        Using connection As New MySqlConnection(connectionString)
            connection.Open()

            Dim transaction As MySqlTransaction = connection.BeginTransaction()
            Using cmd As MySqlCommand = connection.CreateCommand()
                cmd.Connection = connection
                cmd.Transaction = transaction

                Try
                    ' Execute each SQL statement within the transaction
                    For Each sql In sqlStatements
                        cmd.CommandText = sql
                        cmd.ExecuteNonQuery()
                    Next

                    ' Commit the transaction if all statements succeeded
                    transaction.Commit()
                    Console.WriteLine(" Transaction [" & caller & "] processed succesfully")
                Catch ex As Exception
                    ' Rollback the transaction in case of an error
                    Console.WriteLine("*** ERROR *** " & caller & ": " & ex.Message)
                    Try
                        transaction.Rollback()
                        Console.WriteLine("[" & caller & "] Transaction rolled back due to error: " & ex.Message)
                    Catch rollbackEx As Exception
                        Console.WriteLine("[" & caller & "] Error during rollback: " & rollbackEx.Message)
                    End Try
                End Try
            End Using
        End Using
    End Sub
    Function GetSQLValue(ByVal query As String, p6 As String, p7 As String) As VariantType

        Connect_archi()
        Using connection As New MySqlConnection(connectionString)
            Try
                connection.Open()
                Using cmd As New MySqlCommand(query, connection)
                    cmd.Parameters.AddWithValue("@p6", p6)
                    cmd.Parameters.AddWithValue("@p7", p7)
                    Dim result = cmd.ExecuteScalar()
                    Return result
                End Using

            Catch ex As MySqlException
                ' Handle any errors
                Console.WriteLine("Error4: " & ex.Message)
                Return Nothing
            End Try

        End Using

    End Function
    Sub Run_Stored_Procedure(ByVal YourStoredProcedureName As String)
        ' Define your MySQL connection string (replace with your credentials and database info)

        Console.WriteLine("Running stored procedure [" & YourStoredProcedureName & "]")
        ' Create a new MySQL connection
        Using connection As New MySqlConnection(connectionString)
            Try
                ' Open the connection
                connection.Open()

                ' Create a command to call the stored procedure
                Using cmd As New MySqlCommand(YourStoredProcedureName, connection)
                    cmd.CommandType = CommandType.StoredProcedure
                    Dim outputParam As New MySqlParameter("@OutputParam", MySqlDbType.Int32)
                    outputParam.Direction = ParameterDirection.Output
                    cmd.Parameters.Add(outputParam)
                    cmd.ExecuteNonQuery()
                    Dim result As Integer = Convert.ToInt32(cmd.Parameters("@OutputParam").Value)
                    Console.WriteLine("Output Parameter Value: " & result)
                End Using
            Catch ex As MySqlException
                ' Handle any SQL-related errors
                Console.WriteLine("MySQL Error (" & YourStoredProcedureName & ex.Message)
            Catch ex As Exception
                ' Handle any other errors
                Console.WriteLine("Error: " & ex.Message)
            Finally
                ' Close the connection
                connection.Close()
            End Try
        End Using
    End Sub
End Module
Module Module1

    Public ns As XNamespace = "http://www.opengroup.org/xsd/archimate/3.0/"
    Public iniFilePath As String
    Public Property_Array(0 To 1000, 0 To 2)
    Public mid As String = ""
    Public mdt As String = ""
    Public connectionString As String
    Public start As DateTime = Now


    Sub Main()

        'declaratie variabelen
        Dim oid As String = ""
        Dim tbl As String = ""
        Dim otp As String = ""
        Dim nam As String = ""
        Dim doc As String = ""
        Dim src As String = ""
        Dim tgt As String = ""
        Dim stp As String = ""
        Dim pid As String = ""
        Dim rid As String = ""
        Dim propname As String = ""
        Dim xmldoc As New XmlDocument()

        '---------------------------------------------------------------------
        Dim data As New List(Of Dictionary(Of String, String))
        Dim start As DateTime = Now

        '==================================================================================================
        '===                                      Read INI file                                        ====
        '==================================================================================================



        If ReadIniFile() = False Then Exit Sub



        RunSQL("Truncate staging", Nothing, Nothing, Nothing, Nothing, Nothing, Nothing, Nothing, Nothing, Nothing, Nothing)

        Dim xmlFiles As String() = Directory.GetFiles(iniFilePath, "*.xml")

        For Each xmlFile In xmlFiles
            Try

                Dim archifile As String = Path.Combine(iniFilePath, xmlFile)
                xmldoc.Load(archifile)
                Dim xml As XDocument = XDocument.Load(archifile)
                Dim fileInfo As New FileInfo(archifile)
                mdt = fileInfo.LastWriteTime.ToString("yyyy-MM-dd HH:mm:ss")

                '==================================================================================================
                '===                                 Process PropertyDefinitions                               ====
                '==================================================================================================
                Console.WriteLine("Processing data from " & xmlFile & ":")
                Console.Write("property definitions")

                ' Query the property definitions
                Dim propertyDefinitions = (From propDef In xml.Descendants(ns + "propertyDefinition")
                                           Select New With {
                                      .Identifier = propDef.Attribute("identifier").Value,
                                      .Type = propDef.Attribute("type").Value,
                                      .Name = propDef.Element(ns + "name").Value
                                  })
                Dim linecounter = 0

                If propertyDefinitions IsNot Nothing Then

                    For Each propDef In propertyDefinitions
                        Dim propDefId As String = propDef.Identifier
                        Dim propDefName As String = propDef.Name
                        Property_Array(linecounter, 0) = propDef.Identifier
                        Property_Array(linecounter, 1) = propDef.Name
                        linecounter = linecounter + 1
                    Next
                End If
                '==================================================================================================
                '===                                         Process Model                                     ====
                '==================================================================================================
                Console.Write(", model")

                mid = xml.Root.Attribute("identifier").Value
                Dim nameElement As XElement = xml.Root.Element(ns + "name")
                Dim objectname As String = nameElement.Value
                Dim ns_dc As XNamespace = "http://purl.org/dc/elements/1.1/"
                Dim documentation As String = If(xml.Root.Element(ns + "documentation") IsNot Nothing, xml.Root.Element(ns + "documentation").Value.Replace("'", "`"), String.Empty)
                Dim schema As String = String.Empty
                Dim schemaversion As String = String.Empty
                Dim title As String = String.Empty
                Dim creator As String = String.Empty
                Dim subject As String = String.Empty
                Dim identifier As String = String.Empty
                ' Extract metadata values


                'If(rel.Element(ns + "documentation") IsNot Nothing, rel.Element(ns + "documentation").Value.Replace("'", "`"), String.Empty),

                If xml.Root.Element(ns + "metadata") IsNot Nothing Then

                    schema = If(xml.Root.Element(ns + "metadata").Element(ns_dc + "schema") IsNot Nothing,
                        xml.Root.Element(ns + "metadata").Element(ns_dc + "schema").Value, String.Empty)
                    schemaversion = If(xml.Root.Element(ns + "metadata").Element(ns_dc + "schemaversion") IsNot Nothing,
                        xml.Root.Element(ns + "metadata").Element(ns_dc + "schemaversion").Value, String.Empty)
                    title = If(xml.Root.Element(ns + "metadata").Element(ns_dc + "title") IsNot Nothing,
                        xml.Root.Element(ns + "metadata").Element(ns_dc + "title").Value, String.Empty)
                    creator = If(xml.Root.Element(ns + "metadata").Element(ns_dc + "creator") IsNot Nothing,
                        xml.Root.Element(ns + "metadata").Element(ns_dc + "creator").Value, String.Empty)
                    subject = If(xml.Root.Element(ns + "metadata").Element(ns_dc + "subject") IsNot Nothing,
                        xml.Root.Element(ns + "metadata").Element(ns_dc + "subject").Value, String.Empty)
                    identifier = If(xml.Root.Element(ns + "metadata").Element(ns_dc + "identifier") IsNot Nothing,
                        xml.Root.Element(ns + "metadata").Element(ns_dc + "identifier").Value, String.Empty)

                End If
                RunSQL("INSERT INTO archi1.models (id, name, documentation, title, run_date, subject, creator, `schema`, schema_version, identifier,xml_date) 
                  VALUES(@p1,@p2,@p3,@p4,@p5,@p6,@p7,@p8,@p9,@p10,'" & mdt & "');", mid, objectname, documentation, title, Now.ToString("yyyy-MM-dd HH:mm:ss"), subject, creator, schema, schemaversion, identifier)



                '==================================================================================================
                '===                                        Process Elements                                   ====
                '==================================================================================================
                Console.Write(", elements")
                Call reset_values(oid, tbl, otp, nam, doc, src, tgt, stp, pid, rid)
                tbl = "elements"

                Dim elements = From el In xml.Descendants(ns + "element")
                               Select New With {
                               .Identifier = el.Attribute("identifier").Value,
                               .Type = el.Attribute(XName.Get("type", "http://www.w3.org/2001/XMLSchema-instance")).Value,
                               .Name = el.Element(ns + "name").Value,
                               .Documentation = If(el.Element(ns + "documentation") IsNot Nothing, el.Element(ns + "documentation").Value, String.Empty),
                               .Properties = From prop In el.Descendants(ns + "property")
                                             Select New With {
                                                 .PropertyRef = prop.Attribute("propertyDefinitionRef").Value,
                                                 .Value = prop.Element(ns + "value").Value
                                             }
                           }

                ' Output the parsed data
                If elements IsNot Nothing Or 1 = 1 Then
                    For Each element In elements
                        oid = element.Identifier
                        otp = element.Type
                        nam = element.Name
                        doc = element.Documentation
                        If Not String.IsNullOrEmpty(element.Documentation) Then doc = element.Documentation Else doc = ""

                        Add_record_to_dictionary(data, oid, tbl, otp, nam, doc, src, tgt, stp, pid, rid)
                        For Each prop In element.Properties
                            propname = GetProperty(prop.PropertyRef)
                            Add_record_to_dictionary(data, propname, "properties", "", propname, prop.Value, "", "", "", "", oid)
                        Next

                    Next
                End If


                '==================================================================================================
                '===                                         Process Relations                                 ====
                '==================================================================================================
                Console.Write(", relations")
                Call reset_values(oid, tbl, otp, nam, doc, src, tgt, stp, pid, rid)
                tbl = "relations"


                Dim relationships = From rel In xml.Descendants(ns + "relationship")
                                    Select New With {
                                .Documentation = If(rel.Element(ns + "documentation") IsNot Nothing, rel.Element(ns + "documentation").Value.Replace("'", "`"), String.Empty),
                                .Identifier = rel.Attribute("identifier").Value,
                                .Source = rel.Attribute("source").Value,
                                .Target = rel.Attribute("target").Value,
                                .Type = rel.Attribute(XName.Get("type", "http://www.w3.org/2001/XMLSchema-instance")).Value,
                                .Name = If(rel.Element(ns + "name") IsNot Nothing, rel.Element(ns + "name").Value, String.Empty),
                                .IsDirected = If(rel.Attribute("isDirected") IsNot Nothing, rel.Attribute("isDirected").Value, "false"),
                                .Modifier = If(rel.Attribute("modifier") IsNot Nothing, rel.Attribute("modifier").Value, "false"),' Note: "isDirected" with lowercase "i"
                                .Properties = From prop In rel.Descendants(ns + "property")
                                              Select New With {
                                                  .PropertyRef = prop.Attribute("propertyDefinitionRef").Value,
                                                  .Value = prop.Element(ns + "value").Value
                                              }
                            }

                ' Output the parsed relationship data
                If relationships IsNot Nothing Then
                    For Each relationship In relationships
                        oid = relationship.Identifier
                        otp = relationship.Type
                        nam = relationship.Name
                        doc = relationship.Documentation
                        src = relationship.Source
                        tgt = relationship.Target

                        If otp = "Association" Then
                            stp = relationship.IsDirected
                        ElseIf otp = "Influence" Then
                            stp = relationship.Modifier
                        Else
                            stp = ""
                        End If

                        If Not String.IsNullOrEmpty(relationship.Documentation) Then doc = relationship.Documentation

                        Add_record_to_dictionary(data, oid, tbl, otp, nam, doc, src, tgt, stp, pid, rid)
                        For Each prop In relationship.Properties
                            propname = GetProperty(prop.PropertyRef)
                            Add_record_to_dictionary(data, propname, "properties", "", propname, prop.Value, "", "", "", "", oid)
                        Next

                    Next
                End If

                '==================================================================================================
                '===                                         Process Views                                     ====
                '==================================================================================================
                Console.Write(", views")

                Call reset_values(oid, tbl, otp, nam, doc, src, tgt, stp, pid, rid)
                tbl = "views"

                Dim views = From view In xml.Descendants(ns + "view")
                            Select New With
                       {
                        .Identifier = view.Attribute("identifier").Value,
                        .Name = If(view.Element(ns + "name") IsNot Nothing, view.Element(ns + "name").Value, String.Empty),
                        .Documentation = If(view.Element(ns + "documentation") IsNot Nothing, view.Element(ns + "documentation").Value.Replace("'", "`"), String.Empty),
                        .Properties = From prop In view.Descendants(ns + "property")
                                      Select New With {
                                          .PropertyRef = prop.Attribute("propertyDefinitionRef").Value,
                                          .Value = prop.Element(ns + "value").Value
                                      }
                        }


                ' Output the parsed view data
                If views IsNot Nothing Then

                    Dim views2 = From view In xml.Descendants(ns + "view")
                                 Select New With {
                            .Identifier = view.Attribute("identifier").Value,
                            .Name = If(view.Element(ns + "name") IsNot Nothing, view.Element(ns + "name").Value, String.Empty),
                            .Documentation = If(view.Element(ns + "documentation") IsNot Nothing, view.Element(ns + "documentation").Value.Replace("'", "`"), String.Empty),
                            .Nodes = From node In view.Descendants(ns + "node")
                                     Select New With {.ElementRef = If(node.Attribute("elementRef") IsNot Nothing, node.Attribute("elementRef").Value, String.Empty)}
                            }

                    Dim views3 = From view In xml.Descendants(ns + "view")
                                 Select New With {
                            .Identifier = view.Attribute("identifier").Value,
                            .Name = If(view.Element(ns + "name") IsNot Nothing, view.Element(ns + "name").Value, String.Empty),
                            .Documentation = If(view.Element(ns + "documentation") IsNot Nothing, view.Element(ns + "documentation").Value.Replace("'", "`"), String.Empty),
                            .Connections = From connection In view.Descendants(ns + "connection")
                                           Select New With {.RelationshipRef = If(connection.Attribute("relationshipRef") IsNot Nothing, connection.Attribute("relationshipRef").Value, String.Empty)}
                            }


                    For Each view In views
                        oid = view.Identifier
                        nam = view.Name
                        doc = view.Documentation

                        Add_record_to_dictionary(data, oid, tbl, "", nam, doc, "", "", "", pid, rid)
                        For Each prop In view.Properties
                            propname = GetProperty(prop.PropertyRef)
                            Add_record_to_dictionary(data, propname, "properties", "", propname, prop.Value, "", "", "", "", oid)
                        Next
                    Next

                    For Each view In views2
                        For Each node In view.Nodes
                            oid = view.Identifier
                            rid = node.ElementRef
                            nam = view.Name
                            doc = view.Documentation
                            Add_record_to_dictionary(data, oid, "objects_in_view", "", nam, doc, "", "", "", "", rid)
                        Next
                    Next

                    For Each view In views3
                        For Each connection In view.Connections
                            oid = view.Identifier
                            rid = connection.RelationshipRef
                            nam = view.Name
                            doc = view.Documentation
                            Add_record_to_dictionary(data, oid, "objects_in_view", "", nam, doc, "", "", "", "", rid)
                        Next
                    Next
                End If

                '==================================================================================================
                '===                                         Process Folders                                   ====
                '==================================================================================================
                Console.WriteLine(", folders.")

                Call reset_values(oid, tbl, otp, nam, doc, src, tgt, stp, pid, rid)
                For Each childNode As XmlNode In xmldoc.DocumentElement.ChildNodes
                    ProcessFolders(childNode, "", childNode.Name, data)
                Next

                '==================================================================================================
                '===                               Store in staging table                                      ====
                '==================================================================================================
                Console.WriteLine("Storing data into staging database")
                Dim query As String = "INSERT INTO staging (model_id,date,object_id,target_table,object_type,name,documentation,source_id,target_id,subtype,parent_id,ref_id) 
                VALUES (@mid, @mdt, @oid, @tbl, @otp, @nam, @doc, @src, @tgt, @stp, @pid, @rid);"
                Insert_Staging_Transaction(query, data, "main - store in staging table")

                Dim sql1 As String = "UPDATE staging AS s1 JOIN staging AS s2 ON s1.object_id = s2.ref_id SET s1.parent_id = s2.name WHERE s1.target_table = 'views' AND s2.target_table = 'folder';"
                Dim sql2 As String = "UPDATE staging SET md5=md5(concat(name, documentation, source_id, target_id, subtype, parent_id, ref_id));"
                RunSQL(sql1, "", "", "", "", "", "", "", "", "", "")
                RunSQL(sql2, "", "", "", "", "", "", "", "", "", "")


                ' After processing, move the file to the "Processed" folder
                Dim fileName As String = Path.GetFileNameWithoutExtension(xmlFile) & "_" & Now.ToString("yyyyMMdd_HHmmss") & ".txt"
                Dim destinationPath As String = Path.Combine(iniFilePath & "\Processed", fileName)

                File.Move(xmlFile, destinationPath)
                Console.WriteLine($"'{xmlFile}' moved to: '{destinationPath}' in '{(Now - start)}' ")

            Catch ex As Exception
                Console.WriteLine("Error processing file: " & xmlFile & " - " & ex.Message)
            End Try

        Next
        Run_Stored_Procedure("update_database")
        Console.WriteLine("Finished in " & (Now - start).ToString)
        '==================================================================================================
        '===                                         F I N I S H E D                                   ====
        '==================================================================================================

    End Sub

    Sub Update_database()

        '==================================================================================================
        '===                                    Save to archi database tables                           ====
        '==================================================================================================

        Run_Stored_Procedure("update_database")


    End Sub

    Sub ProcessFolders(node As XmlNode, parentPath As String, tablename As String, data As List(Of Dictionary(Of String, String)))
        Dim ref As String
        If node.Name = "organizations" Then tablename = "folder"

        For Each childNode As XmlNode In node.ChildNodes
            'Console.WriteLine(childNode.Name)

            Select Case childNode.Name
                Case "item"
                    Dim objectname As String = parentPath

                    If childNode.Attributes("identifierRef") IsNot Nothing Then
                        ref = childNode.Attributes("identifierRef").Value
                        ' If the <item> has identifierRef, concatenate it to the current path
                        If LCase(Strings.Left(objectname, 5)) = "views" Then
                            Add_record_to_dictionary(data, "", tablename, "", objectname, "", "", "", "", "", ref)
                        End If

                    ElseIf childNode("label") IsNot Nothing Then
                        ' Otherwise, concatenate the label
                        Dim label As String = childNode("label").InnerText
                        objectname &= If(String.IsNullOrEmpty(parentPath), label, "/" & label)
                    End If
                    ' Recursively parse child items
                    ProcessFolders(childNode, objectname, tablename, data)
                    'End If
            End Select
        Next

    End Sub

    Sub reset_values(oid, tbl, otp, nam, doc, src, tgt, stp, pid, rid)
        oid = ""
        tbl = ""
        otp = ""
        nam = ""
        doc = ""
        src = ""
        tgt = ""
        stp = ""
        pid = ""
        rid = ""
    End Sub

    Sub Add_record_to_dictionary(data, oid, tbl, otp, nam, doc, src, tgt, stp, pid, rid)
        Dim record As New Dictionary(Of String, String)
        record("mid") = mid
        record("mdt") = mdt
        record("oid") = oid
        record("tbl") = tbl
        record("otp") = otp
        record("nam") = nam
        record("doc") = doc
        record("src") = src
        record("tgt") = tgt
        record("stp") = stp
        record("pid") = pid
        record("rid") = rid

        data.Add(record)

    End Sub


    Function ReadIniFile() As Boolean
        Dim server As String = ""
        Dim port As String = ""
        Dim database As String = ""
        Dim user As String = ""
        Dim password As String = ""
        Dim iniFileName As String = "archixml.ini"
        Dim exeDirectory As String = AppDomain.CurrentDomain.BaseDirectory
        Dim iniFile As String = Path.Combine(exeDirectory, iniFileName)

        If File.Exists(iniFile) Then
            Dim lines() As String = File.ReadAllLines(iniFile)

            For Each line As String In lines
                If line.StartsWith("server=") Then
                    server = line.Split("="c)(1)
                ElseIf line.StartsWith("port=") Then
                    port = line.Split("="c)(1)
                ElseIf line.StartsWith("database=") Then
                    database = line.Split("="c)(1)
                ElseIf line.StartsWith("user=") Then
                    user = line.Split("="c)(1)
                ElseIf line.StartsWith("password=") Then
                    password = line.Split("="c)(1)
                ElseIf line.StartsWith("directory=") Then
                    iniFilePath = line.Split("="c)(1)
                End If
            Next
            connectionString = "server=" & server & ";port=" & port & ";database=" & database & ";user=" & user & ";password=" & password
            If Strings.Len(connectionString) < 50 Then
                Console.WriteLine($"'{iniFileName}' incomplete, please correct (see '{exeDirectory}'")
                Return False
            Else
                Return True
            End If

        Else


                Dim iniContent As String = "[DatabaseConnection]" & Environment.NewLine &
                                        "server=" & Environment.NewLine &
                                        "port=" & Environment.NewLine &
                                        "database=" & Environment.NewLine &
                                        "user=" & Environment.NewLine &
                                        "password=" & Environment.NewLine &
                                        Environment.NewLine &
                                        "[Settings]" & Environment.NewLine &
                                        "directory="

            ' Create and write to the INI file
            Try
                File.WriteAllText(iniFile, iniContent)
                Console.WriteLine($"'{iniFileName}' not found. An empty INI file has been created in the directory.")
            Catch ex As Exception
                Console.WriteLine("Error creating INI file: " & ex.Message)
            End Try
            Return False

        End If



    End Function


    Function GetProperty(input As String) As String
        ' Loop through the array
        For i As Integer = 0 To Property_Array.GetLength(0) - 1
            If Property_Array(i, 0) = input Then
                GetProperty = Property_Array(i, 1).ToString
                Return GetProperty
            End If
        Next
        Return "" ' Return False indicating the value was not found
    End Function


End Module

