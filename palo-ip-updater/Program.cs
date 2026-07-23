using palo_ip_updater;
using palo_ip_updater.Models;
using System.Runtime;

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.Configure<PaloConfigurationSettings>(context.Configuration.GetSection("PaloLoginDetails"));
        services.AddHostedService<Worker>();
    });

var host = builder.Build();
host.Run();
