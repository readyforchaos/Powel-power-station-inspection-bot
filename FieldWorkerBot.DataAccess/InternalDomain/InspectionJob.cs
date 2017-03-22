using System;

namespace FieldWorkerBot.DataAccess.InternalDomain
{
    public class InspectionJob
    {
        public string Id { get; set; }
        public string ObjectId { get; set; }
        public string InspectionType { get; set; }
        public string JobSpecificationVersion { get; set; }
        public int InspectionYear { get; set; }
        public DateTime AddedDate { get; set; }
        public DateTime? DueDate { get; set; }
        public DateTime? InspectionDate { get; set; }
        public int StatusCode { get; set; }
    }
}