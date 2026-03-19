# ETL Project — Programação Funcional (Aula 6)

**Disciplina:** Programação Funcional — Insper 2025-1  
**Projeto:** ETL em F# com dados de pedidos  

---

## Uso de IA Generativa

Este projeto foi desenvolvido com auxílio do Claude (Anthropic) para geração da estrutura inicial de código e do relatório. Todo o código foi revisado e validado pelo autor.

---

## Descrição do Projeto

Pipeline ETL que lê dois arquivos CSV (`order.csv` e `order_item.csv`), aplica filtros e transformações funcionais, e gera um CSV de saída com o total de receita e impostos por pedido.

---

## Estrutura do Projeto

```
EtlProject/
├── EtlProject.fsproj   # Projeto .NET — define ordem de compilação
├── Types.fs            # Records: Order, OrderItem, OrderJoined, OrderSummary
├── Transform.fs        # Funções PURAS: parse, filter, join, fold, map
├── Extract.fs          # Funções IMPURAS: leitura de arquivo e HTTP
├── Load.fs             # Funções IMPURAS: escrita do CSV de saída
└── Program.fs          # Ponto de entrada e rotina principal
```

A ordem dos arquivos no `.fsproj` é obrigatória: cada arquivo só pode referenciar o que foi declarado antes dele.

---

## Como Reproduzir

### Pré-requisitos
- [.NET SDK 8.0+](https://dotnet.microsoft.com/download)

### Passos

```bash
# 1. Clone ou descompacte o projeto
cd EtlProject

# 2. Coloque os arquivos order.csv e order_item.csv na pasta do projeto

# 3. Restaure dependências e execute
dotnet run
```

O arquivo `output.csv` será gerado na mesma pasta.

### Alterar filtros

Em `Program.fs`, ajuste as variáveis:

```fsharp
let status = "Complete"   // "Complete", "Pending" ou "Cancelled"
let origin = "O"          // "O" (online) ou "P" (physical)
```

### Usar leitura via HTTP (opcional)

Em `Program.fs`, substitua as chamadas de leitura:

```fsharp
let orders     = readOrdersFromUrl "https://seu-servidor/order.csv"
let orderItems = readOrderItemsFromUrl "https://seu-servidor/order_item.csv"
```

---

## Etapas do Pipeline

### 1. Extract (impuro)
`Extract.fs` lê o conteúdo dos arquivos (ou URLs HTTP) e usa as **helper functions** `parseOrder` e `parseOrderItem` (definidas em `Transform.fs`) para converter cada linha CSV em um Record F#.

### 2. Transform (puro)
`Transform.fs` contém apenas funções puras:

| Função | Operação FP | Descrição |
|---|---|---|
| `filterOrders` | `List.filter` | Filtra por status e origem |
| `innerJoin` | `List.collect` + `List.filter` + `List.map` | Junta Orders e OrderItems |
| `buildSummaries` | `List.groupBy` + `List.map` | Agrupa itens por pedido |
| `summarizeGroup` | `List.fold` (×2) | Soma receita e impostos |
| `formatSummaryLine` | `map` (implícito) | Formata linha CSV |

**Cálculos:**
- Receita do item = `price × quantity`  
- Imposto do item = `tax × receita`  
- `total_amount` = Σ receitas de todos os itens do pedido  
- `total_taxes` = Σ impostos de todos os itens do pedido  

### 3. Load (impuro)
`Load.fs` recebe a lista de `OrderSummary` e escreve o CSV final com cabeçalho `order_id,total_amount,total_taxes`.

---

## Separação Puro / Impuro

| Arquivo | Tipo | Motivo |
|---|---|---|
| `Types.fs` | neutro | apenas definições de tipos |
| `Transform.fs` | **puro** | sem I/O; mesmo input → mesmo output |
| `Extract.fs` | **impuro** | lê sistema de arquivos e rede |
| `Load.fs` | **impuro** | escreve no sistema de arquivos |
| `Program.fs` | **impuro** | rotina principal, coordena tudo |

---

## Requisitos Atendidos

### Obrigatórios
- [x] Projeto em F#
- [x] Uso de `map`, `fold` e `filter` nas transformações
- [x] Funções de leitura e escrita de CSV
- [x] Separação de funções puras e impuras em arquivos distintos
- [x] Entrada carregada em lista de Records
- [x] Helper Functions para carregar campos nos Records (`parseOrder`, `parseOrderItem`)
- [x] Relatório do projeto (este README)

### Opcionais Implementados
- [x] **Inner join em F#** — `Transform.innerJoin` junta as duas tabelas antes do Transform
- [x] **Projeto .NET organizado** — estrutura multi-arquivo com `.fsproj` e ordem de compilação explícita
- [x] **Saída adicional por mês/ano** — `Transform.buildMonthlyAverages` agrupa os summaries por mês e ano e calcula a média de receita e impostos, gerando `output_monthly.csv`
