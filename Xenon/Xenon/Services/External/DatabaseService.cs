#region

using System;
using System.Linq;
using Raven.Client.Documents;
using Raven.Client.ServerWide;
using Raven.Client.ServerWide.Operations;

#endregion

namespace Xenon.Services.External
{
    public class DatabaseService
    {
        private const string DatabaseName = "NewDiscordBot";
        private readonly ConfigurationService _configurationService;
        private readonly Lazy<IDocumentStore> _store;

        public DatabaseService(ConfigurationService configurationService)
        {
            _store = new Lazy<IDocumentStore>(CreateStore);
            _configurationService = configurationService;
            if (Store.Maintenance.Server.Send(new GetDatabaseNamesOperation(0, 5)).All(x => x != DatabaseName))
                Store.Maintenance.Server.Send(new CreateDatabaseOperation(new DatabaseRecord(DatabaseName)));
        }

        private IDocumentStore Store => _store.Value;

        public T GetObject<T>(object id) where T : class, new()
        {
            using (var session = Store.OpenSession())
            {
                var obj = session.Load<T>($"{id}") ?? new T();
                return obj;
            }
        }

        public void AddOrUpdateObject<T>(T entity, object id)
        {
            Store.AggressivelyCache();
            using (var session = Store.OpenSession())
            {
                session.Store(entity, $"{id}");
                session.SaveChanges();
            }
        }

        public void RemoveObject<T>(object entity)
        {
            using (var session = Store.OpenSession())
            {
                session.Delete((T) entity);
                session.SaveChanges();
            }
        }

        public void RemoveObject(object id)
        {
            using (var session = Store.OpenSession())
            {
                session.Delete($"{id}");
                session.SaveChanges();
            }
        }

        private IDocumentStore CreateStore()
        {
            var store = new DocumentStore
            {
                Urls = new[] {_configurationService.DatabaseConnectionString},
                Database = DatabaseName
            }.Initialize();
            return store;
        }
    }
}