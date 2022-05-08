using Azure.Storage.Queues;
using ProducerAPI.Models;
using System.Text.Json;

namespace ProducerAPI.Services
{
    public class UsuarioService
    {
        private readonly IConfiguration _Configuration;

        public UsuarioService(IConfiguration configuration)
        {
            _Configuration = configuration;
        }

        public async Task CadastrarUsuarioNovo(UsuarioNovo usuario)
        {
            string connectionString = _Configuration["AzuriteEmulatorLocal:queue"];
            string queueName = "usuarios-processar";
            string usuarioJson = JsonSerializer.Serialize(usuario);

            QueueClient queueClient = new QueueClient(connectionString, queueName);
            await queueClient.CreateIfNotExistsAsync();
            await queueClient.SendMessageAsync(usuarioJson);
        }
    }
}
