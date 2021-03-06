﻿using System.Threading.Tasks;
using MongoDB.Bson;
using MongoDB.Driver;
using Ray.Core.Snapshot;
using Ray.Core.Storage;

namespace Ray.Storage.MongoDB
{
    public class FollowSnapshotStorage<PrimaryKey> : IFollowSnapshotStorage<PrimaryKey>
    {
        readonly StorageConfig grainConfig;
        public FollowSnapshotStorage(StorageConfig table)
        {
            grainConfig = table;
        }
        public Task Delete(PrimaryKey id)
        {
            var filter = Builders<BsonDocument>.Filter.Eq("StateId", id);
            return grainConfig.Storage.GetCollection<BsonDocument>(grainConfig.DataBase, grainConfig.GetFollowStateTable()).DeleteManyAsync(filter);
        }

        public async Task<FollowSnapshot<PrimaryKey>> Get(PrimaryKey id)
        {
            var filter = Builders<BsonDocument>.Filter.Eq("StateId", id);
            var cursor = await grainConfig.Storage.GetCollection<BsonDocument>(grainConfig.DataBase, grainConfig.GetFollowStateTable()).FindAsync<BsonDocument>(filter);
            var document = await cursor.FirstOrDefaultAsync();
            if (document != default)
            {
                return new FollowSnapshot<PrimaryKey>()
                {
                    StateId = id,
                    Version = document["Version"].AsInt64,
                    DoingVersion = document["Version"].AsInt64,
                    StartTimestamp = document["StartTimestamp"].AsInt64
                };
            }
            return default;
        }

        public Task Insert(FollowSnapshot<PrimaryKey> snapshot)
        {
            var doc = new BsonDocument
            {
                { "StateId", BsonValue.Create(snapshot.StateId) },
                { "Version", snapshot.Version },
                { "StartTimestamp", snapshot.StartTimestamp }
            };
            return grainConfig.Storage.GetCollection<BsonDocument>(grainConfig.DataBase, grainConfig.GetFollowStateTable()).InsertOneAsync(doc);
        }

        public Task Update(FollowSnapshot<PrimaryKey> snapshot)
        {
            var filter = Builders<BsonDocument>.Filter.Eq("StateId", snapshot.StateId);
            var update = Builders<BsonDocument>.Update.Set("Version", snapshot.Version).Set("StartTimestamp", snapshot.StartTimestamp);
            return grainConfig.Storage.GetCollection<BsonDocument>(grainConfig.DataBase, grainConfig.GetFollowStateTable()).UpdateOneAsync(filter, update);
        }

        public Task UpdateStartTimestamp(PrimaryKey id, long timestamp)
        {
            var filter = Builders<BsonDocument>.Filter.Eq("StateId", id);
            var update = Builders<BsonDocument>.Update.Set("StartTimestamp", timestamp);
            return grainConfig.Storage.GetCollection<BsonDocument>(grainConfig.DataBase, grainConfig.GetFollowStateTable()).UpdateOneAsync(filter, update);
        }
    }
}
