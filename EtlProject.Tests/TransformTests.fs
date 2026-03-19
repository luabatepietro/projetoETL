module TransformTests

open Expecto
open Types
open Transform

// ── Dados de teste ────────────────────────────────────────────────────────────

let sampleOrders = [
    { Id = 1; ClientId = 10; OrderDate = "2024-08-01T00:00:00"; Status = "Complete"; Origin = "O" }
    { Id = 2; ClientId = 11; OrderDate = "2024-08-15T00:00:00"; Status = "Pending";  Origin = "P" }
    { Id = 3; ClientId = 12; OrderDate = "2024-09-10T00:00:00"; Status = "Complete"; Origin = "O" }
    { Id = 4; ClientId = 13; OrderDate = "2024-09-20T00:00:00"; Status = "Cancelled";Origin = "O" }
]

let sampleItems = [
    { OrderId = 1; ProductId = 101; Quantity = 2; Price = 100.0; Tax = 0.10 }
    { OrderId = 1; ProductId = 102; Quantity = 1; Price = 50.0;  Tax = 0.05 }
    { OrderId = 2; ProductId = 103; Quantity = 3; Price = 30.0;  Tax = 0.12 }
    { OrderId = 3; ProductId = 104; Quantity = 4; Price = 25.0;  Tax = 0.08 }
]

let sampleJoined = [
    { OrderId = 1; OrderDate = "2024-08-01T00:00:00"; Status = "Complete"; Origin = "O"; Quantity = 2; Price = 100.0; Tax = 0.10 }
    { OrderId = 1; OrderDate = "2024-08-01T00:00:00"; Status = "Complete"; Origin = "O"; Quantity = 1; Price = 50.0;  Tax = 0.05 }
    { OrderId = 3; OrderDate = "2024-09-10T00:00:00"; Status = "Complete"; Origin = "O"; Quantity = 4; Price = 25.0;  Tax = 0.08 }
]

// ── Testes: parseOrder ────────────────────────────────────────────────────────

let parseOrderTests =
    testList "parseOrder" [
        test "deve parsear campos corretamente" {
            let fields = [| "1"; "10"; "2024-08-01T00:00:00"; "Complete"; "O" |]
            let result = parseOrder fields
            Expect.equal result.Id        1                    "Id incorreto"
            Expect.equal result.ClientId  10                   "ClientId incorreto"
            Expect.equal result.OrderDate "2024-08-01T00:00:00" "OrderDate incorreto"
            Expect.equal result.Status    "Complete"           "Status incorreto"
            Expect.equal result.Origin    "O"                  "Origin incorreto"
        }
        test "deve parsear status Pending" {
            let fields = [| "5"; "20"; "2024-01-01T00:00:00"; "Pending"; "P" |]
            let result = parseOrder fields
            Expect.equal result.Status "Pending" "Status deveria ser Pending"
            Expect.equal result.Origin "P"       "Origin deveria ser P"
        }
    ]

// ── Testes: parseOrderItem ────────────────────────────────────────────────────

let parseOrderItemTests =
    testList "parseOrderItem" [
        test "deve parsear campos numericos corretamente" {
            let fields = [| "1"; "101"; "2"; "100.0"; "0.10" |]
            let result = parseOrderItem fields
            Expect.equal result.OrderId   1     "OrderId incorreto"
            Expect.equal result.ProductId 101   "ProductId incorreto"
            Expect.equal result.Quantity  2     "Quantity incorreto"
            Expect.equal result.Price     100.0 "Price incorreto"
            Expect.equal result.Tax       0.10  "Tax incorreto"
        }
    ]

// ── Testes: filterOrders ─────────────────────────────────────────────────────

let filterOrdersTests =
    testList "filterOrders" [
        test "deve retornar apenas pedidos Complete e Online" {
            let result = filterOrders "Complete" "O" sampleOrders
            Expect.equal (List.length result) 2 "Deveria retornar 2 pedidos"
            Expect.all result (fun o -> o.Status = "Complete") "Todos deveriam ser Complete"
            Expect.all result (fun o -> o.Origin = "O")        "Todos deveriam ser Online"
        }
        test "deve ser case-insensitive no status" {
            let result = filterOrders "complete" "O" sampleOrders
            Expect.equal (List.length result) 2 "Deveria ignorar case no status"
        }
        test "deve retornar lista vazia quando nenhum pedido bate" {
            let result = filterOrders "Complete" "X" sampleOrders
            Expect.isEmpty result "Deveria retornar lista vazia"
        }
        test "deve retornar apenas pedidos Pending Physical" {
            let result = filterOrders "Pending" "P" sampleOrders
            Expect.equal (List.length result) 1 "Deveria retornar 1 pedido"
            Expect.equal result.[0].Id 2 "Deveria ser o pedido de id 2"
        }
    ]

// ── Testes: innerJoin ─────────────────────────────────────────────────────────

let innerJoinTests =
    testList "innerJoin" [
        test "deve combinar orders com seus itens" {
            let orders = sampleOrders |> List.filter (fun o -> o.Id = 1)
            let result  = innerJoin orders sampleItems
            Expect.equal (List.length result) 2 "Order 1 tem 2 itens"
            Expect.all result (fun j -> j.OrderId = 1) "Todos os itens devem ser do order 1"
        }
        test "deve retornar lista vazia quando order nao tem itens" {
            let orders = sampleOrders |> List.filter (fun o -> o.Id = 4)
            let result  = innerJoin orders sampleItems
            Expect.isEmpty result "Order 4 nao tem itens"
        }
        test "deve preservar a data do order no joined" {
            let orders = sampleOrders |> List.filter (fun o -> o.Id = 1)
            let result  = innerJoin orders sampleItems
            Expect.all result (fun j -> j.OrderDate = "2024-08-01T00:00:00") "Data deveria ser preservada"
        }
    ]

