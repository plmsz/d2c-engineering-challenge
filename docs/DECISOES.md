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