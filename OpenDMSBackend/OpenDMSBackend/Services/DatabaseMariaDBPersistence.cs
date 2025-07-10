using GRYLibrary.Core.APIServer.Services.Interfaces;
using GRYLibrary.Core.APIServer.Services.Trans;
using GRYLibrary.Core.Logging.GeneralPurposeLogger;
using GRYLibrary.Core.Logging.GRYLogger;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using OpenDMSBackend.Core.Database;
using OpenDMSBackend.Core.Services.Misc;
using System;

namespace OpenDMSBackend.Core.Services
{
    public sealed class DatabaseMariaDBPersistence : GenericPersistence, IPersistence
    {
        public DatabaseMariaDBPersistence(DbContextOptions<DatabaseContext> options, IGeneralLogger logger, ITimeService timeService, IDatabaseManager databaseManager, IGRYLog log, ISQLProvider sqlProvider) : base(options, logger, timeService, databaseManager, log, sqlProvider)
        {
        }
        public override MySqlParameter GetParameter(string parameterName, object? value, Type type)
        {
            return new MySqlParameter(parameterName, value ?? DBNull.Value)
            {
                MySqlDbType = this.GetType(type)
            };
        }
        private MySqlDbType GetType(Type type)
        {
            return type switch
            {
                var t when t == typeof(string) => MySqlDbType.VarChar,
                var t when t == typeof(int) => MySqlDbType.Int32,
                var t when t == typeof(long) => MySqlDbType.Int64,
                var t when t == typeof(short) => MySqlDbType.Int16,
                var t when t == typeof(bool) => MySqlDbType.Bit,
                var t when t == typeof(DateTime) => MySqlDbType.DateTime,
                var t when t == typeof(float) => MySqlDbType.Float,
                var t when t == typeof(double) => MySqlDbType.Double,
                var t when t == typeof(decimal) => MySqlDbType.Decimal,
                var t when t == typeof(Guid) => MySqlDbType.Guid,
                var t when t == typeof(sbyte) => MySqlDbType.Byte,
                var t when t == typeof(byte) => MySqlDbType.UByte,
                var t when t == typeof(byte[]) => MySqlDbType.VarBinary,
                var t when t == typeof(char) => MySqlDbType.VarChar,
                var t when t == typeof(TimeSpan) => MySqlDbType.Time,
                var t when t == typeof(UInt16) => MySqlDbType.UInt16,
                var t when t == typeof(UInt32) => MySqlDbType.UInt32,
                var t when t == typeof(UInt64) => MySqlDbType.UInt64,
                var t when t == typeof(UInt128) => MySqlDbType.UInt64,

                _ => throw new NotSupportedException($"Type '{type.FullName}' is not supported.")
            };
        }
    }
}