// ── Testes: calcRevenue ───────────────────────────────────────────────────────

let calcRevenueTests =
    testList "calcRevenue" [
        test "deve calcular preco vezes quantidade" {
            let item = { OrderId = 1; OrderDate = ""; Status = ""; Origin = ""; Quantity = 3; Price = 10.0; Tax = 0.1 }
            Expect.equal (calcRevenue item) 30.0 "3 * 10.0 = 30.0"
        }
        test "deve retornar zero quando quantidade e zero" {
            let item = { OrderId = 1; OrderDate = ""; Status = ""; Origin = ""; Quantity = 0; Price = 99.9; Tax = 0.1 }
            Expect.equal (calcRevenue item) 0.0 "0 * 99.9 = 0.0"
        }
    ]

// ── Testes: calcTax ───────────────────────────────────────────────────────────

let calcTaxTests =
    testList "calcTax" [
        test "deve calcular tax percentual sobre a receita" {
            let item = { OrderId = 1; OrderDate = ""; Status = ""; Origin = ""; Quantity = 2; Price = 100.0; Tax = 0.10 }
            Expect.equal (calcTax item) 20.0 "10% de 200.0 = 20.0"
        }
        test "deve retornar zero quando tax e zero" {
            let item = { OrderId = 1; OrderDate = ""; Status = ""; Origin = ""; Quantity = 5; Price = 50.0; Tax = 0.0 }
            Expect.equal (calcTax item) 0.0 "0% de qualquer valor = 0.0"
        }
    ]

// ── Testes: summarizeGroup ────────────────────────────────────────────────────

let summarizeGroupTests =
    testList "summarizeGroup" [
        test "deve somar receitas e impostos de todos os itens" {
            // item1: 2 * 100.0 = 200.0, tax = 20.0
            // item2: 1 * 50.0  = 50.0,  tax = 2.5
            let items = sampleJoined |> List.filter (fun j -> j.OrderId = 1)
            let result = summarizeGroup 1 items
            Expect.equal result.TotalAmount 250.0 "200.0 + 50.0 = 250.0"
            Expect.equal result.TotalTaxes  22.5  "20.0 + 2.5 = 22.5"
        }
        test "deve retornar zeros para lista vazia" {
            let result = summarizeGroup 99 []
            Expect.equal result.TotalAmount 0.0 "Sem itens, amount = 0"
            Expect.equal result.TotalTaxes  0.0 "Sem itens, taxes = 0"
        }
    ]

// ── Testes: buildSummaries ────────────────────────────────────────────────────

let buildSummariesTests =
    testList "buildSummaries" [
        test "deve gerar um summary por order distinto" {
            let result = buildSummaries sampleJoined
            Expect.equal (List.length result) 2 "Deve haver 2 summaries (orders 1 e 3)"
        }
        test "deve conter os order ids corretos" {
            let result  = buildSummaries sampleJoined
            let ids     = result |> List.map (fun s -> s.OrderId) |> List.sort
            Expect.equal ids [1; 3] "Order IDs devem ser 1 e 3"
        }
    ]

// ── Testes: parseYearMonth ────────────────────────────────────────────────────

let parseYearMonthTests =
    testList "parseYearMonth" [
        test "deve extrair ano e mes corretamente" {
            let (year, month) = parseYearMonth "2024-08-17T03:05:39"
            Expect.equal year  2024 "Ano deveria ser 2024"
            Expect.equal month 8    "Mes deveria ser 8"
        }
        test "deve funcionar com mes de um digito" {
            let (year, month) = parseYearMonth "2025-01-01T00:00:00"
            Expect.equal year  2025 "Ano deveria ser 2025"
            Expect.equal month 1    "Mes deveria ser 1"
        }
    ]

// ── Testes: formatSummaryLine ─────────────────────────────────────────────────

let formatSummaryLineTests =
    testList "formatSummaryLine" [
        test "deve formatar linha CSV corretamente" {
            let s = { OrderId = 1; TotalAmount = 250.0; TotalTaxes = 22.5 }
            Expect.equal (formatSummaryLine s) "1,250,22.5" "Formato CSV incorreto"
        }
    ]

// ── Testes: formatMonthlyLine ─────────────────────────────────────────────────

let formatMonthlyLineTests =
    testList "formatMonthlyLine" [
        test "deve formatar mes com dois digitos" {
            let m = { Year = 2024; Month = 8; AvgAmount = 300.0; AvgTaxes = 25.0 }
            Expect.equal (formatMonthlyLine m) "2024,08,300,25" "Mes deve ter dois digitos"
        }
        test "deve formatar mes de dezembro corretamente" {
            let m = { Year = 2024; Month = 12; AvgAmount = 100.5; AvgTaxes = 10.5 }
            Expect.equal (formatMonthlyLine m) "2024,12,100.5,10.5" "Dezembro deve ser 12"
        }
    ]

// ── Suite principal ───────────────────────────────────────────────────────────

let allTests =
    testList "EtlProject" [
        parseOrderTests
        parseOrderItemTests
        filterOrdersTests
        innerJoinTests
        calcRevenueTests
        calcTaxTests
        summarizeGroupTests
        buildSummariesTests
        parseYearMonthTests
        formatSummaryLineTests
        formatMonthlyLineTests
    ]
