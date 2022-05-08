using ConsumerAPI.Services;

namespace ConsumerAPI.BackgroundServices
{
    public class UsuariosProcessarBackgroundService : BackgroundService
    {
        private readonly ILogger<UsuariosProcessarBackgroundService> _Logger;
        private readonly IServiceProvider _ServiceProvider;

        public UsuariosProcessarBackgroundService(ILogger<UsuariosProcessarBackgroundService> logger, IServiceProvider serviceProvider)
        {
            _Logger = logger;
            _ServiceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using (var scope = _ServiceProvider.CreateScope())
                {
                    var usuarioService = scope.ServiceProvider.GetRequiredService<UsuarioService>();
                    _Logger.LogInformation(await usuarioService.ProcessarUsuarioNovo(stoppingToken));
                }
                await Task.Delay(5000, stoppingToken);
            }
        }
    }
}
