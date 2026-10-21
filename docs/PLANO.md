### PLANO — Account Service (Verificação de Biometria)

**Fase 0 — Entendimento (Fundações e DDD)**

* **0.1:** Mapear a responsabilidade de cada camada descrita no README (`Account.Api`, `Account.Domain`, `Account.Data`, `Account.Tasks`). Entregável: um breve `docs/ARQUITETURA.md`.
* **0.2:** Analisar o `CommandHandlerBase`. Ele é o seu *Template Method*. Garanta que ele continue controlando o Unit of Work e o Commit, mantendo as regras de transição de estado isoladas no `BiometricVerificationAggregate` para não vazar lógica de domínio para a aplicação.
* **0.3:** Testar o provedor falso via curl (na porta `5080`) usando as palavras-chave da tabela (`approved`, `rejected`, `pending`) na propriedade `reference` para mapear exatamente o payload de entrada e saída.
0.2 Executar o projeto e testar os três cenários via curl (approved, rejected, pending). Anotar as respostas reais, incluindo erros e o que acontece com uma referência nunca processada.
 ```json
 {"id":"32e76626-5227-4f74-a37f-07a403ac32e7","reference":"verification-approved-1","status":"Pending","resultDescription":null,"createDate":"2026-10-21T18:56:49.1055111Z","updatedAt":null}
 ```
 ```json
 {"id":"d4d5a1b8-99b4-41a4-98ee-f764ee4ad454","reference":"verification-rejected-2","status":"Pending","resultDescription":null,"createDate":"2026-10-21T19:01:36.0087484Z","updatedAt":null}
 ```
 ```json
 {"id":"d74bcc99-f743-4d25-866e-305dc67d4da2","reference":"verification-pending-3","status":"Pending","resultDescription":null,"createDate":"2026-10-21T19:02:44.9518835Z","updatedAt":null}
 ```

**Fase 1 — Etapa 1: Processamento Síncrono**

* **1.1:** Criar a camada de *Anti-Corruption* no `Account.Data` (provavelmente dentro de `FakeBiometricProvider`). É aqui que a string do provedor externo vira o seu `BiometricVerificationStatus` interno.
* **1.2:** Implementar o fluxo do `ProcessBiometricVerificationCommand`. O handler chama o provedor, o Aggregate processa a transição de estado, e o `IRepository` persiste os dados em memória.
* **1.3:** Ajustar o endpoint `GET` para consumir o `IBiometricVerificationQuery`, buscando o estado persistido diretamente da memória, e não indo ao provedor novamente.
* **1.4:** Escrever os testes usando o padrão AAA (Arrange, Act, Assert). Como você prefere testar a interação, foque em enviar o comando e verificar o estado final no repositório, em vez de mockar os detalhes de implementação internos do handler.

**Fase 2 — Etapa 2: Processamento Assíncrono (O Coração do Desafio)**

* **2.1:** **Decisão Arquitetural:** O README já define o caminho com a pasta `Account.Tasks/ # Jobs agendados`. 
* **2.2:** Alterar o `ProcessBiometricVerificationCommand` para **não travar o request**. Se a primeira consulta retornar `PENDING` (ou cair no fallback de texto qualquer), persista como `PENDING` e devolva a resposta HTTP imediatamente para o cliente.
* **2.3:** No `SyncBiometricVerificationsJob`, implemente a lógica que busca no repositório todas as verificações paradas em `PENDING`, chama o provedor para cada uma, e atualiza o estado via Aggregate se houver mudança.
* **2.4:** **O truque do Mock para Testes:** Como o `FakeBiometricProvider` responde baseado apenas no texto da referência, para testar a transição de um Job (de PENDING para APPROVED), você precisará implementar um estado interno (um dicionário ou contador) no *FakeProvider* para que ele responda PENDING na primeira chamada e APPROVED na segunda, permitindo que o teste do Job valide a mudança de estado com sucesso.

**Fase 3 — Refinamento, Guard Clauses e Resiliência**

* **3.1:** Aplicar *Guard Clauses* no Aggregate e no Command para evitar referências nulas, vazias ou mal formatadas, retornando um HTTP 400 limpo.
* **3.2:** Idempotência e Concorrência: O que acontece se o usuário fizer um POST da mesma referência duas vezes seguidas enquanto ela está `PENDING`? Trate essa concorrência no nível do Repositório ou da Controller para evitar dupla inserção/processamento.
* **3.3:** Tratamento de falhas: Garanta que, se o `IBiometricProvider` lançar uma exceção de rede durante o Job assíncrono, isso seja logado e não derrube a thread inteira do BackgroundService.

**Fase 4 — Preparação para a Entrevista (Apresentação)**

* **4.1:** Documentar o `README.md` da sua entrega. Deixe claro o fluxo: Controller -> Mediator -> Command -> Provider -> Aggregate -> Repo.
* **4.2:** Escrever as limitações conhecidas: Dados voláteis (em memória), ausência de mensageria real (RabbitMQ/Azure Service Bus), e como isso seria resolvido em um ambiente de produção.
* **4.3:** Preparar o roteiro de explicação em formato **STAR** (Situação, Tarefa, Ação, Resultado). Estruture a explicação da Etapa 2 (Jobs Assíncronos) como um *case* clássico de design de sistemas que resolve problemas de timeout e gargalos em APIs de terceiros.