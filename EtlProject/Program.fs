module Program

open Extract
open Transform
open Load

[<EntryPoint>]
let main argv =
    // --- Parâmetros configuráveis ---
    let status = "Complete"
    let origin = "O"        // O = Online, P = Physical
    let outputPath        = "output.csv"
    let outputMonthlyPath = "output_monthly.csv"

    let orderPath     = "order.csv"
    let orderItemPath = "order_item.csv"

    printfn "=== ETL Project - Programação Funcional ==="
    printfn "Filtro: status=%s, origin=%s" status origin

    // --- EXTRACT ---
    printfn "\n[1/3] Extraindo dados..."
    let orders     = readOrdersFromFile orderPath
    let orderItems = readOrderItemsFromFile orderItemPath
    printfn "  Orders carregadas: %d" (List.length orders)
    printfn "  OrderItems carregados: %d" (List.length orderItems)

    // --- TRANSFORM ---
    printfn "\n[2/3] Transformando dados..."
    let filteredOrders = filterOrders status origin orders
    printfn "  Pedidos após filtro: %d" (List.length filteredOrders)

    let joined = innerJoin filteredOrders orderItems
    printfn "  Itens após join: %d" (List.length joined)

    let summaries = buildSummaries joined
    printfn "  Summaries gerados: %d" (List.length summaries)

    let monthlyAverages = buildMonthlyAverages summaries joined
    printfn "  Médias mensais geradas: %d" (List.length monthlyAverages)

    // --- LOAD ---
    printfn "\n[3/3] Carregando saída..."
    writeSummariesToFile outputPath summaries
    writeMonthlyAveragesToFile outputMonthlyPath monthlyAverages

    printfn "\nProcessamento concluído com sucesso!"
    0
