(*
Copyright 2011 Stephen Swensen

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*)

///Extra reflection functions sprinting and reducing Quotation Expressions
module internal Swensen.Unquote.ExtraReflection
open System
open System.Reflection
open Microsoft.FSharp.Reflection

open System.Text.RegularExpressions
//Regex.CacheSize <- (default is 15)
///Match the pattern using a cached interpreted Regex
let (|InterpretedMatch|_|) pattern input =
    if input = null then None
    else
        let m = Regex.Match(input, pattern)
        if m.Success then Some [for x in m.Groups -> x]
        else None

///Match the pattern using a cached compiled Regex
let (|CompiledMatch|_|) pattern input =
    if input = null then None
    else
        let ro =
            RegexOptions.Compiled
        let m = Regex.Match(input, pattern, ro)
        if m.Success then Some [for x in m.Groups -> x]
        else None



let private applyParensForPrecInContext context prec s = if prec > context then s else sprintf "(%s)" s

//the usefullness of this function makes me think to open up Sprint module (currently just added TypeExt with this feature)
///Sprint the F#-style type signature of the given Type.  Handles known type abbreviations,
///simple types, arbitrarily complex generic types (multiple parameters and nesting),
///lambdas, tuples, and arrays.
let sprintSig (outerTy:Type) =
    //list of F# type abbrs: http://207.46.16.248/en-us/library/ee353649.aspx
    ///Get the type abbr name or short name from the "clean" name
    let displayName = function
        | "System.Object"   -> "obj"
        | "System.String"   -> "string"
        | "System.Char"     -> "char"
        | "System.Boolean"  -> "bool"
        | "System.Decimal"  -> "decimal"

        | "System.Int16"    -> "int16"
        | "System.Int32"    -> "int"//int32
        | "System.Int64"    -> "int64"

        | "System.UInt16"   -> "uint16"
        | "System.UInt32"   -> "uint32"
        | "System.UInt64"   -> "uint64"

        | "System.Single"   -> "float32"//single
        | "System.Double"   -> "float"//double

        | "System.Byte"     -> "byte"//uint8
        | "System.SByte"    -> "sbyte"//int8

        | "System.IntPtr"   -> "nativeint"
        | "System.UIntPtr"  -> "unativeint"

        | "System.Numerics.BigInteger"  -> "bigint"
        | "Microsoft.FSharp.Core.Unit"  -> "unit"
        | "Microsoft.FSharp.Math.BigRational"   -> "BigNum"
        | "Microsoft.FSharp.Core.FSharpRef"     -> "ref"
        | "Microsoft.FSharp.Core.FSharpOption"  -> "option"
        | "Microsoft.FSharp.Collections.FSharpList" -> "list"
        | "Microsoft.FSharp.Collections.FSharpMap"  -> "Map"
        | "System.Collections.Generic.IEnumerable"  -> "seq"
        | CompiledMatch @"[\.\+]?([^\.\+]*)$" [_;nameMatch] -> nameMatch.Value //short name
        | cleanName -> failwith "failed to lookup type display name from it's \"clean\" name: " + cleanName

    let rec sprintSig context (ty:Type) =
        let applyParens = applyParensForPrecInContext context
        let cleanName, arrSig =
            //if is generic type, then doesn't have FullName, need to use just Name
            match (if String.IsNullOrEmpty(ty.FullName) then ty.Name else ty.FullName) with
            | CompiledMatch @"^([^`\[]*)`?.*?(\[[\[\],]*\])?$" [_;cleanNameMatch;arrSigMatch] -> //long name type encoding left of `, array encoding at end
                cleanNameMatch.Value, arrSigMatch.Value
            | _ ->
                failwith ("failed to parse type name: " + ty.FullName)

        match ty.GetGenericArguments() with
        | args when FSharpType.IsTuple ty ->
            (applyParens (if arrSig.Length > 0 then 0 else 3) (sprintf "%s" (args |> Array.map (sprintSig 3) |> String.concat " * "))) +  arrSig
        | args when FSharpType.IsFunction ty -> //right assoc, binding not as strong as tuples
            let lhs, rhs = FSharpType.GetFunctionElements ty
            (applyParens (if arrSig.Length > 0 then 0 else 2) (sprintf "%s -> %s" (sprintSig 2 lhs) (sprintSig 1 rhs))) + arrSig
        | args when args.Length = 0 ->
            (if outerTy.IsGenericTypeDefinition then "'" else "") + (displayName cleanName) + arrSig
        | args ->
            sprintf "%s<%s>%s" (displayName cleanName) (args |> Array.map (sprintSig 1) |> String.concat ", ") arrSig

    sprintSig 0 outerTy
