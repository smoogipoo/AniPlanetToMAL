Imports System.Runtime.Serialization

Public Class Info
    <DataContract()>
    Public Class AnimeInfo
        <DataMember(Name:="id")>
        Public id As String

        <DataMember(Name:="title")>
        Public title As String

        <DataMember(Name:="type")>
        Public type As String

        <DataMember(Name:="episodes")>
        Public episodes As String

        <DataMember(Name:="other_titles")>
        Public OtherTitles As OtherTitlesClass
    End Class
    <DataContract()>
    Public Class OtherTitlesClass
        <DataMember(Name:="english")>
        Public english() As String

        <DataMember(Name:="synonyms")>
        Public synonyms() As String
    End Class
End Class
