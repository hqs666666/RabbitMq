using System;
using System.Diagnostics;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using RabbitMQ.Client;
using ReptileDashboard.Helpers;
using ReptileDashboard.Models;

namespace ReptileDashboard.Controllers
{
    public class HomeController : Controller
    {
        private readonly RedisService redis = new RedisService();
        public IActionResult Message()
        {
            var data = redis.GetList<RequestList>("receive_msg");
            var model = new SendMessageDto
            {
                Id = Guid.NewGuid().ToString("N"),
                SendTime = DateTime.Now,
                RequestUrl = "http://www.win4000.com",
                ViewUrl = "http://reptile.t.cn"
            };
            ViewBag.Data = data;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Message(SendMessageDto model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var link = redis.ExistUrlBackLink("receive_msg", model.RequestUrl);
            if (!string.IsNullOrEmpty(link))
            {
                return Redirect(link);
            }
                
            SendMessage(model);
            TempData["Message"] = "添加成功，请稍后查看....";
            return RedirectToAction("Index");
        }

        public IActionResult Privacy(string code)
        {
            if (string.IsNullOrEmpty(code))
                return BadRequest();

            var imgs = redis.GetListNotJson(code);
            return View(imgs);
        }


        public IActionResult Index()
        {
            return View();
        }

        private void SendMessage(SendMessageDto model)
        {
            try
            {
                var factory = new ConnectionFactory
                {
                    UserName = "guest", //用户名
                    Password = "guest", //密码
                    HostName = "t.cn" //rabbitmq ip
                };

                var exchageName = "exchange_reptile";
                var queueName = "queue_reptile";
                var routeKey = "key_reptile";

                using (var connection = factory.CreateConnection())
                {
                    using (var channel = connection.CreateModel())
                    {
                        channel.ExchangeDeclare(exchageName, ExchangeType.Direct, false, false);
                        channel.QueueDeclare(queueName, false, false, false, null);
                        channel.QueueBind(queueName, exchageName, routeKey, null);

                        var message = JsonConvert.SerializeObject(model);
                        var sendBytes = Encoding.UTF8.GetBytes(message);
                        //发布消息
                        channel.BasicPublish(exchageName, routeKey, null, sendBytes);
                        Console.Write(message);
                    }
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
