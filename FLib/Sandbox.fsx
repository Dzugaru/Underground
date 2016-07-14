

//http://fsharpforfunandprofit.com/monadster/

type DeadBodyPart = DeadBodyPart of string
type LiveBodyPart = LiveBodyPart of string
type VitalForce = VitalForce of int

type M<'a> = M of (VitalForce -> 'a * VitalForce)

let getDeadBodyPartEssence (DeadBodyPart essence) = essence    

let create deadBodyPart = 
    let essence = getDeadBodyPartEssence deadBodyPart

    let makeAlive (VitalForce vf) = 
        let remainingVitalForce = VitalForce (vf - 1)
        (LiveBodyPart essence), remainingVitalForce

    M makeAlive

let ret x = 
    let makeAlive vf = 
        x, vf
    M makeAlive


let runM vitalForce m = 
    let (M makeAlive) = m
    makeAlive vitalForce   

let apply wrappedF = 
    let (M m) = wrappedF
    let fm x' =
        let makeAlive vf =
            let f, vf' = m vf
            let x, vf'' = runM vf' x'
            (f x), vf''
        M makeAlive
    fm
    
let map2 f = 
    let fm x' y' = 
        let makeAlive vf = 
            let x, remVf = runM vf x'
            let y, remVf2 = runM remVf y'
            (f x y), remVf2
        M makeAlive
    fm

let assembleTwo x y =
    let (LiveBodyPart essenceX) = x
    let (LiveBodyPart essenceY) = y
    LiveBodyPart (essenceX + "+" + essenceY)

let (<*>) f = 
    apply f

let assembleTwoM = map2 assembleTwo
     
let liveArmM = create (DeadBodyPart "Arm")
let liveLegM = create (DeadBodyPart "Leg")
let lightningVitalForce = VitalForce 10

//let result = assembleTwoM liveArmM liveLegM |> runM lightningVitalForce
let result = (ret assembleTwo) <*> liveArmM <*> liveLegM |> runM lightningVitalForce

printfn "%A" result







