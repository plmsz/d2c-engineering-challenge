# Arquitetura do Projeto Account

Este documento descreve a separação de responsabilidades (Clean Architecture / DDD) e o fluxo de execução das requisições.

## Mapeamento de Camadas e Responsabilidades

* **Account.Api**
  * **Responsabilidade:** Ponto de entrada da aplicação. Lida com o roteamento HTTP, validação inicial de requests (via anotações), injeção de dependências e formatação das respostas da API.
  * **Arquivos Principais:** `Startup.cs` (configuração de injeção de dependência e middlewares) e `AccountController.cs` (receção das chamadas REST).
* **Account.Domain**
  * **Responsabilidade:** Coração da aplicação. Contém as regras de negócio, interfaces de contratos externos (repositórios e provedores) e a implementação do padrão CQRS (Commands e Queries). Não possui dependências de infraestrutura.
  * **Arquivos Principais:** `BiometricVerificationAggregate.cs` (garante a consistência dos dados de biometria), `ProcessBiometricVerificationCommand.cs` (intenção de mudança de estado), `CommandHandlerBase.cs` (orquestração base) e interfaces como `IBiometricProvider.cs` e `IRepository.cs`.
* **Account.Data**
  * **Responsabilidade:** Camada de infraestrutura e persistência. Implementa as interfaces definidas no Domain. Lida com acesso a banco de dados (Stores/Repositories) e comunicação com APIs externas.
  * **Arquivos Principais:** `Repository.cs` (acesso a dados genérico), `FakeBiometricProvider.cs` (simulação da API externa de biometria) e `InMemoryUnitOfWork.cs` (gerenciamento de transações na memória).
* **Account.Tasks**
  * **Responsabilidade:** Processamento em background e rotinas agendadas (Jobs/Workers). Serve para rotinas assíncronas que não devem bloquear o fluxo principal da API.
  * **Arquivos Principais:** `SyncBiometricVerificationsJob.cs` (sincroniza o status das biometrias pendentes com o provedor externo de forma assíncrona).

## Fluxo de Requisição (Controller até Repository)

Abaixo encontra-se o diagrama de sequência ilustrando o caminho de uma requisição típica de escrita (Command), utilizando o padrão CQRS e Mediator.

```mermaid
sequenceDiagram
    participant C as AccountController
    participant M as Mediator
    participant CH as CommandHandler
    participant A as Aggregate
    participant R as Repository
    participant U as UnitOfWork

    C->>M: Envia Command (ex: ProcessBiometric...)
    M->>CH: Roteia para o Handler específico
    CH->>R: Obtém Entidade/Aggregate (se existir)
    R-->>CH: Retorna Aggregate
    CH->>A: Executa método de domínio (regras de negócio)
    A-->>CH: Estado atualizado
    CH->>R: Save/Update(Aggregate)
    CH->>U: Commit()
    U-->>CH: Sucesso
    CH-->>M: CommandHandlerResponse
    M-->>C: Retorna HTTP 200/201/400