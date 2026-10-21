# Decisão 01:

Contexto: o provedor retorna só PENDING, APPROVED ou REJECTED, e o enunciado não diz o que fazer com outros valores.
Decisão: o mapper lança BusinessException para valor desconhecido, nulo ou vazio, com a referência e o valor recebido na mensagem. Não há default silencioso, nem conversão para PENDING.
Alternativa descartada: converter desconhecido em PENDING, porque esconderia a quebra de contrato e geraria polling infinito na Fase 2.
Trade-off em aberto: se BusinessException virar HTTP 400, o cliente receberá erro de requisição por causa de um problema do provedor. Isso precisa ser confirmado no código antes de fechar a decisão.