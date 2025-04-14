using Npgsql;
using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

namespace MobileAPI.Helpers;

public class PostgresHelper
{
    private readonly string _connectionString;

    public PostgresHelper(IConfiguration configuration)
    {
        _connectionString = Environment.GetEnvironmentVariable("DATABASE_URL");
    }

    public List<UserData> GetUsers()
    {
        var users = new List<UserData>();

        using (var conn = new NpgsqlConnection(_connectionString))
        {
            conn.Open();
            using (var cmd = new NpgsqlCommand("SELECT userid,name,email FROM Users order by userid;", conn))
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


    public bool UpdateUser(int userid, string name, string email)
    {
        using (var conn = new NpgsqlConnection(_connectionString))
        {
            conn.Open();
            using (var cmd = new NpgsqlCommand("UPDATE  Users set name=@name , email=@email  where  userid=@userid", conn))
            {
                cmd.Parameters.AddWithValue("name", name);
                cmd.Parameters.AddWithValue("email", email);
                cmd.Parameters.AddWithValue("userid", userid);
                cmd.ExecuteNonQuery();
            }
        }

        return true;
    }


    public List<object> GetPolygons()
    {
        var list = new List<object>();
        using var conn = new NpgsqlConnection(_connectionString);
        conn.Open();

        using var cmd = new NpgsqlCommand("SELECT id, name, description, ST_AsGeoJSON(polygon) AS geojson,color FROM location_drawings", conn);
        using var reader = cmd.ExecuteReader();
        while (reader.Read())
        {
            list.Add(new
            {
                Id = reader.GetInt32(0),
                Name = reader.GetString(1),
                Description = reader.GetString(2),
                Polygon = reader.GetString(3),// GeoJSON
                Color=reader.GetString(4)
            });
        }
        return list;
    }

    public bool InsertPolygon(string name, string description, List<List<double>> coordinates)
    {
        if (coordinates.Count < 4) return false; // Needs at least 4 points to form a closed polygon

        // Ensure it's closed
        if (!coordinates[0].SequenceEqual(coordinates[^1]))
        {
            coordinates.Add(coordinates[0]);
        }

        // Convert to WKT
        string polygonWKT = "POLYGON((" +
            string.Join(",", coordinates.Select(c => $"{c[0]} {c[1]}")) +
            "))";

        using var conn = new NpgsqlConnection(_connectionString);
        conn.Open();

        using var cmd = new NpgsqlCommand("INSERT INTO location_drawings (name, description, polygon) VALUES (@name, @desc, ST_GeomFromText(@wkt, 4326))", conn);
        cmd.Parameters.AddWithValue("name", name);
        cmd.Parameters.AddWithValue("desc", description);
        cmd.Parameters.AddWithValue("wkt", polygonWKT);

        return cmd.ExecuteNonQuery() > 0;
    }
}
  

public class UserData
{
    public string UserId { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
}
