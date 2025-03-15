using LimOrleansServer.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Orleans.Configuration;

var host = new HostBuilder()
    .UseOrleans(siloBuilder =>
    {
        siloBuilder
            .AddStartupTask<ActivateLimBridge>()
            .AddMemoryStreams("Default")
            .UseLocalhostClustering()
            .UseInMemoryReminderService()
            .Configure<ClusterOptions>(options =>
            {
                options.ClusterId = "dev";
                options.ServiceId = "LimOrleansServerOnlySimulator";
            })
            .AddMemoryGrainStorage("Default")
            .AddStreaming()
            .UseDashboard(options =>
            {
                options.HostSelf = true;
                options.Port = 5004;
            });
    })
    .Build();

await host.StartAsync();

Console.WriteLine("Press Enter to stop the simulator...");
Console.ReadLine();

await host.StopAsync();
