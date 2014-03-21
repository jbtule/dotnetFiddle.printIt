#I "packages/FAKE/tools"
#r "FakeLib.dll"
#I "packages/FSharp.Compiler.Service/lib/net40/"
#r "FSharp.Compiler.Service.dll"

open Fake
open System
open Microsoft.FSharp.Compiler.SimpleSourceCodeServices

exception CompilerError of string

let srcDir = "./src"
let buildDir = "./build"
let deployDir = "./deploy"

let projectName = "dotnetFiddle.printIt"
let title = "printIt for dotnetFiddle"
let version = sprintf "1.0.0.%s" <| environVarOrDefault "APPVEYOR_BUILD_NUMBER" "0"
let authors = ["Jay Tuley"]
let description = "printIt and printAs for dotnet Fiddling"

Target "Clean" (fun () ->
    trace " --- Cleaning stuff --- "
    CleanDir buildDir
)

Target "Build" (fun () ->
  trace " --- Building the app --- "

  CreateDir buildDir

  AssemblyInfoFile.CreateFSharpAssemblyInfo (srcDir + "/AssemblyInfo.fsx")
        [AssemblyInfoFile.Attribute.Title title
         AssemblyInfoFile.Attribute.Description description
         AssemblyInfoFile.Attribute.Product projectName
         AssemblyInfoFile.Attribute.Version version
         AssemblyInfoFile.Attribute.FileVersion version
         ]

  let scs = SimpleSourceCodeServices()

  let files = !! (srcDir + "/*.fsx")

  let args = ["fsc.exe"; "-a"; "-o" ; FullName (buildDir+ "/Fiddle.dll") ]
              @ (files |> Seq.toList)

  let errors,errorCode = scs.Compile(args |> List.toArray)

  for e in errors do
    let errMsg = e.ToString()
    match e.Severity with
        | Microsoft.FSharp.Compiler.Warning -> traceImportant errMsg
        | Microsoft.FSharp.Compiler.Error -> traceError errMsg
  if errorCode = 0 then
      trace "Compile Success"
  else
      raise (CompilerError "Compile Failed")
)

Target "Deploy" (fun () ->
    trace " --- Deploying app --- "
    NuGet (fun p ->
        {p with
            ToolPath = FullName "./packages/NuGet/NuGet.exe"
            Authors = authors
            Project = projectName
            OutputPath = deployDir
            WorkingDir = buildDir
            Description = description
            Version = version
            Publish = false })
            (deployDir + "/" + projectName + ".nuspec")
)

"Clean"
  ==> "Build"
  ==> "Deploy"

RunTargetOrDefault "Deploy"
