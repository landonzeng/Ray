﻿using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Ray.Core.Storage;
using Ray.Storage.PostgreSQL;

namespace Ray.Grain
{
    public class PostgreSQLStorageConfig : IStorageConfiguration<StorageConfig, ConfigParameter>
    {
        readonly IOptions<SqlConfig> options;
        public PostgreSQLStorageConfig(IOptions<SqlConfig> options) => this.options = options;
        public Task Configure(IConfigureContainer<StorageConfig, ConfigParameter> container)
        {
            new SQLConfigureBuilder<long>((grain, id, parameter) =>new StorageConfig(options.Value.ConnectionDict["core_event"], "account_event", parameter != default && !string.IsNullOrEmpty(parameter.SnapshotTable) ? parameter.SnapshotTable : "account_state")).
                BindTo<Account>().BindTo<AccountRep>().BindTo<AccountDb>("account_db_state").BindTo<AccountFlow>("account_flow_state").Complete(container);

            return Task.CompletedTask;
        }
    }
}
