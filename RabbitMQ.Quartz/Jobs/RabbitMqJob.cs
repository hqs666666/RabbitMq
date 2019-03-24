using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Quartz;
using RabbitMQ.Client;

namespace RabbitMQ.Quartz
{
    public class RabbitMqJob : IJob
    {
        private readonly ILogger _logger;

        public RabbitMqJob(ILogger<RabbitMqJob> logger)
        {
            _logger = logger;
        }

        public Task Execute(IJobExecutionContext context)
        {
            var factory = new ConnectionFactory
            {
                UserName = "guest",//用户名
                Password = "guest",//密码
                HostName = "t.cn"//rabbitmq ip
            };

            var exchageName = "exchange_jb";
            var queueName = "queue_jbcode";
            var routeKey = "key_jbcode";

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
                        Title = "lanyu's code",
                        Content = "code update",
                        RequestUrl = "http://idea.lanyus.com/getkey?userName=lan+yu",
                        ViewUrl = "http://devhqs.vicp.io"
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
