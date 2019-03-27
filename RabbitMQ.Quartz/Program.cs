using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Quartz.Impl;
using Quartz.Spi;
using RabbitMQ.Quartz.Factory;
using RabbitMQ.Quartz.Options;
using RabbitMQ.Quartz.Services;

namespace RabbitMQ.Quartz
{
    class Program
    {
        static void Main(string[] args)
        {
            var host = new HostBuilder()
                       .ConfigureHostConfiguration(configHost =>
                       {
                           configHost.SetBasePath(Directory.GetCurrentDirectory());
                           configHost.AddEnvironmentVariables("ASPNETCORE_");
                       })
                       .ConfigureAppConfiguration((hostContext, configApp) =>
                       {
                           configApp.AddJsonFile("appsettings.json", true);
                           configApp.AddJsonFile($"appsettings.{hostContext.HostingEnvironment.EnvironmentName}.json", true);
                           configApp.AddEnvironmentVariables();
                       })
                       .ConfigureServices((hostContext, services) =>
                       {
                           //配置服务及依赖注入注册，注：没有Middleware的配置了。
                           services.AddLogging();
                           services.AddSingleton<IJobFactory, JobFactory>();
                           services.AddSingleton(provider =>
                           {
                               var option = new QuartzOption(hostContext.Configuration);
                               var sf = new StdSchedulerFactory(option.ToProperties());
                               var scheduler = sf.GetScheduler().Result;
                               scheduler.JobFactory = provider.GetService<IJobFactory>();
                               return scheduler;
                           });
                           services.AddHostedService<QuartzService>();

                           services.AddSingleton<RabbitMqJob, RabbitMqJob>();
                           services.AddSingleton<WeatherJob, WeatherJob>();
                       })
                       .ConfigureLogging((hostContext, configLogging) =>
                       {
                           configLogging.AddConsole();
                           configLogging.AddDebug();
                       })
                       .UseConsoleLifetime()
                       .Build();

            host.Run();
        }
    }
}
