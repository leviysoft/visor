namespace Tinkoff.Visor.Gen.FSharp

open FSharp.Quotations
open FSharp.Quotations.Patterns
open TypeShape.Core

(*
    Initially based on http://www.fssnip.net/7VY/title/Lens-Generation-using-TypeShape
*)

type private Lens<'T, 'F> =
    {
        get : 'T -> 'F
        set : 'T -> 'F -> 'T
    }

module private Impl =
    type Path = Node list
    and Node =
        | Property of string
        | Item of int

    // converts a quotation of the form <@ fun x -> x.Foo.Bar.[0].Baz @> into a path
    let extractPath (e : Expr<'T -> 'F>) : Path =
        let rec aux v acc e =
            match e with
            | Var v' when v = v' -> acc
            | PropertyGet(Some o, p, [Value((:? int as i), _)]) when p.Name = "Item" -> aux v (Item i :: acc) o
            | PropertyGet(Some o, p, []) -> aux v (Property p.Name :: acc) o
            | Call(None, m, [o ; Value(:? int as i, _)]) when m.Name = "GetArray" && o.Type.IsArray && e.Type = o.Type.GetElementType() -> aux v (Item i :: acc) o
            // we support tuples, as they are often used to encode fields in erased type providers
            | TupleGet(x, i) -> aux v (Item i :: acc) x
            | _ -> invalidArg "expr" "invalid lens expression"

        match e with
        | Lambda(v, body) -> aux v [] body
        | _ -> invalidArg "expr" "lens expressions must be lambda literals"

    let rec mkLensAux<'T, 'F> (path : Path) : Lens<'T, 'F> =
        let wrap (l : Lens<'a,'b>) : Lens<'T, 'F> = unbox l

        let nest chain (m : IShapeMember<'T>) =
            m.Accept { new IMemberVisitor<'T, Lens<'T, 'F>> with
                member _.Visit<'F0> (m : ShapeMember<'T, 'F0>) =
                    let inner = mkLensAux<'F0, 'F> chain
                    {
                        get = fun (t:'T) -> inner.get (m.Get t)
                        set = fun (t:'T) (f:'F) -> m.Set t (inner.set (m.Get t) f)
                    }
        
            }

        match shapeof<'T>, path with
        | _, [] -> wrap { get = id<'F> ; set = fun (_:'F) (y:'F) -> y }
        | Shape.FSharpList s, Item i :: rest ->
            s.Element.Accept { new ITypeVisitor<Lens<'T,'F>> with
                member _.Visit<'t> () =
                    let inner = mkLensAux<'t, 'F> rest
                    wrap {
                        get = fun (ts : 't list) -> inner.get ts.[i]
                        set = fun (ts : 't list) (f : 'F) -> ts |> List.mapi (fun j t -> if j = i then inner.set t f else t)
                    }
            }

        | Shape.FSharpOption s, Property "Value" :: rest ->
            s.Element.Accept { new ITypeVisitor<Lens<'T,'F>> with
                member _.Visit<'t> () =
                    let inner = mkLensAux<'t, 'F> rest
                    wrap {
                        get = fun (ts : 't option) -> inner.get (Option.get ts)
                        set = fun (ts : 't option) (f : 'F) -> inner.set (Option.get ts) f |> Some
                    }
            }

        | Shape.Tuple (:? ShapeTuple<'T> as s), Item i :: rest ->
            s.Elements.[i] |> nest rest

        | Shape.Array s, Item i :: rest when s.Rank = 1 ->
            s.Element.Accept { new ITypeVisitor<Lens<'T,'F>> with
                member _.Visit<'t> () =
                    let inner = mkLensAux<'t, 'F> rest
                    wrap {
                        get = fun (ts : 't[]) -> inner.get ts.[i]
                        set = fun (ts : 't[]) (f : 'F) ->  ts.[i] <- inner.set ts.[i] f ; ts
                    }
            }

        | Shape.FSharpRecord (:? ShapeFSharpRecord<'T> as s) & Shape.FSharpRef _, Property "Value" :: rest ->
            s.Fields |> Array.find (fun p -> p.Label = "contents") |> nest rest

        | Shape.FSharpRecord (:? ShapeFSharpRecord<'T> as s), Property id :: rest ->
            s.Fields |> Array.find (fun p -> p.Label = id) |> nest rest

        | _ -> failwithf "unsupported lens type %O" typeof<'T>