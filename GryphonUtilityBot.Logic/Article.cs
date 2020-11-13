﻿using System;
using Newtonsoft.Json;

namespace GryphonUtilityBot.Logic
{
    public sealed class Article
    {
        [JsonProperty]
        public Uri Uri { get; set; }
        [JsonProperty]
        public DateTime Date { get; set; }
        [JsonProperty]
        public string Name { get; set; }
    }
}
