namespace FieldWorkerBot.DataAccess.InternalDomain
{
    public class Geometry
    {
        public GeoObjectType type;
        public double[] coordinates;

        public Geometry(double latitude, double longitude)
        {
            coordinates = new[] {latitude, longitude};
        }

        public Geometry()
        {
            
        }
    }
}