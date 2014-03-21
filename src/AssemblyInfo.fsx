namespace System
open System.Reflection

[<assembly: AssemblyTitleAttribute("printIt for dotnetFiddle")>]
[<assembly: AssemblyDescriptionAttribute("printIt and printAs for dotnet Fiddling")>]
[<assembly: AssemblyProductAttribute("dotnetFiddle.printIt")>]
[<assembly: AssemblyVersionAttribute("1.0.0.0")>]
[<assembly: AssemblyFileVersionAttribute("1.0.0.0")>]
do ()

module internal AssemblyVersionInformation =
    let [<Literal>] Version = "1.0.0.0"
