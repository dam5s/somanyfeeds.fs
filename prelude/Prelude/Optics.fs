[<AutoOpen>]
module Optics

type Lens<'a, 'b> =
    { get: 'a  -> 'b
      set: 'b -> 'a -> 'a }

[<RequireQualifiedAccess>]
module Lens =
  let compose (lensAB: Lens<'a, 'b>) (lensBC: Lens<'b, 'c>): Lens<'a, 'c> =
      { get = lensAB.get >> lensBC.get
        set = fun c a -> lensAB.set (lensBC.set c (lensAB.get a)) a }

  module Operators =
      let (>=>) = compose
