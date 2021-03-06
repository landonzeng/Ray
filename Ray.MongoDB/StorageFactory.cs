﻿using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;
using Ray.Core.Serialization;
using Ray.Core.Storage;

namespace Ray.Storage.MongoDB
{
    public class StorageFactory : IStorageFactory
    {
        readonly IServiceProvider serviceProvider;
        readonly ISerializer serializer;
        public StorageFactory(
            IServiceProvider serviceProvider,
            ISerializer serializer)
        {
            this.serializer = serializer;
            this.serviceProvider = serviceProvider;
        }
        readonly ConcurrentDictionary<IStorageConfig, object> eventStorageDict = new ConcurrentDictionary<IStorageConfig, object>();
        public ValueTask<IEventStorage<PrimaryKey>> CreateEventStorage<PrimaryKey>(IStorageConfig config, PrimaryKey grainId)
        {
            if (config.Singleton)
            {
                var storage = eventStorageDict.GetOrAdd(config, key =>
                {
                    return new EventStorage<PrimaryKey>(serviceProvider, config as StorageConfig);
                });
                return new ValueTask<IEventStorage<PrimaryKey>>(storage as EventStorage<PrimaryKey>);
            }
            else
            {
                return new ValueTask<IEventStorage<PrimaryKey>>(new EventStorage<PrimaryKey>(serviceProvider, config as StorageConfig));
            }
        }
        readonly ConcurrentDictionary<IStorageConfig, object> stateStorageDict = new ConcurrentDictionary<IStorageConfig, object>();
        public ValueTask<ISnapshotStorage<PrimaryKey, State>> CreateSnapshotStorage<PrimaryKey, State>(IStorageConfig config, PrimaryKey grainId)
            where State : class, new()
        {
            if (config.Singleton)
            {
                var storage = stateStorageDict.GetOrAdd(config, key =>
                {
                    return new SnapshotStorage<PrimaryKey, State>(serializer, config as StorageConfig);
                });
                return new ValueTask<ISnapshotStorage<PrimaryKey, State>>(storage as SnapshotStorage<PrimaryKey, State>);
            }
            else
            {
                return new ValueTask<ISnapshotStorage<PrimaryKey, State>>(new SnapshotStorage<PrimaryKey, State>(serializer, config as StorageConfig));
            }
        }

        readonly ConcurrentDictionary<IStorageConfig, object> ArchiveStorageDict = new ConcurrentDictionary<IStorageConfig, object>();
        public ValueTask<IArchiveStorage<PrimaryKey, State>> CreateArchiveStorage<PrimaryKey, State>(IStorageConfig config, PrimaryKey grainId)
             where State : class, new()
        {
            if (config.Singleton)
            {
                var storage = ArchiveStorageDict.GetOrAdd(config, key =>
                {
                    return new ArchiveStorage<PrimaryKey, State>(serializer, config as StorageConfig);
                });
                return new ValueTask<IArchiveStorage<PrimaryKey, State>>(storage as IArchiveStorage<PrimaryKey, State>);
            }
            else
            {
                return new ValueTask<IArchiveStorage<PrimaryKey, State>>(new ArchiveStorage<PrimaryKey, State>(serializer, config as StorageConfig));
            }
        }

        readonly ConcurrentDictionary<IStorageConfig, object> FollowSnapshotStorageDict = new ConcurrentDictionary<IStorageConfig, object>();
        public ValueTask<IFollowSnapshotStorage<PrimaryKey>> CreateFollowSnapshotStorage<PrimaryKey>(IStorageConfig config, PrimaryKey grainId)
        {
            if (config.Singleton)
            {
                var storage = FollowSnapshotStorageDict.GetOrAdd(config, key =>
                {
                    return new FollowSnapshotStorage<PrimaryKey>(config as StorageConfig);
                });
                return new ValueTask<IFollowSnapshotStorage<PrimaryKey>>(storage as IFollowSnapshotStorage<PrimaryKey>);
            }
            else
            {
                return new ValueTask<IFollowSnapshotStorage<PrimaryKey>>(new FollowSnapshotStorage<PrimaryKey>(config as StorageConfig));
            }
        }
    }
}
