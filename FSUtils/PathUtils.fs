[<AutoOpen>]
module PathUtils

open System.IO
open System.Numerics
open System
open System.Text
open System.Text.RegularExpressions
open System.Reflection


/// Given absolute path, this function 
/// finds the nth parent recursing 
/// upward.
let moveUp n absoluteDirectory = 
    let rec findParent dir counter =
        let parent = Directory.GetParent(dir).FullName
        if (counter > 0) 
        then findParent parent (counter-1)
        else parent
    findParent absoluteDirectory n
    

/// Given a root path and a relative one,
/// this function calculates the resulting,
/// absolute path.
let calculatePath rootPath relativePath =
    Path.GetFullPath(Path.Combine(rootPath,relativePath))


/// Finds one path relative to another.
/// see http://social.msdn.microsoft.com/Forums/en-US/csharpgeneral/thread/954346c8-cbe8-448c-80d0-d3fc27796e9c/
let makePathRelativeTo absoluteRootPath absolutePath = 
    (new Uri(absolutePath))
        .MakeRelativeUri(new Uri(absoluteRootPath))
        .ToString();


/// Transforms a sequence of absolute paths 
/// to ones relative to the given, absolute 
/// relative path.
let makePathsRelativeTo absoluteRootPath (absolutePaths:seq<string>) = 
    let inline isLiteralPath (path:string) =
        not (path.StartsWith("!"))
    absolutePaths 
    |> Seq.map (fun absolutePath ->
                    if absolutePath |> isLiteralPath
                    then makePathRelativeTo absolutePath absoluteRootPath
                    else absolutePath.Trim([|'!'|]))


/// Converts the given file URL 
/// to a standard path.
let convertFromUrlToStandardPath fileUrl =
    let pattern = @"^*///(?<Path>.*)$"
    (Regex.Match(fileUrl, pattern).Groups.["Path"].Value).Replace('/', '\\')


/// Gets the codebase of an assembly as a standard path.
let getCodeBasePathOfAssembly (assembly:Assembly) =
    Path.GetDirectoryName(convertFromUrlToStandardPath (assembly.CodeBase))    


/// Find the absolute directory of a path which 
/// is a child (rootDirToLookFor) of a parent 
/// of startDir.
let findParentHavingRoot startDir rootDirToLookFor = 
    let rec findAbsDirRelativeToStartDir absDir = 
        let foundDir = 
            Directory.GetDirectories(absDir)
            |> Seq.tryFind (fun dir -> dir.EndsWith(rootDirToLookFor))
        match foundDir with
        | None     -> findAbsDirRelativeToStartDir (moveUp 0 absDir)
        | Some dir -> dir
    findAbsDirRelativeToStartDir startDir


/// Wrapper for Path.Combine()
let combinePaths path1 path2 = 
    Path.Combine(path1, path2)


/// Directory separator as a string.
let dirSep = new String([| Path.DirectorySeparatorChar |])

/// Append a path separator
/// to the given path if it 
/// doesn't already end with one.
let appendPathSep (path:string) = 
    if (path.EndsWith(dirSep))
    then path
    else path + dirSep