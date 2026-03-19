module Transform

open Types

/// <summary>
/// Converte uma linha CSV (array de strings) em um record Order.
/// Helper function utilizada na etapa de Extract.
/// </summary>
/// <param name="fields">Array de strings com os campos da linha CSV na ordem: id, client_id, order_date, status, origin.</param>
/// <returns>Um record Order preenchido com os valores da linha.</returns>
let parseOrder (fields: string[]) : Order =
    { Id        = int fields.[0]
      ClientId  = int fields.[1]
      OrderDate = fields.[2]
      Status    = fields.[3]
      Origin    = fields.[4] }

/// <summary>
/// Converte uma linha CSV (array de strings) em um record OrderItem.
/// Helper function utilizada na etapa de Extract.
/// </summary>
/// <param name="fields">Array de strings com os campos da linha CSV na ordem: order_id, product_id, quantity, price, tax.</param>
/// <returns>Um record OrderItem preenchido com os valores da linha.</returns>
let parseOrderItem (fields: string[]) : OrderItem =
    { OrderId   = int    fields.[0]
      ProductId = int    fields.[1]
      Quantity  = int    fields.[2]
      Price     = float  fields.[3]
      Tax       = float  fields.[4] }

/// <summary>
/// Filtra uma lista de pedidos pelo status e origem fornecidos.
/// Funcao pura - nao possui efeitos colaterais.
/// </summary>
/// <param name="status">Status desejado (ex: "Complete", "Pending", "Cancelled"). Case-insensitive.</param>
/// <param name="origin">Origem desejada (ex: "O" para online, "P" para physical). Case-insensitive.</param>
/// <param name="orders">Lista de Orders a ser filtrada.</param>
/// <returns>Lista de Orders que satisfazem os criterios de status e origem.</returns>
let filterOrders (status: string) (origin: string) (orders: Order list) : Order list =
    orders
    |> List.filter (fun o ->
        o.Status.ToLower() = status.ToLower() &&
        o.Origin.ToUpper() = origin.ToUpper())

/// <summary>
/// Realiza o inner join entre uma lista de Orders e uma lista de OrderItems pelo campo OrderId.
/// Cada Order pode gerar multiplos registros joined (um por item).
/// Funcao pura - nao possui efeitos colaterais.
/// </summary>
/// <param name="orders">Lista de Orders filtrados.</param>
/// <param name="items">Lista completa de OrderItems.</param>
/// <returns>Lista de OrderJoined combinando os dados de cada Order com seus respectivos itens.</returns>
let innerJoin (orders: Order list) (items: OrderItem list) : OrderJoined list =
    orders
    |> List.collect (fun o ->
        items
        |> List.filter (fun i -> i.OrderId = o.Id)
        |> List.map (fun i ->
            { OrderId   = o.Id
              OrderDate = o.OrderDate
              Status    = o.Status
              Origin    = o.Origin
              Quantity  = i.Quantity
              Price     = i.Price
              Tax       = i.Tax }))

/// <summary>
/// Calcula a receita de um item joined (preco * quantidade).
/// Funcao pura - nao possui efeitos colaterais.
/// </summary>
/// <param name="item">Item joined com os campos Price e Quantity.</param>
/// <returns>Receita do item como float.</returns>
let calcRevenue (item: OrderJoined) : float =
    item.Price * float item.Quantity

/// <summary>
/// Calcula o imposto de um item joined (percentual de tax * receita do item).
/// Funcao pura - nao possui efeitos colaterais.
/// </summary>
/// <param name="item">Item joined com os campos Tax, Price e Quantity.</param>
/// <returns>Valor do imposto do item como float.</returns>
let calcTax (item: OrderJoined) : float =
    item.Tax * calcRevenue item

