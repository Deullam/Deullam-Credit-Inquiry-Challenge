using NUnit.Framework;

// A suíte inteira roda em série. Isso é premissa do KafkaTopicProbe (CI-02): a prova de "nenhuma
// mensagem nova no tópico" só vale se nenhum outro teste desta execução estiver publicando ao
// mesmo tempo. Não remova sem revisar o desenho do probe.
[assembly: Parallelizable(ParallelScope.None)]
[assembly: LevelOfParallelism(1)]

// Toda a suíte leva a categoria "E2E": é o que o `make test` usa para excluí-la (--filter
// "TestCategory!=E2E"), já que ela exige a pilha do docker-compose no ar e não um dublê.
[assembly: Category("E2E")]
