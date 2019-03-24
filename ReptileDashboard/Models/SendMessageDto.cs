using System;
using System.ComponentModel.DataAnnotations;

namespace ReptileDashboard.Models
{
    public class SendMessageDto
    {
        [Required]
        public string Id { get; set; }
        [Required]
        public DateTime SendTime { get; set; }
        [Required]
        public string Title { get; set; }
        [Required]
        public string Content { get; set; }
        [Required]
        public string RequestUrl { get; set; }
        [Required]
        public string ViewUrl { get; set; }
    }

    public class RequestList
    {
        public string Mid { get; set; }
        public DateTime SendTime { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public string Request_url { get; set; }
        public string Red_url { get; set; }
    }
}
