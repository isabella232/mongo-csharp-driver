/* Copyright 2010-present MongoDB Inc.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

using System;
using System.Collections.Generic;
using System.Linq;
using MongoDB.Driver.Core;
using MongoDB.Driver.Core.Clusters;
using MongoDB.Driver.Core.Configuration;
using MongoDB.Driver.Core.TestHelpers.Logging;
using MongoDB.Driver.Linq;
using MongoDB.Driver.TestHelpers;

namespace MongoDB.Driver.Tests
{
    /// <summary>
    /// A static class to handle online test configuration.
    /// </summary>
    public static class DriverTestConfiguration
    {
        // private static fields
        private static Lazy<MongoClient> __client;
        private static Lazy<MongoClient> __clientWithMultipleShardRouters;
        private static CollectionNamespace __collectionNamespace;
        private static DatabaseNamespace __databaseNamespace;
        private static Lazy<IReadOnlyList<IMongoClient>> __directClientsToShardRouters;
        private static Lazy<MongoClient> __linq3Client;

        // static constructor
        static DriverTestConfiguration()
        {
            __client = new Lazy<MongoClient>(() => new MongoClient(GetClientSettings()), true);
            __clientWithMultipleShardRouters = new Lazy<MongoClient>(() => CreateClient(useMultipleShardRouters: true), true);
            __databaseNamespace = CoreTestConfiguration.DatabaseNamespace;
            __directClientsToShardRouters = new Lazy<IReadOnlyList<IMongoClient>>(
                () => CreateDirectClientsToHostsInConnectionString(CoreTestConfiguration.ConnectionStringWithMultipleShardRouters).ToList().AsReadOnly(),
                isThreadSafe: true);
            __collectionNamespace = new CollectionNamespace(__databaseNamespace, "testcollection");
            __linq3Client = new Lazy<MongoClient>(CreateLinq3Client, isThreadSafe: true);
        }

        // public static properties
        /// <summary>
        /// Gets the test client.
        /// </summary>
        public static MongoClient Client
        {
            get { return __client.Value; }
        }

        /// <summary>
        /// Gets the test client with multiple shard routers.
        /// </summary>
        public static MongoClient ClientWithMultipleShardRouters
        {
            get { return __clientWithMultipleShardRouters.Value; }
        }

        /// <summary>
        /// Sequence of clients that connect directly to the shard routers
        /// </summary>
        public static IReadOnlyList<IMongoClient> DirectClientsToShardRouters
        {
            get => __directClientsToShardRouters.Value;
        }

        /// <summary>
        /// Gets the collection namespace.
        /// </summary>
        /// <value>
        /// The collection namespace.
        /// </value>
        public static CollectionNamespace CollectionNamespace
        {
            get { return __collectionNamespace; }
        }

        /// <summary>
        /// Gets the database namespace.
        /// </summary>
        /// <value>
        /// The database namespace.
        /// </value>
        public static DatabaseNamespace DatabaseNamespace
        {
            get { return __databaseNamespace; }
        }

        /// <summary>
        /// Gets the LINQ3 test client.
        /// </summary>
        public static MongoClient Linq3Client
        {
            get { return __linq3Client.Value; }
        }

        // public static methods
        public static IEnumerable<IMongoClient> CreateDirectClientsToServersInClientSettings(MongoClientSettings settings)
        {
            foreach (var server in settings.Servers)
            {
                var singleServerSettings = settings.Clone();
                singleServerSettings.Server = server;
                yield return new MongoClient(singleServerSettings);
            }
        }

        public static IEnumerable<IMongoClient> CreateDirectClientsToHostsInConnectionString(ConnectionString connectionString)
        {
            return CreateDirectClientsToServersInClientSettings(MongoClientSettings.FromConnectionString(connectionString.ToString()));
        }

        public static DisposableMongoClient CreateDisposableClient(ILogger<DisposableMongoClient> logger = null)
        {
            return CreateDisposableClient((MongoClientSettings s) => { }, logger);
        }

        public static DisposableMongoClient CreateDisposableClient(Action<ClusterBuilder> clusterConfigurator, ILogger<DisposableMongoClient> logger = null)
        {
            return CreateDisposableClient((MongoClientSettings s) => s.ClusterConfigurator = clusterConfigurator, logger);
        }

        public static MongoClient CreateClient(
            Action<MongoClientSettings> clientSettingsConfigurator = null,
            bool useMultipleShardRouters = false)
        {
            var clusterType = CoreTestConfiguration.Cluster.Description.Type;
            if (clusterType != ClusterType.Sharded && clusterType != ClusterType.LoadBalanced)
            {
                // This option has no effect for non-sharded/load balanced topologies.
                useMultipleShardRouters = false;
            }

            var connectionString = useMultipleShardRouters
                ? CoreTestConfiguration.ConnectionStringWithMultipleShardRouters.ToString()
                : CoreTestConfiguration.ConnectionString.ToString();
            var clientSettings = MongoClientSettings.FromUrl(new MongoUrl(connectionString));
            clientSettings.ServerApi = CoreTestConfiguration.ServerApi;
            clientSettingsConfigurator?.Invoke(clientSettings);

            return new MongoClient(clientSettings);
        }

        public static DisposableMongoClient CreateDisposableClient(
            Action<MongoClientSettings> clientSettingsConfigurator,
            ILogger<DisposableMongoClient> logger,
            bool useMultipleShardRouters = false)
        {
            Action<MongoClientSettings> compositeClientSettingsConfigurator = s =>
            {
                EnsureUniqueCluster(s);
                clientSettingsConfigurator?.Invoke(s);
            };
            var client = CreateClient(compositeClientSettingsConfigurator, useMultipleShardRouters);
            return new DisposableMongoClient(client, logger);
        }

        public static DisposableMongoClient CreateDisposableClient(EventCapturer capturer, ILogger<DisposableMongoClient> logger = null)
        {
            return CreateDisposableClient((ClusterBuilder c) => c.Subscribe(capturer), logger);
        }

        public static DisposableMongoClient CreateDisposableClient(MongoClientSettings settings, ILogger<DisposableMongoClient> logger = null)
        {
            EnsureUniqueCluster(settings);
            return new DisposableMongoClient(new MongoClient(settings), logger);
        }

        private static MongoClient CreateLinq3Client()
        {
            var linq3ClientSettings = Client.Settings.Clone();
            linq3ClientSettings.LinqProvider = LinqProvider.V3;
            return new MongoClient(linq3ClientSettings);
        }

        public static MongoClientSettings GetClientSettings()
        {
            var connectionString = CoreTestConfiguration.ConnectionString.ToString();
            var clientSettings = MongoClientSettings.FromUrl(new MongoUrl(connectionString));

            var serverSelectionTimeoutString = Environment.GetEnvironmentVariable("MONGO_SERVER_SELECTION_TIMEOUT_MS");
            if (serverSelectionTimeoutString == null)
            {
                serverSelectionTimeoutString = "30000";
            }
            clientSettings.ServerSelectionTimeout = TimeSpan.FromMilliseconds(int.Parse(serverSelectionTimeoutString));
            clientSettings.ClusterConfigurator = cb => CoreTestConfiguration.ConfigureLogging(cb);
            clientSettings.ServerApi = CoreTestConfiguration.ServerApi;

            return clientSettings;
        }

        private static void EnsureUniqueCluster(MongoClientSettings settings)
        {
            // make the settings unique (as far as the ClusterKey is concerned) by instantiating a new ClusterConfigurator
            var configurator = settings.ClusterConfigurator;
            settings.ClusterConfigurator = b => { configurator?.Invoke(b); };
        }
    }
}
