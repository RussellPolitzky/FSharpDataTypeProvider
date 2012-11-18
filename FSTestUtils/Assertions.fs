module Assertions

open Microsoft.VisualStudio.TestTools.UnitTesting    
open System.Diagnostics
open System
open System.IO
open System.Diagnostics
open System.Windows

/// Wrapper for Assert.AreEqual
[<DebuggerStepThrough>]
let Equals (a:'a) (b:'a) = 
    Assert.AreEqual(a,b)

/// Wrapper for NotEqualTo
[<DebuggerStepThrough>]
let NotEqualTo (a:'a) (b:'a) = 
    Assert.AreNotEqual(a,b)

/// Wrapper for Assert.AreSame
[<DebuggerStepThrough>]
let SameAs (a:'a) (b:'a) = 
    Assert.AreSame(a,b)

/// Wrapper for Assert.AreNotSame
[<DebuggerStepThrough>]
let NotSameAs (a:'a) (b:'a) = 
    Assert.AreNotSame(a,b)

/// Wrapper for Assert.IsTrue
[<DebuggerStepThrough>]
let IsTrue (a:bool) = 
    Assert.IsTrue(a)

/// Wrapper for Assert.IsFalse
[<DebuggerStepThrough>]
let IsFalse (a:bool) = 
    Assert.IsFalse(a)


/// Ensures that the expected exception is thrown.
/// Allows the following syntax:
///
/// (fun () -> getAllReferencedFiles @"SampleFilesCircularRef\a.js"
///                   |> Seq.map (fun i -> Path.GetFileName(i)) 
///                   |> Seq.toSingleSringWithSep ","
///                   |> Equals "a.js,c.js,e.js,d.js,f.js,b.js,g.js,h.js")
/// |> throwsException typeof<Exception> 
///
/// Note the use of the lambda above.  Simply wrap the 
/// code which should throw in the lambda and pipe that 
/// to this function.
[<DebuggerStepThrough>]
let throwsException (typeOfException:Type) (codeBlock:unit->unit) = 
    try
      codeBlock() // Execute the lambda function.
      failwith (sprintf "Expected exception of type %s to be thrown" 
                 (typeOfException.Name))
    with
      | ex -> if (ex.GetType() <> typeOfException)
              then failwith (sprintf "Expected exception of type %s to be thrown but received one of type %s instead." 
                              (typeOfException.Name) (ex.GetType().Name)) 
                   () // returns unit
              else () // returns unit


/// Opens a set of files in a diff viewer.
let openDiff file1 file2 =
    let startInfo = new ProcessStartInfo()
    startInfo.FileName <- @"C:\Program Files\Perforce\p4merge.exe"
    startInfo.Arguments <- sprintf "%s %s" file1 file2
    Process.Start(startInfo);

/// Useable tempfile (see: http://fssnip.net/4N/)
type TempFile() =
     let path = System.IO.Path.GetTempFileName()
     member x.Path = path
     interface System.IDisposable with
         member x.Dispose() = System.IO.File.Delete(path)

/// Predicate for matching strings.
let stringsDiffer s1 s2 = 
    not (s1 = s2)

/// Uses a tester function to determine 
/// if the given strings, s1 and s2 match.
let testStrings' testFails expected actual  = 

    let diffMessage = 
        Environment.NewLine + "-------------------------" + 
        Environment.NewLine + 
        "This string has been copied to the clipboard." + Environment.NewLine + 
        "To approve, simply paste it into your test."
    
    // Escape this to create a verabtim F# string that
    // may be pasted directly into the test code.
    let buildEscapedStringForClipboard (str:string) = 
        let doubleQuotesEscaped = str.Replace(@"""", @"""""")
        sprintf @"@""%s""" doubleQuotesEscaped

    let copyToClipboard str = 
        try 
            System.Windows.Clipboard.SetData(
                DataFormats.Text, 
                buildEscapedStringForClipboard str);
        with 
            | ex -> () // swallow any COM exceptions here. ... 

    // True if a debugger is attached to the 
    // process running the tests.
    let debuggerIsAttached =                 
        System.Diagnostics.Debugger.IsAttached

    if not debuggerIsAttached then
        Assert.AreEqual(expected, actual) // Assume running on CI server.
    else
        if testFails expected actual
        then // Show any change in diff viewer and copy received to clipboard.
            use tempFileExpected = new TempFile()
            use tempFileActual   = new TempFile()
            let tempFiles = [| tempFileExpected.Path; tempFileActual.Path |] 
            [| expected; actual + diffMessage |] 
            |> Array.zip tempFiles 
            |> Array.iter (fun tpl -> File.WriteAllText((fst tpl), (snd tpl)))
            openDiff tempFileExpected.Path tempFileActual.Path |> ignore
            actual |> copyToClipboard
            failwith (sprintf "Expected <%s>, but actual is <%s>." expected actual)

/// Tests the given strings for equality.
/// If they're found to differ, then the 
/// p4merge tools is opened to show the 
/// differences.  The received string is 
/// escaped and sent to the clipboard 
/// so that it may be easily copied to the 
/// relvant test to accept the changes if 
/// they're expected.
[<DebuggerStepThrough>]        
let IsSameStringAs = testStrings' stringsDiffer

