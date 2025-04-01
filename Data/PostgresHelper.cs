using Npgsql;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace MobileAPI.Helpers;

public class PostgresHelper
{
    private readonly string _connectionString;

    public PostgresHelper(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("PostgresConnection");
    }

    public List<string> GetUsers()
    {
        var users = new List<string>();

        using (var conn = new NpgsqlConnection(_connectionString))
        {
            conn.Open();
            using (var cmd = new NpgsqlCommand("SELECT name FROM Users;", conn))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    users.Add(reader.GetString(0));
                }
            }
        }

        return users;
    }

    public List<string> CreateUsers()
    {
        var users = new List<string>();

        using (var conn = new NpgsqlConnection(_connectionString))
        {
            conn.Open();
            using (var cmd = new NpgsqlCommand("CREATE TABLE Users ( userid serial primary key,  name VARCHAR(40) not null,email VARCHAR(40) not null);", conn))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    users.Add(reader.GetString(0));
                }
            }
        }

        return users;
    }

    public void InsertUser(string name, string email)
    {
        using (var conn = new NpgsqlConnection(_connectionString))
        {
            conn.Open();
            using (var cmd = new NpgsqlCommand("INSERT INTO Users (name, email) VALUES (@name, @email);", conn))
            {
                cmd.Parameters.AddWithValue("name", name);
                cmd.Parameters.AddWithValue("email", email);
                cmd.ExecuteNonQuery();
            }
        }
    }
}
