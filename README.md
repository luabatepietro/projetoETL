# ETL Project — Programação Funcional (Aula 6)

**Disciplina:** Programação Funcional — Insper 2025-1  
**Projeto:** ETL em F# com dados de pedidos  

---

## Descrição do Projeto

Pipeline ETL que lê dois arquivos CSV (`order.csv` e `order_item.csv`), aplica filtros e transformações funcionais usando o paradigma de **Programação Funcional em F#**, e gera dois arquivos CSV de saída:

- `output.csv` — total de receita e impostos por pedido
- `output_monthly.csv` — média de receita e impostos agrupados por mês e ano

---

## Estrutura do Projeto

```
projetoETL/
├── EtlProject.sln              
├── EtlProject/                
│   ├── .devcontainer/
│   │   └── devcontainer.json  
│   ├── EtlProject.fsproj      
│   ├── Types.fs               
│   ├── Transform.fs            
│   ├── Extract.fs              
│   ├── Load.fs                 
│   ├── Program.fs              
│   ├── order.csv               
│   └── order_item.csv          
└── EtlProject.Tests/           
    ├── EtlProject.Tests.fsproj
    ├── TransformTests.fs        
    └── Program.fs              
```

---

## Como Reproduzir

### Pré-requisitos
- [.NET SDK 9.0+](https://dotnet.microsoft.com/download)
- Ou abrir direto no **GitHub Codespaces** (ambiente já configurado via devcontainer)

### Rodar o projeto principal

```bash
cd EtlProject
dotnet run
```

Serão gerados `output.csv` e `output_monthly.csv` na pasta do projeto.

### Rodar os testes

```bash
cd EtlProject.Tests
dotnet run
```

Saída esperada:
```
[INF] EXPECTO! 23 tests run – 23 passed, 0 ignored, 0 failed, 0 errored. Success!
```

### Alterar filtros

Em `Program.fs`, ajuste as variáveis:

```fsharp
let status = "Complete"   // "Complete", "Pending" ou "Cancelled"
let origin = "O"          // "O" (online) ou "P" (physical)
```

---

## Etapas do Pipeline

### 1. Extract (impuro)
`Extract.fs` lê o conteúdo dos arquivos CSV e usa as **helper functions** `parseOrder` e `parseOrderItem` (definidas em `Transform.fs`) para converter cada linha em um Record F#.

### 2. Transform (puro)
`Transform.fs` contém apenas funções puras — sem I/O, sem efeitos colaterais:

| Função | Operação FP | Descrição |
|---|---|---|
| `parseOrder` | helper | Converte linha CSV em Record Order |
| `parseOrderItem` | helper | Converte linha CSV em Record OrderItem |
| `filterOrders` | `List.filter` | Filtra por status e origem |
| `innerJoin` | `List.collect` + `List.filter` + `List.map` | Junta Orders e OrderItems |
| `buildSummaries` | `List.groupBy` + `List.map` | Agrupa itens por pedido |
| `summarizeGroup` | `List.fold` (×2) | Soma receita e impostos |
| `buildMonthlyAverages` | `List.groupBy` + `List.fold` | Média por mês/ano |
| `formatSummaryLine` | pura | Formata linha CSV de saída principal |
| `formatMonthlyLine` | pura | Formata linha CSV de saída mensal |

**Cálculos:**
- Receita do item = `price × quantity`
- Imposto do item = `tax × receita`
- `total_amount` = Σ receitas de todos os itens do pedido
- `total_taxes` = Σ impostos de todos os itens do pedido
- `avg_amount` / `avg_taxes` = média dos totais agrupados por mês e ano

### 3. Load (impuro)
`Load.fs` recebe as listas de `OrderSummary` e `MonthlyAverage` e escreve os dois CSVs de saída.

---

## Separação Puro / Impuro

| Arquivo | Tipo | Motivo |
|---|---|---|
| `Types.fs` | neutro | apenas definições de tipos/records |
| `Transform.fs` | **puro** | sem I/O; mesmo input → mesmo output sempre |
| `Extract.fs` | **impuro** | lê sistema de arquivos |
| `Load.fs` | **impuro** | escreve no sistema de arquivos |
| `Program.fs` | **impuro** | rotina principal, coordena o pipeline |

---

## Requisitos Opcionais
- [x] **Projeto .NET organizado** — estrutura multi-arquivo com `.fsproj`, `.sln` e ordem de compilação explícita
- [x] **Inner join em F#** — `Transform.innerJoin` junta as duas tabelas antes da etapa de Transform
- [x] **Saída adicional por mês/ano** — `Transform.buildMonthlyAverages` gera `output_monthly.csv` com médias agrupadas
- [x] **Docstrings em todas as funções** — todas as funções de `Transform.fs` documentadas no formato XML docstring
- [x] **Testes completos para funções puras** — 23 testes cobrindo todas as funções puras com a biblioteca Expecto

## Uso de IA Generativa

Em algumas etapas do desenvolvimento, esse projeto contou com o auxilio de IA's generativas (Claude) para revisar código, criação do ReadMe, além de uma grande ajuda na cobertura de testes.
---
