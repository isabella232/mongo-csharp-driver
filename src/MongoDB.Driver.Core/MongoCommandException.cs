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
using System.Runtime.Serialization;
using MongoDB.Bson;
using MongoDB.Driver.Core.Connections;

namespace MongoDB.Driver
{
    /// <summary>
    /// Represents a MongoDB command exception.
    /// </summary>
    [Serializable]
    public class MongoCommandException : MongoServerException
    {
        // fields
        private readonly BsonDocument _command;
        private readonly BsonDocument _result;

        // constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="MongoCommandException"/> class.
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="message">The message.</param>
        /// <param name="command">The command.</param>
        public MongoCommandException(ConnectionId connectionId, string message, BsonDocument command)
            : this(connectionId, message, command, null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MongoCommandException"/> class.
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="message">The message.</param>
        /// <param name="command">The command.</param>
        /// <param name="result">The command result.</param>
        public MongoCommandException(ConnectionId connectionId, string message, BsonDocument command, BsonDocument result)
            : this(connectionId, message, command, result, innerException: null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MongoCommandException"/> class.
        /// </summary>
        /// <param name="connectionId">The connection identifier.</param>
        /// <param name="message">The message.</param>
        /// <param name="command">The command.</param>
        /// // <param name="result">The command result.</param>
        /// <param name="innerException">The inner exception.</param>
        ///
        public MongoCommandException(ConnectionId connectionId, string message, BsonDocument command, BsonDocument result, Exception innerException)
            : base(connectionId, message, innerException)
        {
            _command = command; // can be null
            _result = result; // can be null

            AddErrorLabelsFromCommandResult(this, result);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MongoCommandException"/> class.
        /// </summary>
        /// <param name="info">The SerializationInfo.</param>
        /// <param name="context">The StreamingContext.</param>
        protected MongoCommandException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _command = (BsonDocument)info.GetValue("_command", typeof(BsonDocument));
            _result = (BsonDocument)info.GetValue("_result", typeof(BsonDocument));
        }

        // properties
        /// <summary>
        /// Gets the error code.
        /// </summary>
        /// <value>
        /// The error code.
        /// </value>
        public int Code
        {
            get { return _result.GetValue("code", -1).ToInt32(); }
        }

        /// <summary>
        /// Gets the name of the error code.
        /// </summary>
        /// <value>
        /// The name of the error code.
        /// </value>
        public string CodeName
        {
            get { return _result.GetValue("codeName", null)?.AsString; }
        }

        /// <summary>
        /// Gets the command.
        /// </summary>
        /// <value>
        /// The command.
        /// </value>
        public BsonDocument Command
        {
            get { return _command; }
        }

        /// <summary>
        /// Gets the error message.
        /// </summary>
        /// <value>
        /// The error message.
        /// </value>
        public string ErrorMessage
        {
            get { return _result.GetValue("errmsg", "Unknown error.").AsString; }
        }

        /// <summary>
        /// Gets the command result.
        /// </summary>
        /// <value>
        /// The command result.
        /// </value>
        public BsonDocument Result
        {
            get { return _result; }
        }

        // methods
        /// <inheritdoc/>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("_command", _command);
            info.AddValue("_result", _result);
        }
    }
}
