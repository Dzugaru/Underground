namespace FLib

open UnityEngine
open System.Runtime.InteropServices

module Native =
    [<DllImport("NativeLib.dll", EntryPoint="test_native")>]
    extern int test_native(int i); 

type FClass() = 
    member this.X = "F#"

type FRecord = {First: float; Second: string; Third: int}

type SimpleComponent() =
    inherit MonoBehaviour()    

    [<SerializeField>]
    let mutable changeSpeed = 2.0f

    member this.stuff = 42

    member this.Start () = 
        Debug.Log("Native result: " + string(Native.test_native(5)))
        this.transform.Translate(1.0f, -1.0f, 2.0f)

