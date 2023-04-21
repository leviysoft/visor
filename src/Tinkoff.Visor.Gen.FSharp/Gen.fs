namespace Tinkoff.Visor.Gen.FSharp
    
open FSharp.Quotations
open Tinkoff.Visor
open System
open TypeShape.Clone
    
module Gen =
    let mkLens<'T, 'F> (expr : Expr<'T -> 'F>) : ILens<'T, 'F> =
        let path = Impl.extractPath expr
        let lens = Impl.mkLensAux<'T, 'F> path
        
        Lens.New(
            lens.get,
            // TypeShape native lenses rely on mutation, so we clone to hide this fact
            fun (f: Func<'F, 'F>) -> Func<'T, 'T>(fun t -> lens.set (clone t) (f.Invoke(lens.get t)))
        )