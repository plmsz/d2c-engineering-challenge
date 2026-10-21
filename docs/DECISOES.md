# Decisão 01:

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
