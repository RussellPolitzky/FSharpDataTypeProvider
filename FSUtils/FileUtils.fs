[<AutoOpen>]
module FileUtils

open System.IO
open System.Text.RegularExpressions

/// Gets all the files in the 
/// root and subdirectories of all 
/// files.
let getAllFiles dir searchPattern = 
    Directory.GetFiles(dir, searchPattern, SearchOption.AllDirectories)

type NameAndDirectory = { Name : string; Directory: string }

/// Returns the and directory for
/// a given file.
let getNameAndDirectory fullPath = 
    let fileInfo = new FileInfo(fullPath)
    { Name = fileInfo.Name; 
      Directory = fileInfo.DirectoryName }
