module ``Example Tests``

    open NUnit.Framework
    open FsUnit

    [<Test>]
    let ``a test`` () =
        true |> should equal true
