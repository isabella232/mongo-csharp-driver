/* Copyright 2013-present MongoDB Inc.
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
using MongoDB.Driver.Core.Misc;
using MongoDB.Shared;

namespace MongoDB.Driver.Core.Configuration
{
    /// <summary>
    /// Represents settings for a connection pool.
    /// </summary>
    public class ConnectionPoolSettings
    {
        // fields
        private readonly TimeSpan _maintenanceInterval;
        private readonly int _maxConnections;
        private readonly int _minConnections;
        private readonly int _waitQueueSize;
        private readonly TimeSpan _waitQueueTimeout;
        private readonly int _maxConnecting;
        private readonly bool _isPausable;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionPoolSettings"/> class.
        /// </summary>
        /// <param name="maintenanceInterval">The maintenance interval.</param>
        /// <param name="maxConnections">The maximum number of connections.</param>
        /// <param name="minConnections">The minimum number of connections.</param>
        /// <param name="waitQueueSize">Size of the wait queue.</param>
        /// <param name="waitQueueTimeout">The wait queue timeout.</param>
        public ConnectionPoolSettings(
            Optional<TimeSpan> maintenanceInterval = default(Optional<TimeSpan>),
            Optional<int> maxConnections = default(Optional<int>),
            Optional<int> minConnections = default(Optional<int>),
            Optional<int> waitQueueSize = default(Optional<int>),
            Optional<TimeSpan> waitQueueTimeout = default(Optional<TimeSpan>))
            : this(maintenanceInterval: maintenanceInterval,
                  maxConnections: maxConnections,
                  minConnections: minConnections,
                  waitQueueSize: waitQueueSize,
                  waitQueueTimeout: waitQueueTimeout,
                  isPausable: true)
        {
        }

        private ConnectionPoolSettings(
            Optional<TimeSpan> maintenanceInterval = default(Optional<TimeSpan>),
            Optional<int> maxConnections = default(Optional<int>),
            Optional<int> minConnections = default(Optional<int>),
            Optional<int> waitQueueSize = default(Optional<int>),
            Optional<TimeSpan> waitQueueTimeout = default(Optional<TimeSpan>),
            Optional<bool> isPausable = default(Optional<bool>),
            Optional<int> maxConnecting = default(Optional<int>))
        {
            _maintenanceInterval = Ensure.IsInfiniteOrGreaterThanOrEqualToZero(maintenanceInterval.WithDefault(TimeSpan.FromMinutes(1)), nameof(maintenanceInterval));
            _maxConnections = Ensure.IsGreaterThanZero(maxConnections.WithDefault(100), nameof(maxConnections));
            _maxConnecting = Ensure.IsGreaterThanZero(maxConnecting.WithDefault(MongoInternalDefaults.ConnectionPool.MaxConnecting), nameof(maxConnecting));
            _minConnections = Ensure.IsGreaterThanOrEqualToZero(minConnections.WithDefault(0), nameof(minConnections));
#pragma warning disable CS0618 // Type or member is obsolete
            _waitQueueSize = Ensure.IsGreaterThanOrEqualToZero(waitQueueSize.WithDefault(ConnectionStringConversions.GetComputedWaitQueueSize(_maxConnections, 5)), nameof(waitQueueSize));
#pragma warning restore CS0618 // Type or member is obsolete
            _waitQueueTimeout = Ensure.IsInfiniteOrGreaterThanOrEqualToZero(waitQueueTimeout.WithDefault(TimeSpan.FromMinutes(2)), nameof(waitQueueTimeout));

            _isPausable = isPausable.WithDefault(true);
        }

        // properties
        /// <summary>
        /// Gets the maintenance interval.
        /// <see cref="System.Threading.Timeout.Infinite"/> values indicates that maintenance thread is disabled.
        /// </summary>
        /// <value>
        /// The maintenance interval.
        /// </value>
        public TimeSpan MaintenanceInterval
        {
            get { return _maintenanceInterval; }
        }

        /// <summary>
        /// Gets the maximum number of connections.
        /// </summary>
        /// <value>
        /// The maximum number of connections.
        /// </value>
        public int MaxConnections
        {
            get { return _maxConnections; }
        }

        /// <summary>
        /// Gets the minimum number of connections.
        /// </summary>
        /// <value>
        /// The minimum number of connections.
        /// </value>
        public int MinConnections
        {
            get { return _minConnections; }
        }

        /// <summary>
        /// Gets the size of the wait queue.
        /// </summary>
        /// <value>
        /// The size of the wait queue.
        /// </value>
        [Obsolete("This property will be removed in a later release.")]
        public int WaitQueueSize
        {
            get { return _waitQueueSize; }
        }

        /// <summary>
        /// Gets the wait queue timeout.
        /// </summary>
        /// <value>
        /// The wait queue timeout.
        /// </value>
        public TimeSpan WaitQueueTimeout
        {
            get { return _waitQueueTimeout; }
        }

        // internal properties
        internal int MaxConnecting
        {
            get { return _maxConnecting; }
        }

        internal bool IsPausable
        {
            get { return _isPausable; }
        }

        // methods
        /// <summary>
        /// Returns a new ConnectionPoolSettings instance with some settings changed.
        /// </summary>
        /// <param name="maintenanceInterval">The maintenance interval.</param>
        /// <param name="maxConnections">The maximum connections.</param>
        /// <param name="minConnections">The minimum connections.</param>
        /// <param name="waitQueueSize">Size of the wait queue.</param>
        /// <param name="waitQueueTimeout">The wait queue timeout.</param>
        /// <returns>A new ConnectionPoolSettings instance.</returns>
        public ConnectionPoolSettings With(
            Optional<TimeSpan> maintenanceInterval = default(Optional<TimeSpan>),
            Optional<int> maxConnections = default(Optional<int>),
            Optional<int> minConnections = default(Optional<int>),
            Optional<int> waitQueueSize = default(Optional<int>),
            Optional<TimeSpan> waitQueueTimeout = default(Optional<TimeSpan>))
        {
            return new ConnectionPoolSettings(
                maintenanceInterval: maintenanceInterval.WithDefault(_maintenanceInterval),
                maxConnections: maxConnections.WithDefault(_maxConnections),
                minConnections: minConnections.WithDefault(_minConnections),
                waitQueueSize: waitQueueSize.WithDefault(_waitQueueSize),
                waitQueueTimeout: waitQueueTimeout.WithDefault(_waitQueueTimeout));
        }

        internal ConnectionPoolSettings WithInternal(
            Optional<bool> isPausable = default(Optional<bool>),
            Optional<int> maxConnecting = default(Optional<int>)) =>
            new ConnectionPoolSettings(
                maintenanceInterval: _maintenanceInterval,
                maxConnections: _maxConnections,
                minConnections: _minConnections,
                waitQueueSize: _waitQueueSize,
                waitQueueTimeout: _waitQueueTimeout,
                isPausable: isPausable.WithDefault(_isPausable),
                maxConnecting: maxConnecting.WithDefault(_maxConnecting));
    }
}
