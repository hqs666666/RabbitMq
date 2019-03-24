using System.Collections.Generic;
using System.Linq;
using CSRedis;
using Newtonsoft.Json;
using ReptileDashboard.Models;

namespace ReptileDashboard.Helpers
{
    public class RedisService
    {
        public RedisService()
        {
            RedisHelper.Initialization(new CSRedisClient("t.cn:6379,ssl=false"));
        }

        public void Set(string key, object value, int expireSeconds = -1)
        {
            RedisHelper.Set(key, value, expireSeconds);
        }

        public T Get<T>(string key)
        {
            return RedisHelper.Get<T>(key);
        }

        public string ExistUrlBackLink(string key,string url)
        {
            var list = new List<RequestList>();
            foreach (var item in RedisHelper.LRange(key, 0, -1))
            {
                var value = JsonConvert.DeserializeObject<RequestList>(item);
                list.Add(value);
            }

            var res = list.FirstOrDefault(p => p.Request_url == url);
            return res == null ? string.Empty : res.Red_url;
        }

        public List<T> GetList<T>(string key)
        {
            var list = new List<T>();
            //遍历链表元素（start: 0,end: -1即可返回所有元素）
            foreach (var item in RedisHelper.LRange(key, 0, 15))
            {
                var value = JsonConvert.DeserializeObject<T>(item);
                list.Add(value);
            }
            return list;
        }

        public List<string> GetListNotJson(string key)
        {
            var list = new List<string>();
            //遍历链表元素（start: 0,end: -1即可返回所有元素）
            foreach (var item in RedisHelper.LRange(key, 0, -1))
            {
                list.Add(item);
            }
            return list;
        }

        public void Remove(params string[] key)
        {
            RedisHelper.Del(key);
        }
    }
}
