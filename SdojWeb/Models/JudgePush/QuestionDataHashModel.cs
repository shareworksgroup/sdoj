using System;
using Newtonsoft.Json;
using SdojWeb.Infrastructure.Mapping;
using SdojWeb.Models.DbModels;

namespace SdojWeb.Models
{
    public class QuestionDataHashModel : IMapFrom<QuestionData>
    {
        [JsonProperty("a")]
        public int Id { get; set; }

        [JsonProperty("b")]
        public DateTime UpdateTime { get; set; }
    }
}