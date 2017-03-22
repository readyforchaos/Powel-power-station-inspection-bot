using System;
using System.Linq;
using FieldWorkerBot.DataAccess.Infrastructure.Coordinates;
using FieldWorkerBot.DataAccess.InternalDomain;
using Microsoft.Azure.Documents.Spatial;
using Powel.ArcGIS.RestApi;

namespace FieldWorkerBot.DataAccess.Infrastructure.ArcGis
{
    public class ObjectRestApiFeatureSerializer : IRestApiFeatureSerializer<ObjectItem>
    {
        private const string ArcGisObjectIdProperty = "OBJECTID";
        private const string YearPostfix = "y";

        public ObjectItem ToObject(Feature feature)
        {
            var esriPoint = feature.Geometry as EsriPoint;
            var coordinates = CoordinateTools.ConvertWebMercatorToGps(esriPoint.X, esriPoint.Y);
            var newObject = new ObjectItem
            {
                GeoJsonObject = new GeoJson
                {
                    type = GeoObjectType.Feature,
                    geometry = new Point(coordinates[0], coordinates[1])
                },
                Id = feature.Attributes[ArcGisObjectIdProperty].ToString(),
                ExternalId = feature.Attributes[nameof(ObjectItem.ExternalId)].ToString(),
                Name = feature.Attributes[nameof(ObjectItem.Name)].ToString(),
                ObjectType = feature.Attributes[nameof(ObjectItem.ObjectType)].ToString(),
                Route = feature.Attributes[nameof(ObjectItem.Route)].ToString(),
                District = feature.Attributes[nameof(ObjectItem.District)].ToString(),
                Municipality = feature.Attributes[nameof(ObjectItem.Municipality)].ToString(),
                SubstationName = feature.Attributes.ContainsKey(nameof(ObjectItem.SubstationName)) ? feature.Attributes[nameof(ObjectItem.SubstationName)]?.ToString() : "",
                Address = feature.Attributes.ContainsKey(nameof(ObjectItem.Address)) ? feature.Attributes[nameof(ObjectItem.Address)]?.ToString() : "",
                ConstructionYear = (int?)(long?)feature.Attributes[nameof(ObjectItem.ConstructionYear)],
                ObservationCount = (int)(long)feature.Attributes[nameof(ObjectItem.ObservationCount)],
                HasPlannedObservations = feature.Attributes[nameof(ObjectItem.HasPlannedObservations)] != null && (int)(long)feature.Attributes[nameof(ObjectItem.HasPlannedObservations)] == 1,
                CurrentStatusCode = (int?)(long?)feature.Attributes[nameof(ObjectItem.CurrentStatusCode)] ?? 0
            };
            var inspectionInfos =
                feature.Attributes.Keys.Where(k => k.StartsWith(nameof(InspectionInfo.InspectionPlanned)))
                    .Select(key => key.Replace(nameof(InspectionInfo.InspectionPlanned), "").Replace(YearPostfix, ""))
                    .Select(inspectionType => new InspectionInfo
                    {
                        InspectionType = inspectionType,
                        InspectionPlanned =
                            feature.Attributes[
                                $"{nameof(InspectionInfo.InspectionPlanned)}{inspectionType}{YearPostfix}"] != null
                                ? (int) (long)
                                    feature.Attributes[
                                        $"{nameof(InspectionInfo.InspectionPlanned)}{inspectionType}{YearPostfix}"] == 1
                                : false,
                        StatusCode =
                            feature.Attributes[$"{nameof(InspectionInfo.StatusCode)}{inspectionType}{YearPostfix}"] != null
                                ? (int) (long)
                                    feature.Attributes[
                                        $"{nameof(InspectionInfo.StatusCode)}{inspectionType}{YearPostfix}"]
                                : -1,
                        LastInspectionDate =
                            feature.Attributes[
                                $"{nameof(InspectionInfo.LastInspectionDate)}{inspectionType}{YearPostfix}"] != null
                                ? DateTimeOffset.FromUnixTimeMilliseconds(
                                    (long)feature.Attributes[
                                        $"{nameof(InspectionInfo.LastInspectionDate)}{inspectionType}{YearPostfix}"]).UtcDateTime
                                : (DateTime?) null,
                        NextInspectionDate =
                            feature.Attributes[
                                $"{nameof(InspectionInfo.NextInspectionDate)}{inspectionType}{YearPostfix}"] != null
                                ? DateTimeOffset.FromUnixTimeMilliseconds(
                                    (long)feature.Attributes[
                                        $"{nameof(InspectionInfo.NextInspectionDate)}{inspectionType}{YearPostfix}"]).UtcDateTime
                                : (DateTime?)null
                    }).ToList();
            newObject.InspectionInfos = inspectionInfos;
            return newObject;
        }

