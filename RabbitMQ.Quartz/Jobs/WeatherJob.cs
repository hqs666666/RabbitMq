using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Quartz;
using RabbitMQ.Client;

namespace RabbitMQ.Quartz
{
    public class WeatherJob : IJob
    {
        private readonly ILogger _logger;

        public WeatherJob(ILogger<RabbitMqJob> logger)
        {
            _logger = logger;
        }

        public Task Execute(IJobExecutionContext context)
        {
            var factory = new ConnectionFactory
            {
                UserName = "guest",//用户名
                Password = "guest",//密码
                HostName = "localhost"//rabbitmq ip
            };

            var exchageName = "exchange_weather";
            var queueName = "queue_weather";
            var routeKey = "key_weather";

            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    channel.ExchangeDeclare(exchageName, ExchangeType.Direct, false, false);
                    channel.QueueDeclare(queueName, false, false, false, null);
                    channel.QueueBind(queueName, exchageName, routeKey, null);

                    var push = new
                    {
                        Id = Guid.NewGuid().ToString("N"),
                        SendTime = DateTime.Now,
                        Title = "天气预报",
                        Content = "天气预报",
                        RequestUrl = "http://t.weather.sojson.com/api/weather/city/101021300",
                        ViewUrl = "https://weather.qq.com"
                    };
                    var message = JsonConvert.SerializeObject(push);
                    var sendBytes = Encoding.UTF8.GetBytes(message);
                    //发布消息
                    channel.BasicPublish(exchageName, routeKey, null, sendBytes);
                    Console.Write(message);
                    _logger.LogInformation("success");
                }
            }
            return Task.CompletedTask;
        }
    }
}
