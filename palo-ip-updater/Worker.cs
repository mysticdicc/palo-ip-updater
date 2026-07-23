using Microsoft.Extensions.Options;
using palo_ip_updater.Models;
using Renci.SshNet;
using System.Text;

namespace palo_ip_updater
{
    internal class Worker(IOptionsMonitor<PaloConfigurationSettings> configuration, ILogger<Worker> logger) : BackgroundService
    {
        private IOptionsMonitor<PaloConfigurationSettings> _config = configuration;
        private ILogger<Worker> _logger = logger;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    if (ConfigIsValid()) await UpdateIpAddressAsync(stoppingToken);
                    else _logger.LogError("One or more items in the configuration file are invalid.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.Message);
                }

                _logger.LogInformation($"Service action finished entering sleep for {_config.CurrentValue.UpdateInterval} seconds.");
                await Task.Delay((_config.CurrentValue.UpdateInterval * 1000), stoppingToken);
            }
        }

        private async Task RunCommandsWithShellAsync(SshClient ssh, IEnumerable<string> commands, CancellationToken token)
        {
            using var shell = ssh.CreateShellStream("xterm", 80, 24, 800, 600, 1024);
            var sb = new StringBuilder();

            async Task<string> ReadAvailableAsync()
            {
                var outSb = new StringBuilder();
                for (int i = 0; i < 10 && !token.IsCancellationRequested; i++)
                {
                    while (shell.DataAvailable)
                    {
                        var buffer = new byte[4096];
                        var read = shell.Read(buffer, 0, buffer.Length);
                        if (read > 0) outSb.Append(Encoding.UTF8.GetString(buffer, 0, read));
                    }
                    await Task.Delay(150, token);
                }
                return outSb.ToString();
            }

            sb.Append(await ReadAvailableAsync());

            foreach (var cmd in commands)
            {
                _logger.LogInformation("SSH -> {Cmd}", cmd);
                shell.WriteLine(cmd);
                await Task.Delay(200, token);
                var response = await ReadAvailableAsync();
                _logger.LogInformation("SSH Resp: {Resp}", response.Trim());
                sb.Append(response);
            }
        }

        private async Task UpdateIpAddressAsync(CancellationToken token = default)
        {
            using var client = new HttpClient();

            _logger.LogInformation("Fetching IP Address.");
            var ip = await client.GetStringAsync("https://api.ipify.org/");
            if (string.IsNullOrEmpty(ip)) return;
            _logger.LogInformation($"Current IP: {ip}.");

            _logger.LogInformation($"Starting ssh client with {_config.CurrentValue.HostIp}");
            using var ssh = new SshClient(_config.CurrentValue.HostIp, _config.CurrentValue.Username, _config.CurrentValue.Password);
            await ssh.ConnectAsync(token);

            if (!ssh.IsConnected) throw new InvalidOperationException("SSH connect failed");

            var commands = new[]
            {
                "configure",
                $"set address {_config.CurrentValue.HostIpName} ip-netmask {ip}",
                "commit",
                "exit"
            };

            await RunCommandsWithShellAsync(ssh, commands, token);
            ssh.Disconnect();
        }

        private bool ConfigIsValid()
        {
            if (string.IsNullOrWhiteSpace(_config.CurrentValue.Username)) return false;
            if (string.IsNullOrWhiteSpace(_config.CurrentValue.Password)) return false;
            if (string.IsNullOrWhiteSpace(_config.CurrentValue.HostIp)) return false;
            if (string.IsNullOrWhiteSpace(_config.CurrentValue.HostIpName)) return false;
            return true;
        }
    }
}
