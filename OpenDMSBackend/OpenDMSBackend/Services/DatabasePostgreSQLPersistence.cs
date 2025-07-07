using GRYLibrary.Core.APIServer.Services.Interfaces;
using GRYLibrary.Core.APIServer.Services.Trans;
using GRYLibrary.Core.Logging.GeneralPurposeLogger;
using GRYLibrary.Core.Logging.GRYLogger;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using OpenDMSBackend.Core.Database;
using System;
using System.Data.Common;
using NpgsqlTypes;

namespace OpenDMSBackend.Core.Services
{
    public sealed class DatabasePostgreSQLPersistence : GenericPersistence, IPersistence
    {
        public DatabasePostgreSQLPersistence(DbContextOptions<DatabaseContext> options, IGeneralLogger logger, ITimeService timeService, IDatabaseManager databaseManager, IGRYLog log, ISQLProvider sqlProvider) : base(options, logger, timeService, databaseManager, log, sqlProvider)
        {
        }

        public override DbParameter GetParameter(string parameterName, object? value,Type type)
        {
            return new NpgsqlParameter(parameterName, value ?? DBNull.Value)
            {
                NpgsqlDbType = this.GetType(type)
            };
        }
        private NpgsqlDbType GetType(Type type)
        {
            return type switch
            {
                var t when t == typeof(string) => NpgsqlDbType.Text,
                var t when t == typeof(int) => NpgsqlDbType.Integer,
                var t when t == typeof(long) => NpgsqlDbType.Bigint,
                var t when t == typeof(short) => NpgsqlDbType.Smallint,
                var t when t == typeof(bool) => NpgsqlDbType.Boolean,
                var t when t == typeof(DateTime) => NpgsqlDbType.Timestamp,
                var t when t == typeof(float) => NpgsqlDbType.Real,
                var t when t == typeof(double) => NpgsqlDbType.Double,
                var t when t == typeof(decimal) => NpgsqlDbType.Numeric,
                var t when t == typeof(Guid) => NpgsqlDbType.Uuid,
                var t when t == typeof(byte[]) => NpgsqlDbType.Bytea,
                var t when t == typeof(char) => NpgsqlDbType.Char,
                var t when t == typeof(TimeSpan) => NpgsqlDbType.Interval,

                _ => throw new NotSupportedException($"Type '{type.FullName}' is not supported.")
            };
        }
    }
}
