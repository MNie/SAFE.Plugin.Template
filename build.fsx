#r "paket: groupref build //"
#load "./.fake/build.fsx/intellisense.fsx"

#if !FAKE
#r "netstandard"
#r "Facades/netstandard" // https://github.com/ionide/ionide-vscode-fsharp/issues/839#issuecomment-396296095
#endif

open System

open Fake.Core
open Fake.DotNet
open Fake.IO
open Fake.IO.FileSystemOperators
open Fake.IO.Globbing.Operators
open Fake.Tools

let templatePath = "./src/Content/.template.config/template.json"
let versionFilePath = "./src/Content/src/Runner/Version.fs"
let templateProj = "SAFE.Plugin.Template.proj"
let templateName = "SAFE-Stack Plugin"
let nupkgDir = Path.getFullName "./nupkg"

let release = ReleaseNotes.load "RELEASE_NOTES.md"

let formattedRN =
    release.Notes
    |> List.map (sprintf "* %s")
    |> String.concat "\n"

Target.create "Clean" (fun _ ->
    Shell.cleanDirs [ nupkgDir ]
    Git.CommandHelper.directRunGitCommandAndFail "./src/Content" "clean -fxd"
)

Target.create "Pack" (fun _ ->
    Shell.regexReplaceInFileWithEncoding
        "  \"name\": .+,"
       ("  \"name\": \"" + templateName + " v" + release.NugetVersion + "\",")
        System.Text.Encoding.UTF8
        templatePath
    //Shell.regexReplaceInFileWithEncoding
    //    "let template = \".+\""
    //   ("let template = \"" + release.NugetVersion + "\"")
    //    System.Text.Encoding.UTF8
    //    versionFilePath
    DotNet.pack
        (fun args ->
            { args with
                    OutputPath = Some nupkgDir
                    Common =
                        { args.Common with
                            CustomParams =
                                Some (sprintf "/p:PackageVersion=%s /p:PackageReleaseNotes=\"%s\""
                                        release.NugetVersion
                                        formattedRN) }
            })
        templateProj
)

Target.create "Install" (fun _ ->
    let args =
      let packages = [ "SAFE.Plugin.Template.Runner"; "SAFE.Plugin.Template.Plugin" ]
      let create arg =
          let nupkgFileName = sprintf "SAFE.Plugin.Template.%s.nupkg" release.NugetVersion
          let fullPathToNupkg = System.IO.Path.Combine(nupkgDir, nupkgFileName)
          sprintf "-i \"%s\"" fullPathToNupkg
      packages |> List.map create
    args
    |> List.iter (fun arg -> 
        let result = DotNet.exec (fun x -> { x with DotNetCliPath = "dotnet" }) "new" arg
        if not result.OK then failwithf "`dotnet %s` failed with %O" arg result
    )
)

let psi exe arg dir (x: ProcStartInfo) : ProcStartInfo =
    { x with
        FileName = exe
        Arguments = arg
        WorkingDirectory = dir }

let run exe arg dir =
    let result = Process.execWithResult (psi exe arg dir) TimeSpan.MaxValue
    if not result.OK then (failwithf "`%s %s` failed: %A" exe arg result.Errors)

let fullLockFileName build client server =
    sprintf "paket_%O_%O_%O.lock" build client server

let runPaket args wd =
    run "paket" args wd

Target.create "Tests" (fun _ ->
    let cmd = "run"
    let args = "--project tests/tests.fsproj"
    let result = DotNet.exec (fun x -> { x with DotNetCliPath = "dotnet" }) cmd args
    if not result.OK then failwithf "`dotnet %s %s` failed" cmd args
)

Target.create "Push" (fun _ ->
    Paket.push ( fun args ->
        { args with
                PublishUrl = "https://www.nuget.org"
                WorkingDir = nupkgDir
        }
    )

    let remoteGit = "upstream"
    let commitMsg = sprintf "Bumping version to %O" release.NugetVersion
    let tagName = string release.NugetVersion

    Git.Branches.checkout "" false "master"
    Git.CommandHelper.directRunGitCommand "" "fetch origin" |> ignore
    Git.CommandHelper.directRunGitCommand "" "fetch origin --tags" |> ignore

    Git.Staging.stageAll ""
    Git.Commit.exec "" commitMsg
    Git.Branches.pushBranch "" remoteGit "master"

    Git.Branches.tag "" tagName
    Git.Branches.pushTag "" remoteGit tagName
)

Target.create "Release" ignore

open Fake.Core.TargetOperators

"Clean"
    ==> "Pack"
    ==> "Install"
    ==> "Tests"
    ==> "Push"
    ==> "Release"

Target.runOrDefaultWithArguments "Install"
