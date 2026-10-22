# 01:

Contexto: o provedor retorna só PENDING, APPROVED ou REJECTED, e o enunciado não diz o que fazer com outros valores.
Decisão: o mapper lança BusinessException para valor desconhecido, nulo ou vazio, com a referência e o valor recebido na mensagem. Não há default silencioso, nem conversão para PENDING.
Alternativa descartada: converter desconhecido em PENDING, porque esconderia a quebra de contrato e geraria polling infinito na Fase 2.
Trade-off em aberto: se BusinessException virar HTTP 400, o cliente receberá erro de requisição por causa de um problema do provedor. Isso precisa ser confirmado no código antes de fechar a decisão.
## 02 — Leitura do estado no GET

Contexto: o GET precisa retornar o estado da verificação.

Decisão: o GET lê o estado persistido via IBiometricVerificationQuery e não consulta o provedor.

Alternativas descartadas: consultar o provedor a cada GET (duplica chamadas externas e pode divergir do estado salvo).

Trade-off: o GET pode mostrar PENDING até o job da Fase 2 atualizar.

## 03 — Referência inexistente no GET

Contexto: o enunciado não define o retorno para referência não encontrada.

Decisão: retorna 404.

Alternativas descartadas: 200 com corpo vazio.

Trade-off: se o avaliador esperar outro formato, precisa de ajuste.

## 04 — Local do job de reconsulta

Contexto: o job precisa reconsultar verificações PENDING. O README define Account.Tasks para jobs agendados.
Decisão: SyncBiometricVerificationsJob fica em Account.Tasks e depende só de Account.Domain. As implementações concretas são registradas pelo Account.Api.
Alternativas descartadas: BackgroundService dentro de Account.Tasks (exigiria pacote de hosting fora do Web SDK).
Trade-off: o registro de DI fica concentrado no Account.Api.

## 05 — Execução do job por hosted service com cron e escopo por ciclo

Contexto: o job define o cron "0 */15 * * * *" (seis campos, com segundos) e usa dependências Scoped. Registrá-lo como Singleton causaria captive dependency.
Decisão: ScheduledJobHostedService (BackgroundService, Account.Api) lê Schedule do job resolvido em um escopo na inicialização, interpreta com Cronos (CronFormat.IncludeSeconds) e, a cada ciclo, espera pelo relógio real, avança um cursor, cria um escopo novo, executa DoWorkAsync e loga erros sem derrubar o host. O cron fica só no job.
Alternativas descartadas: string do cron duplicada no Startup (duas fontes de verdade); Configure posterior (estado mutável); reflexão (quebra tipagem); intervalo fixo de 15 min (ignora o cron definido); System.Threading.Timer (permite sobreposição e exceções em callback async); job Singleton com IServiceProvider (esconde o escopo).
Trade-off: se o job demorar mais que o intervalo, os horários atrasados rodam em sequência, sem espera. O cron só muda com reinício da aplicação. O pacote Cronos entra no Account.Api.

## 06 — Listagem de verificações pendentes para o job
- Contexto: o SyncBiometricVerificationsJob precisa buscar todas as verificações com status Pending para reconsultar o provedor.
- Decisão: adicionar um método de leitura em IBiometricVerificationQuery (ex.: GetPendingAsync) e implementá-lo em Account.Data, lendo do BiometricVerificationStore. A escrita continua passando por IRepository.
- Alternativas descartadas: o job ler o store diretamente (quebra a separação de Account.Data e Account.Domain); o job manter a própria lista de referências (duplica estado).
- Trade-off: uma mudança na interface de domínio, que exige atualizar as implementações existentes.

## 07 — Criação atômica da verificação por referência

- Contexto: o handler do POST lê a verificação pela referência, chama o provedor e depois cria e grava um registro novo. Essas etapas não são atômicas, então dois POSTs simultâneos com a mesma referência criam registros com `id`s diferentes (observado: 10 POSTs geraram 4 e 8 ids distintos).
- Decisão: criar a interface `IBiometricVerificationRepository` em `Account.Domain/Repositories`, com `GetOrCreateByReference(reference, create)`, implementada em `Account.Data` por delegação ao `BiometricVerificationStore`. O store executa leitura, criação e escrita sob um único lock. O handler usa essa interface e só chama `ApplyResult` e `UpdateAsync` quando o status muda ou a verificação acabou de ser criada. `IRepository` e `Account.Domain/Framework` não mudam.
- Alternativas descartadas: `ConcurrentDictionary.GetOrAdd` com factory (a factory pode executar mais de uma vez e gerar entidades descartadas); lock estático no handler (estado global no domínio); alterar `IRepository` (viola a regra de não mexer em `Framework`).
- Trade-off: uma interface a mais no domínio e um lock global no store. Aceitável para store em memória. Dois POSTs simultâneos recebem a mesma instância; como escrevem o mesmo status, o resultado é consistente, mas não há lock por entidade.# 08 — Idempotência e concorrência com GetOrCreateByReference

Contexto: o processo de verificação de biometria pode receber a mesma referência múltiplas vezes enquanto o status está PENDING. É necessário evitar dupla inserção/processamento.

Decisão: Implementar o padrão GetOrCreateByReference no repositório para garantir que, para uma dada referência, apenas uma instância de BiometricVerification seja criada e gerenciada. O método atomicamente verifica a existência e, se não existir, cria usando o delegado fornecido. No handler do comando, a chamada a GetOrCreateByReference substitui o padrão GetByReference + null check + nova instância, seguido de UpdateAsync. Após mapear o status do provedor, aplica-se o resultado e persiste apenas se current.Status != domainStatus, garantindo idempotência natural mesmo com múltiplas requisições concorrentes.

Alternativas descartadas: manter o padrão original GetByReference + null check (risco de dupla inserção); usar bloqueio no nível da controller (quebra da separação de responsabilidades); usar lock no aggregate (dificulta testing e viola a encapsulacão).
