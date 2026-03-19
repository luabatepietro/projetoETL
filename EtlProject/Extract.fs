module Extract

open Types
open Transform

/// Faz o parse de um conteúdo CSV (string com quebras de linha) em linhas sem o cabeçalho
let private parseLines (content: string) : string[][] =
    content.Split('\n')
    |> Array.skip 1
    |> Array.filter (fun l -> l.Trim() <> "")
    |> Array.map (fun l -> l.Trim().Split(','))

/// Lê Orders a partir de um arquivo CSV local
let readOrdersFromFile (path: string) : Order list =
    System.IO.File.ReadAllText(path)
    |> parseLines
    |> Array.map parseOrder
    |> Array.toList

/// Lê OrderItems a partir de um arquivo CSV local
let readOrderItemsFromFile (path: string) : OrderItem list =
    System.IO.File.ReadAllText(path)
    |> parseLines
    |> Array.map parseOrderItem
    |> Array.toList


