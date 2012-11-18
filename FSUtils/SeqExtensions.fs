[<AutoOpen>]
module Seq

open System.Text
open StringUtils

let toString a = a.ToString()

/// Converts the given collection to a single string
/// using the given string converter and separator.
/// This version of the function is not usually used 
/// directly.
let inline toSingleSringUsingConverterAndSeparator (toStringConverter:'a->string) (sep:string) s = 
    (s |> Seq.fold (fun (sb:StringBuilder) i -> sb.Append(toStringConverter i).Append(sep)) (new StringBuilder())
       |> toString).TrimEnd(sep.ToCharArray())
    
/// Converts the given collection to a single string
/// using the type's ToString() method.
let toSingleSring s = 
    toSingleSringUsingConverterAndSeparator (fun a -> a.ToString()) "" s

/// Converts the given collection to a single string
/// using the using the given string converter.
let toSingleSringUsing (toStringConverter:'a->string) s = 
    toSingleSringUsingConverterAndSeparator toStringConverter "" s   
      
/// Converts the given collection to a single string
/// with the given sepatrator.
let toSingleSringWithSep sep s = 
    toSingleSringUsingConverterAndSeparator (fun a -> a.ToString()) sep s   

/// Tests to see if the given collection 
/// has duplicates.
let hasDuplicates (s:_) = 
    let dictionary = new System.Collections.Generic.HashSet<_>()
    s |> Seq.exists (fun i -> not(dictionary.Add(i)))

    




        