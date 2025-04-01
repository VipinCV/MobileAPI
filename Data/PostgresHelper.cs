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

    public List<UserData> GetUsers()
    {
        var users = new List<UserData>();

        using (var conn = new NpgsqlConnection(_connectionString))
        {
            conn.Open();
            using (var cmd = new NpgsqlCommand("SELECT userid,name,email FROM Users;", conn))
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    UserData item = new UserData();
                    item.UserId = reader.GetInt32(0).ToString();
                    item.Name = reader.GetString(1);
                    item.Email = reader.GetString(2);
                    users.Add(item);
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

    public bool DeleteUser(int userid)
    {
        var users = new List<string>();

        using (var conn = new NpgsqlConnection(_connectionString))
        {
            conn.Open();
            using (var cmd = new NpgsqlCommand("DELETE FROM  Users WHERE userid=" + userid.ToString(), conn))
            { cmd.ExecuteNonQuery(); }
        }

        return true;
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


    public bool UpdateUser(int id, string name, string email)
    {
        using (var conn = new NpgsqlConnection(_connectionString))
        {
            conn.Open();
            using (var cmd = new NpgsqlCommand("UPDATE  Users set name" + name + ", email=" + email + "where  userid=" + id.ToString(), conn))
            {
                //cmd.Parameters.AddWithValue("name", name);
                //cmd.Parameters.AddWithValue("email", email);
                cmd.ExecuteNonQuery();
            }
        }

        return true;
    }
}
  

public class UserData
{
    public string UserId { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
}
