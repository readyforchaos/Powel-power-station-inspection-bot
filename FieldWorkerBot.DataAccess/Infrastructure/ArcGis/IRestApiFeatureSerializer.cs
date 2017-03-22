using Powel.ArcGIS.RestApi;

namespace FieldWorkerBot.DataAccess.Infrastructure.ArcGis
{
    public interface IRestApiFeatureSerializer<T> where T:class
    {
        T ToObject(Feature feature);
        Feature ToFeature(T item, Field[] fields);
    }
}