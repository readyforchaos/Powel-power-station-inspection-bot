using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FieldWorkerBot.DataAccess.InternalDomain
{
    public class ObjectItem
    {
        public string Id { get; set; }
        public string ExternalId { get; set; }
        public GeoJson GeoJsonObject { get; set; }
        public string Name { get; set; }
        public string ObjectType { get; set; }
        public string Route { get; set; }
        public string District { get; set; }
        public string Municipality { get; set; }
        public string SubstationName { get; set; }
        public string Address { get; set; }
        public int? ConstructionYear { get; set; }
        public int ObservationCount { get; set; }
        public IEnumerable<InspectionInfo> InspectionInfos { get; set; }
        public bool HasPlannedObservations { get; set; }
        public int CurrentStatusCode { get; set; }
    }

    public class InspectionInfo
    {
        public string InspectionType { get; set; }
        public int StatusCode { get; set; }
        public bool InspectionPlanned { get; set; }
        public DateTime? LastInspectionDate { get; set; }
        public DateTime? NextInspectionDate { get; set; }
    }
}
