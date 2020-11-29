open System
open System.IO
open System.Net
open System.Text.RegularExpressions

let getBook (url:string) = (new WebClient()).DownloadString(url)

let getChars (file:string) = ((new Regex(@"[a-z]")).Matches(file.ToLower())) 
                                |> Seq.cast 
                                |> Seq.map (fun (f:Match) -> f.Value.[0])

let freq (chars: seq<char>) = let len = double (chars |> Seq.length) in
                                    chars |> Seq.groupBy (fun t -> t)
                                          |> Seq.map (fun f -> (fst f,  (double (Seq.length (snd f))/len)))

let result = (getBook "https://www.gutenberg.org/files/2600/2600-0.txt") 
                |> getChars 
                |> freq

result |> Seq.sortBy (fun f -> fst f) |> Seq.iter (fun f -> printfn "%c - %f" (fst f) (snd f))

printfn "%f" <| (result |> Seq.map (fun f -> snd f) |> Seq.sum)
printfn "%f" <| (result |> Seq.map (fun f -> Math.Pow((snd f), 2.0)) |> Seq.sum)