/// <summary>
/// Agrega todos os itens de um pedido em um OrderSummary com totais de receita e imposto.
/// Utiliza List.fold para acumular os valores de cada item.
/// Funcao pura - nao possui efeitos colaterais.
/// </summary>
/// <param name="orderId">Identificador do pedido.</param>
/// <param name="items">Lista de itens joined pertencentes ao pedido.</param>
/// <returns>Um OrderSummary com o total de receita e impostos do pedido, arredondados a 2 casas decimais.</returns>
let summarizeGroup (orderId: int) (items: OrderJoined list) : OrderSummary =
    let totalAmount = items |> List.fold (fun acc i -> acc + calcRevenue i) 0.0
    let totalTaxes  = items |> List.fold (fun acc i -> acc + calcTax i)    0.0
    { OrderId     = orderId
      TotalAmount = System.Math.Round(totalAmount, 2)
      TotalTaxes  = System.Math.Round(totalTaxes,  2) }

/// <summary>
/// Transforma a lista de itens joined em uma lista de OrderSummary agrupados por OrderId.
/// Utiliza List.groupBy para agrupar e List.map para transformar cada grupo.
/// Funcao pura - nao possui efeitos colaterais.
/// </summary>
/// <param name="joined">Lista de OrderJoined a ser agregada.</param>
/// <returns>Lista de OrderSummary, um por pedido.</returns>
let buildSummaries (joined: OrderJoined list) : OrderSummary list =
    joined
    |> List.groupBy (fun i -> i.OrderId)
    |> List.map (fun (orderId, items) -> summarizeGroup orderId items)

/// <summary>
/// Formata um OrderSummary como uma linha CSV no formato: order_id,total_amount,total_taxes.
/// Funcao pura - nao possui efeitos colaterais.
/// </summary>
/// <param name="s">OrderSummary a ser formatado.</param>
/// <returns>String com a linha CSV formatada.</returns>
let formatSummaryLine (s: OrderSummary) : string =
    $"{s.OrderId},{s.TotalAmount},{s.TotalTaxes}"

/// <summary>
/// Extrai o ano e o mes de uma string de data no formato ISO 8601 (ex: "2024-08-17T03:05:39").
/// Funcao pura - nao possui efeitos colaterais.
/// </summary>
/// <param name="dateStr">String de data no formato ISO 8601.</param>
/// <returns>Tupla (ano, mes) como inteiros.</returns>
let parseYearMonth (dateStr: string) : int * int =
    let parts = dateStr.Split('-')
    int parts.[0], int parts.[1]

/// <summary>
/// Calcula a media de receita e impostos dos summaries agrupados por mes e ano.
/// Utiliza List.groupBy para agrupar, List.fold para somar e divisao para calcular a media.
/// Funcao pura - nao possui efeitos colaterais.
/// </summary>
/// <param name="summaries">Lista de OrderSummary ja calculados.</param>
/// <param name="joined">Lista de OrderJoined com as datas dos pedidos.</param>
/// <returns>Lista de MonthlyAverage ordenada por ano e mes.</returns>
let buildMonthlyAverages (summaries: OrderSummary list) (joined: OrderJoined list) : MonthlyAverage list =
    joined
    |> List.groupBy (fun i -> parseYearMonth i.OrderDate)
    |> List.map (fun ((year, month), items) ->
        let orderIds = items |> List.map (fun i -> i.OrderId) |> List.distinct
        let relevantSummaries =
            summaries
            |> List.filter (fun s -> List.contains s.OrderId orderIds)
        let count = List.length relevantSummaries
        let avgAmount = relevantSummaries |> List.fold (fun acc s -> acc + s.TotalAmount) 0.0 |> fun t -> t / float count
        let avgTaxes  = relevantSummaries |> List.fold (fun acc s -> acc + s.TotalTaxes)  0.0 |> fun t -> t / float count
        { Year      = year
          Month     = month
          AvgAmount = System.Math.Round(avgAmount, 2)
          AvgTaxes  = System.Math.Round(avgTaxes,  2) })
    |> List.sortBy (fun m -> m.Year, m.Month)

/// <summary>
/// Formata um MonthlyAverage como uma linha CSV no formato: year,month,avg_amount,avg_taxes.
/// Funcao pura - nao possui efeitos colaterais.
/// </summary>
/// <param name="m">MonthlyAverage a ser formatado.</param>
/// <returns>String com a linha CSV formatada.</returns>
let formatMonthlyLine (m: MonthlyAverage) : string =
    $"{m.Year},{m.Month:D2},{m.AvgAmount},{m.AvgTaxes}"
