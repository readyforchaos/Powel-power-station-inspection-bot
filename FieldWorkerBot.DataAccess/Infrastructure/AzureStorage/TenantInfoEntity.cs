using System;
using Microsoft.WindowsAzure.Storage.Table;

namespace FieldWorkerBot.DataAccess.Infrastructure.AzureStorage
{
    public class TenantInfoEntity : TableEntity
    {
        public string ApplicationName { get; private set; }
        public string Tenant { get; private set; }
        public string Data { get; set; }

        public TenantInfoEntity() { }

        public TenantInfoEntity(string applicationName, string tenant)
        {
            if (applicationName == null)
                throw new ArgumentNullException("applicationName");
            ApplicationName = applicationName;

            if (tenant == null)
                throw new ArgumentNullException("tenant");
            Tenant = tenant;

            PartitionKey = applicationName;
            RowKey = tenant;
        }
    }
}
