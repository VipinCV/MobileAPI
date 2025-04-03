using StackExchange.Redis;
using System;

public class RedisHelper
{
    private static readonly Lazy<ConnectionMultiplexer> LazyConnection =
        new Lazy<ConnectionMultiplexer>(() =>
            ConnectionMultiplexer.Connect("redis://red-cvm0paq4d50c73ftt2pg:6379,abortConnect=false")); // Change to your Render Redis URL

    public static ConnectionMultiplexer Connection => LazyConnection.Value;
}
