using System;
using System.Collections.Generic;

namespace FieldWorkerBot.Domain
{
    public class Asset
    {
        public string Id { get; set; }
        public string ObjectId { get; set; }
        public string Name { get; set; }
        public int CurrentStatusCode { get; set; }
    }
}
