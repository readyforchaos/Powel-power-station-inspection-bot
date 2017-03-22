using System;
using System.Collections.Generic;

namespace FieldWorkerBot.DataAccess.Infrastructure.SettingsManager
{
    public interface ISettingsCache
    {
        IEnumerable<string> AllKeys { get; }
        T Get<T>(string key, string region = null) where T : class;
        void Remove(string key, string region = null);
        bool Contains(string key, string region = null);
        void Add(string key, object o, DateTimeOffset expiration, string region = null, string dependsOnKey = null);
    }
}