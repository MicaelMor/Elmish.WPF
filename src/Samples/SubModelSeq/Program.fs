module Elmish.WPF.Samples.SubModelSeq.Program

open System
open Elmish
open Elmish.WPF

module App =

  type TransacaoCartao = {
      Id: int option;
      Posto: string;
      TipoRede: string;
      Data: DateTime;
      NumeroCartao: string;
      MatriculaCartao: string;
      LitrosArredondado: float;
      TipoCombustivel: string;
      NumeroRecibo: string;
      Kms: int;
      IdCond: int16;
      NumeroFatura: string;
      ValorUnitFinal: decimal;
      ValorUnitDesc: decimal;
      ValorUnitRef: decimal;
      TotalLiq: decimal;
      TotalIva: decimal;
      Total: decimal;
      Empresa: string; }

  let getList =
    let sequence = seq {0..5000}
    let getNewRecord id =
      {Id = Some id;
      Posto = "Test";
      TipoRede = "Test";
      Data = DateTime.Now;
      NumeroCartao = "Test";
      MatriculaCartao = "Test";
      LitrosArredondado = 55.5;
      TipoCombustivel = "Test";
      NumeroRecibo = "Test";
      Kms = 55;
      IdCond = (int16)5;
      NumeroFatura = "Test";
      ValorUnitFinal = 55m;
      ValorUnitDesc = 55m;
      ValorUnitRef = 55m;
      TotalLiq = 55m;
      TotalIva = 55m;
      Total = 55m;
      Empresa = "Test"}
    sequence |> Seq.map (fun x -> getNewRecord x) |> Seq.toList


  type LoginWindow = {
      Utilizador: string
      Password: string
  }

  type Model = {
      Transactions: TransacaoCartao list
      IsGettingTransactions: bool
      }

  type Msg =
      | GetAll
      | GetTransactions
      | GotTransactions of TransacaoCartao list
      | TransactionsRetrievalFailure of exn

  let initModel = {
      Transactions = List.empty
      IsGettingTransactions = false }

  let init() = 
      initModel, Cmd.none

  let getTransactions () =
      async {
          do! Async.SwitchToThreadPool()
          let transactions = getList
          Threading.Thread.Sleep(1000)
          return GotTransactions transactions
      }

  let showErrorMessage errorTitle errorMessage =
      Windows.MessageBox.Show(errorMessage, errorTitle, Windows.MessageBoxButton.OK, Windows.MessageBoxImage.Error)

  let update msg m =
      match msg with
      | GetAll -> m, Cmd.batch [Cmd.ofMsg (GetTransactions)]
      | GetTransactions -> {m with IsGettingTransactions = true}, Cmd.OfAsync.either getTransactions () id TransactionsRetrievalFailure
      | GotTransactions x -> {m with Transactions = x; IsGettingTransactions = false}, Cmd.none
      | TransactionsRetrievalFailure ex ->
          showErrorMessage
              "Erro na Aplicação"
              ("Erro ao obter transações, causa:" +
              Environment.NewLine +
              Environment.NewLine +
              ex.Message +
              Environment.NewLine +
              Environment.NewLine +
              "Tente novamente, se erro persistir contactar programador") |> ignore
          {m with Transactions = List.empty; IsGettingTransactions = false}, Cmd.none


module Bindings =

  open App

  let bindings () : Binding<Model, Msg> list =
      [
          "GetAll" |> Binding.cmdIf((fun _ -> GetAll), (fun m -> not (m.IsGettingTransactions)))
          "IsGettingTransactions" |> Binding.oneWay (fun m -> m.IsGettingTransactions)
          "Transactions" |> Binding.subModelSeq (
              (fun m -> m.Transactions),
              (fun e -> e.Id),
              (fun () -> [
                  "Posto" |> Binding.oneWay (fun (_, e) -> e.Posto)
                  "Data" |> Binding.oneWay (fun (_, e) -> e.Data)
                  "Matricula" |> Binding.oneWay (fun (_, e) -> e.MatriculaCartao)
                  "Litros" |> Binding.oneWay (fun (_, e) -> e.LitrosArredondado)
                  "TipoCombustivel" |> Binding.oneWay (fun (_, e) -> e.TipoCombustivel)
                  "Kms" |> Binding.oneWay (fun (_, e) -> e.Kms)
                  "NumeroFatura" |> Binding.oneWay (fun (_, e) -> e.NumeroFatura)
                  "NumeroRecibo" |> Binding.oneWay (fun (_, e) -> e.NumeroRecibo)
                  "ValorUnitFinal" |> Binding.oneWay (fun (_, e) -> e.ValorUnitFinal)
                  "Total" |> Binding.oneWay (fun (_, e) -> e.Total)
              ]))
      ]

let main mainWindow =
    Program.mkProgramWpf App.init App.update Bindings.bindings
    |> Program.withConsoleTrace
    |> Program.runWindowWithConfig
        {ElmConfig.Default with LogConsole = true; Measure = true}
        mainWindow
