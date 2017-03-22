using System.Collections.Generic;
using Newtonsoft.Json;

namespace FieldWorkerBot.DataAccess.InternalDomain
{
    public class JobSpecification
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        public string TenantName { get; set; }
        public string InspectionType { get; set; }
        public string InspectionObjectType { get; set; }
        public string Version { get; set; }
        public List<ControlPoint> ControlPoints { get; set; }
    }

    public class ControlPoint
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Position { get; set; }
        public string Group { get; set; }
        public int GroupPosition { get; set; }
        public int FixMonth { get; set; }
        public string ControlPointStatusType { get; set; }
    }
}