        public Feature ToFeature(ObjectItem item, Field[] fields)
        {
            var coordinates = CoordinateTools.ConvertGpsToWebMercator(item.GeoJsonObject.geometry.Position.Longitude,
                item.GeoJsonObject.geometry.Position.Latitude);
            var feature = new Feature
            {
                Geometry =
                    new EsriPoint { X = coordinates[0], Y = coordinates[1] },
                Attributes =
                {
                    [nameof(item.ExternalId)] = item.ExternalId,
                    [nameof(item.Name)] = item.Name,
                    [nameof(item.ObjectType)] = item.ObjectType,
                    [nameof(item.Route)] = item.Route,
                    [nameof(item.District)] = item.District,
                    [nameof(item.Municipality)] = item.Municipality,
                    [nameof(item.SubstationName)] = item.SubstationName,
                    [nameof(item.Address)] = item.Address,
                    [nameof(item.ConstructionYear)] = item.ConstructionYear,
                    [nameof(item.ObservationCount)] = item.ObservationCount,
                    [nameof(item.HasPlannedObservations)] = item.HasPlannedObservations ? 1 : 0,
                    [nameof(item.CurrentStatusCode)] = item.CurrentStatusCode
                }
            };
            if (!string.IsNullOrWhiteSpace(item.Id))
                feature.Attributes[ArcGisObjectIdProperty] = long.Parse(item.Id);
            if (item.InspectionInfos != null)
            {
                foreach (var inspectionInfo in item.InspectionInfos)
                {
                    feature.Attributes[
                        $"{nameof(inspectionInfo.StatusCode)}{inspectionInfo.InspectionType}{YearPostfix}"] =
                        inspectionInfo.StatusCode;
                    feature.Attributes[
                        $"{nameof(inspectionInfo.InspectionPlanned)}{inspectionInfo.InspectionType}{YearPostfix}"] =
                        (inspectionInfo.InspectionPlanned ? 1 : 0);
                    feature.Attributes[
                        $"{nameof(inspectionInfo.LastInspectionDate)}{inspectionInfo.InspectionType}{YearPostfix}"] =
                        inspectionInfo.LastInspectionDate;
                    feature.Attributes[
                        $"{nameof(inspectionInfo.NextInspectionDate)}{inspectionInfo.InspectionType}{YearPostfix}"] =
                        inspectionInfo.NextInspectionDate;
                }
            }

            var missingFields =
                fields.Where(
                    f =>
                        f.Name.StartsWith(nameof(InspectionInfo.InspectionPlanned)) &&
                        !feature.Attributes.Keys.Contains(f.Name)).Select(f => f.Name);
            foreach (var missingField in missingFields)
            {
                feature.Attributes[missingField] = 0;
            }
            missingFields =
                fields.Where(
                    f =>
                        f.Name.StartsWith(nameof(InspectionInfo.StatusCode)) &&
                        !feature.Attributes.Keys.Contains(f.Name)).Select(f => f.Name);
            foreach (var missingField in missingFields)
            {
                feature.Attributes[missingField] = -1;
            }

            return feature;
        }
    }
}
