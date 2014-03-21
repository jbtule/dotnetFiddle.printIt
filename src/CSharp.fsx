namespace System

#load "Fiddle.fsx"

open System.Runtime.CompilerServices

[<Extension>]
type Fiddle =
  [<Extension>]
  static member inline PrintAs (obj, name) = printAs name obj
  [<Extension>]
  static member inline PrintIt (obj) = printIt obj
