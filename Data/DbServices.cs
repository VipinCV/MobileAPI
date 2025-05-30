﻿namespace MobileAPI.Data
{
    using Npgsql;
    using System.Data;

    public class DbService
    {
        private readonly IConfiguration _config;

        public DbService(IConfiguration config)
        {
            _config = config;
        }

        public async Task ExecuteFunctionAsync(string sql, Dictionary<string, object> parameters = null)
        {
            await using var conn = new NpgsqlConnection(Environment.GetEnvironmentVariable("DATABASE_URL"));
            await conn.OpenAsync();

            await using var cmd = new NpgsqlCommand(sql, conn);

            if (parameters != null)
            {
                foreach (var kv in parameters)
                    cmd.Parameters.AddWithValue(kv.Key, kv.Value);
            }

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<List<T>> ReadAsync<T>(string sql, Func<IDataReader, T> map)
        {
            var result = new List<T>();

            await using var conn = new NpgsqlConnection(Environment.GetEnvironmentVariable("DATABASE_URL"));
            await conn.OpenAsync();

            await using var cmd = new NpgsqlCommand(sql, conn);
            await using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                result.Add(map(reader));
            }

            return result;
        }
    }

}
