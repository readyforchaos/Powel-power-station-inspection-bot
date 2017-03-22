using DotSpatial.Projections;

namespace FieldWorkerBot.DataAccess.Infrastructure.Coordinates
{
    public static class CoordinateTools
    {
        public static double[] ConvertToGps(double x, double y, int sourceEpsg)
        {
            return ConvertCoordinates(x, y, ProjectionInfo.FromEpsgCode(sourceEpsg),
                KnownCoordinateSystems.Geographic.World.WGS1984);
        }

        public static double[] ConvertGpsToWebMercator(double x, double y)
        {
            return ConvertCoordinates(x, y, KnownCoordinateSystems.Geographic.World.WGS1984,
                KnownCoordinateSystems.Projected.World.WebMercator);
        }

        public static double[] ConvertWebMercatorToGps(double x, double y)
        {
            return ConvertCoordinates(x, y, KnownCoordinateSystems.Projected.World.WebMercator,
                KnownCoordinateSystems.Geographic.World.WGS1984);
        }

        private static double[] ConvertCoordinates(double x, double y, ProjectionInfo sourceProjection,
            ProjectionInfo destProjection)
        {
            double[] xy = { x, y };
            double[] z = { 1 };
            Reproject.ReprojectPoints(xy, z, sourceProjection, destProjection, 0, 1);
            return xy;
        }
    }
}
