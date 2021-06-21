﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace GrpcService.Server.Models
{
    public class Post
    {
        [JsonPropertyName("userId")]
        public string UserId { get; set; }
        [JsonPropertyName("title")]
        public string Title { get; set; }
        [JsonPropertyName("body")]
        public string Body { get; set; }
    }
}
