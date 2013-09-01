Imports System.Runtime.Serialization
Namespace mee

End Namespace
Module Module1

    Sub Main()
        Console.WriteLine("This script will export your anime-planet.com list and save it to anime-planet.xml")
        Console.WriteLine("Enter your AP username:")
        Dim username As String = Console.ReadLine
        Console.WriteLine("Enter your MAL username:")
        Dim malusername As String = Console.ReadLine

        Dim baseurl As String = "http://www.anime-planet.com/users/" & username & "/anime"
        Dim req As System.Net.WebRequest = System.Net.WebRequest.Create(baseurl)
        Dim html As String = New System.IO.StreamReader(req.GetResponse.GetResponseStream).ReadToEnd
        Dim pageNumber As Integer = html.Substring(html.IndexOf("<li class='next'>") - 10, 1)
        Dim queryURL = "http://mal-api.com/anime/search?q="

        baseurl = "http://myanimelist.net/profile/" & malusername
        req = System.Net.WebRequest.Create(baseurl)
        html = New System.IO.StreamReader(req.GetResponse.GetResponseStream).ReadToEnd
        Dim malid As String = html.Substring(html.IndexOf("value=", html.IndexOf("profileMemId")) + 7, html.IndexOf(Chr(34), html.IndexOf("value=", html.IndexOf("profileMemId")) + 7) - (html.IndexOf("value=", html.IndexOf("profileMemId")) + 7))


        Console.WriteLine("Exporting myanimelist format...")
        Dim f As New IO.StreamWriter(Environment.CurrentDirectory & "\anime-planet.xml", True, System.Text.Encoding.UTF8)
        f.WriteLine("<?xml version=" & Chr(34) & "1.0" & Chr(34) & " encoding=" & Chr(34) & "UTF-8" & Chr(34) & " ?>")
        f.WriteLine("<myanimelist>")
        f.WriteLine(vbTab & "<myinfo>")
        f.WriteLine(vbTab & vbTab & "<user_id>" & malid & "</user_id>")
        f.WriteLine(vbTab & vbTab & "<user_name>" & malusername & "</user_name>")
        f.WriteLine(vbTab & vbTab & "<user_export_type>1</user_export_type>")
        f.WriteLine(vbTab & vbTab & "</myinfo>")

        For i = 1 To pageNumber
            baseurl = "http://www.anime-planet.com/users/" & username & "/anime?page=" & i
            req = System.Net.WebRequest.Create(baseurl)
            html = New System.IO.StreamReader(req.GetResponse.GetResponseStream).ReadToEnd
            Dim trs As New ArrayList
            Dim index As Integer = html.IndexOf("<tr")
            While index <> -1
                index = html.IndexOf("<tr", index + 1)
                If index = -1 Then
                    Exit While
                End If
                trs.Add(html.Substring(html.IndexOf(">", index + 1) + 1, html.IndexOf("</tr>", html.IndexOf(">", index + 1)) - ((html.IndexOf(">", index + 1) + 1))))
            End While
            For Each t As String In trs
                Dim animeName As String = t.Substring(t.IndexOf(">", t.IndexOf("<a") + 1) + 1, t.IndexOf("<", t.IndexOf(">", t.IndexOf("<a") + 1) + 1) - (t.IndexOf(">", t.IndexOf("<a") + 1) + 1))
                Dim rating As String = t.Substring(t.IndexOf("name=", t.IndexOf("<img src=")) + 6, t.IndexOf(Chr(34), t.IndexOf("name=", t.IndexOf("<img src=")) + 6) - (t.IndexOf("name=", t.IndexOf("<img src=")) + 6))
                Dim animeID As Integer = 0
                Dim status As String = ""
                Dim matchfound As Boolean = False

                Dim type As String = t.Substring(t.IndexOf("tableType") + 11, t.IndexOf("<", t.IndexOf("tableType") + 1) - (t.IndexOf("tableType") + 11))
                If type = "DVD Special" Then : type = "Special" : End If
                Dim watchedeps As String = ((t.Substring(t.IndexOf("tableEps") + 10, t.IndexOf("<", t.IndexOf("tableEps") + 10) - (t.IndexOf("tableEps") + 10))).Replace(vbTab, "")).Replace(vbNewLine, "")
                If IsNumeric(watchedeps) = False Then : watchedeps = 0 : End If
                Dim rewatchedtimes As String = t.Substring(t.IndexOf("tableTimesWatched") + 19, t.IndexOf("</", t.IndexOf("tableTimesWatched") + 19) - (t.IndexOf("tableTimesWatched") + 19)).Replace(vbTab, "").Replace(" ", "").Replace("x", "").Replace(vbNewLine, "")
                If IsNumeric(rewatchedtimes) = False Then : rewatchedtimes = "1" : End If

                Dim AI As Info.AnimeInfo() = GetAnimeInfo(New Uri(queryURL & animeName).ToString)

                If AI.Length = 0 Then
                    My.Computer.FileSystem.WriteAllText(Environment.CurrentDirectory & "\anime_TODOLIST.txt", animeName & vbNewLine, True)
                End If
                For Each x In AI
                    Dim otherenglishtitles() As String = x.OtherTitles.english
                    Dim othersynonyms() As String = x.OtherTitles.synonyms
                    If animeName.ToLower = x.title.ToLower Then
                        animeID = x.id
                    ElseIf otherenglishtitles IsNot Nothing Then
                        For Each n In otherenglishtitles
                            If n.ToLower.Contains(x.title.ToLower) Then
                                animeID = x.id
                            End If
                        Next
                    ElseIf othersynonyms IsNot Nothing Then
                        For Each n In othersynonyms
                            If n.ToLower.Contains(x.title.ToLower) Then
                                animeID = x.id
                            End If
                        Next
                    Else
                        GoTo A
                    End If
                    Dim webstatus As String = (t.Substring(t.IndexOf("<!-- status box -->") + 19, t.IndexOf("<", t.IndexOf("<!-- status box -->") + 1) - (t.IndexOf("<!-- status box -->") + 19))).Replace(" ", "")
                    If webstatus.Contains("Watched") Then
                        status = "Completed"
                    ElseIf webstatus.Contains("Stalled") Then
                        status = "On-Hold"
                    ElseIf webstatus.Contains("WanttoWatch") Then
                        status = "Plan to Watch"
                    ElseIf webstatus.Contains("Won'tWatch") Then
                        status = "Dropped"
                    Else
                        status = webstatus
                    End If
                    f.WriteLine(vbTab & "<anime>")
                    f.WriteLine(vbTab & vbTab & "<series_animedb_id>" & animeID & "</series_animedb_id>")
                    f.WriteLine(vbTab & vbTab & "<series_title><![CDATA[" & animeName & "]]></series_title>")
                    f.WriteLine(vbTab & vbTab & "<series_type>" & type & "</series_type>")
                    f.WriteLine(vbTab & vbTab & "<series_episodes>" & x.episodes & "</series_episodes>")
                    f.WriteLine(vbTab & vbTab & "<my_id>0</my_id>")
                    f.WriteLine(vbTab & vbTab & "<my_watched_episodes>" & watchedeps & "</my_watched_episodes>")
                    f.WriteLine(vbTab & vbTab & "<my_start_date>0000-00-00</my_start_date>")
                    f.WriteLine(vbTab & vbTab & "<my_finish_date>0000-00-00</my_finish_date>")
                    f.WriteLine(vbTab & vbTab & "<my_fansub_group><![CDATA[0]]></my_fansub_group>")
                    f.WriteLine(vbTab & vbTab & "<my_rated></my_rated>")
                    f.WriteLine(vbTab & vbTab & "<my_score>" & CDbl(rating) * 2 & "</my_score>")
                    f.WriteLine(vbTab & vbTab & "<my_dvd></my_dvd>")
                    f.WriteLine(vbTab & vbTab & "<my_storage></my_storage>")
                    f.WriteLine(vbTab & vbTab & "<my_status>" & (status.Replace(vbTab, "")).Replace(vbNewLine, "") & "</my_status>")
                    f.WriteLine(vbTab & vbTab & "<my_comments><![CDATA[]]></my_comments>")
                    f.WriteLine(vbTab & vbTab & "<my_times_watched>" & CInt(rewatchedtimes) - 1 & "</my_times_watched>")
                    f.WriteLine(vbTab & vbTab & "<my_rewatch_value></my_rewatch_value>")
                    f.WriteLine(vbTab & vbTab & "<my_downloaded_eps>0</my_downloaded_eps>")
                    f.WriteLine(vbTab & vbTab & "<my_tags><![CDATA[]]></my_tags>")
                    f.WriteLine(vbTab & vbTab & "<my_rewatching>0</my_rewatching>")
                    f.WriteLine(vbTab & vbTab & "<my_rewatching_ep>0</my_rewatching_ep>")
                    f.WriteLine(vbTab & vbTab & "<update_on_import>1</update_on_import>")
                    f.WriteLine(vbTab & "</anime>" & vbNewLine)
                    Console.WriteLine("Processed anime {0} of {1} from page {2} of {3}", trs.IndexOf(t) + 1, trs.Count, i, pageNumber)
                    matchfound = True
                    Exit For
A:
                Next
                If (matchfound = False) And (AI.Length <> 0) Then
                    My.Computer.FileSystem.WriteAllText(Environment.CurrentDirectory & "\anime_TODOLIST.txt", animeName & vbNewLine, True)
                End If
            Next
        Next
        f.WriteLine("</myanimelist>")
        f.WriteLine(vbNewLine)
        f.Close()
        Console.WriteLine("Processing finished!")
        Console.ReadKey()
    End Sub

    Function GetAnimeInfo(ByVal url As String) As Info.AnimeInfo()
        Return GetDataAndParse(Of Info.AnimeInfo())(url)
    End Function

    Function GetDataAndParse(Of T As Class)(ByVal url As String) As T
        Dim result As T = Nothing
        Dim jsonBytes As Byte() = Nothing
        Using wc As New System.Net.WebClient()
            jsonBytes = wc.DownloadData(url)
        End Using
        Dim serializer As New System.Runtime.Serialization.Json.DataContractJsonSerializer(GetType(T))
        Using ms As New IO.MemoryStream(jsonBytes)
            result = DirectCast(serializer.ReadObject(ms), T)
        End Using
        Return result
    End Function
End Module

