using Microsoft.Azure.Documents.Spatial;

namespace FieldWorkerBot.DataAccess.InternalDomain
{
    public class GeoJson
    {
        public GeoObjectType type;
        public GeoProperty properties;
        public Point geometry;
    }
}