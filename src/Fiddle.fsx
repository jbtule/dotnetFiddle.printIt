// Copyright 2014 Jay Tuley <jay+code@tuley.name>
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

#load "SigPrint.fsx"

namespace global

open System.Runtime.CompilerServices
open Swensen.Unquote.ExtraReflection

[<AutoOpen>]
module TopLevel =
  let printAs name obj =
    printfn "val %s : %s = %A" name (sprintSig <| obj.GetType()) obj
    obj
  let printIt obj = printAs "it" obj

[<Extension>]
type Fiddle =
  [<Extension>]
  static member inline PrintAs (obj, name) = printAs name obj
  [<Extension>]
  static member inline PrintIt (obj) = printIt obj