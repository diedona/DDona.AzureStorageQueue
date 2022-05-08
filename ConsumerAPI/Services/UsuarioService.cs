using Azure.Storage.Queues;
using ConsumerAPI.Models;
using System.Text.Json;

namespace ConsumerAPI.Services
{
    public class UsuarioService
    {
        private readonly IConfiguration _Configuration;
        private readonly ILogger<UsuarioService> _Logger;
        private readonly string connectionStringQueue;

        public UsuarioService(IConfiguration configuration, ILogger<UsuarioService> logger)
        {
            _Configuration = configuration;
            _Logger = logger;
            connectionStringQueue = _Configuration["AzuriteStorageEmulator:queue"];
        }

        public async Task<string> ProcessarUsuarioNovo(CancellationToken stoppingToken)
        {
            QueueClient queueUsuarioProcessarClient = new QueueClient(connectionStringQueue, "usuarios-processar");
            await queueUsuarioProcessarClient.CreateIfNotExistsAsync();

            var messages = await queueUsuarioProcessarClient.ReceiveMessagesAsync(maxMessages: 1, cancellationToken: stoppingToken);
            if (messages == null || !(messages.Value?.Any() ?? false))
                return "Nothing to process right now";

            var message = messages.Value[0];
            _Logger.LogInformation($"Processing {message.MessageText}");

            UsuarioNovo usuarioNovoProcessar;

            try
            {
                usuarioNovoProcessar = JsonSerializer.Deserialize<UsuarioNovo>(message.MessageText);
                await ProcessarUsuarioNovoValido(usuarioNovoProcessar);
            }
            catch (Exception ex) when (ex is JsonException || ex is NotSupportedException)
            {
                await ProcessarUsuarioNovoInvalido(message.MessageText);
            }

            await queueUsuarioProcessarClient.DeleteMessageAsync(message.MessageId, message.PopReceipt, stoppingToken);
            return $"Ok done at {DateTime.Now}";
        }

        private async Task ProcessarUsuarioNovoValido(UsuarioNovo usuarioNovoProcessar)
        {
            QueueClient queueUsuarioProcessadoClient = new QueueClient(connectionStringQueue, "usuarios-processados");
            await queueUsuarioProcessadoClient.CreateIfNotExistsAsync();
            var usuarioNovoProcessado = new UsuarioProcessado()
            {
                Assinatura = usuarioNovoProcessar.Assinatura,
                DataNascimento = usuarioNovoProcessar.DataNascimento,
                Nome = usuarioNovoProcessar.Nome,
                Id = Guid.NewGuid().ToString(),
            };

            string usuarioNovoProcessadoString = JsonSerializer.Serialize(usuarioNovoProcessado);
            await queueUsuarioProcessadoClient.SendMessageAsync(usuarioNovoProcessadoString);
        }

        private async Task ProcessarUsuarioNovoInvalido(string mensagemUsuarioInvalido)
        {
            QueueClient queueUsuarioProcessadoPoisonClient = new QueueClient(connectionStringQueue, "usuarios-processados-poison");
            await queueUsuarioProcessadoPoisonClient.CreateIfNotExistsAsync();
            await queueUsuarioProcessadoPoisonClient.SendMessageAsync(mensagemUsuarioInvalido);
        }
    }
}
