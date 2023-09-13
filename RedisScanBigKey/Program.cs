using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

class Program
{
    private static int countKey = 0;

    static async Task Main(string[] args)
    {
        // Parse command-line arguments or use default values if not provided
        if (args.Length == 0 || args.Length > 3)
        {
            Console.WriteLine("Usage: RedisScanBigKey.exe <redisConnectionString (required, get from Azure portal)> <bigkey Threshod in bytes (optional, default 100KB)> <pageSize per scan of cache (optional, default 1000)>");
        }
        else
        {
            try
            {
                string redisConnectionString = args[0]; //format: <cachename>.redis.cache.windows.net:6380,password=<redis_key>,ssl=True,abortConnect=False
                long bigkeyThreshold = args.Length > 1 ? long.Parse(args[1]) : 1024 * 100; // 100KB default
                int pageSize = args.Length > 2 ? int.Parse(args[2]) : 1000;
                // Connect to the Redis server
                ConnectionMultiplexer redis = await ConnectionMultiplexer.ConnectAsync(redisConnectionString);
                IDatabase db = redis.GetDatabase();

                int cursor = 0;

                do
                {
                    // Perform a SCAN operation to retrieve a batch of keys
                    var scanResult = (RedisResult[])await db.ExecuteAsync("SCAN", cursor.ToString(), "COUNT", pageSize.ToString());

                    var keys = ((RedisResult[])scanResult[1]).Select(key => (string)key);

                    var tasks = new List<Task>();
                    foreach (var keyName in keys)
                    {
                        // Check each key's size asynchronously
                        tasks.Add(CheckAndPrintBigKeyAsync(db, keyName, bigkeyThreshold));
                    }
                    // Wait for all key checks to complete
                    await Task.WhenAll(tasks);
                    //get next cursor see https://redis.io/docs/manual/keyspace/ for usage of SCAN command
                    cursor = (int)scanResult[0];
                    //cursor = cursor = int.Parse(scanResult[0].ToString());

                } while (cursor > 0);

                Console.WriteLine($"Total number of keys above the threshold of {bigkeyThreshold} is {countKey}");
                //get total number of keys current in the cache
                long totalCount = (long)await db.ExecuteAsync("DBSIZE");
                Console.WriteLine($"Total number of keys in the cache is {totalCount}");

                redis.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");

            }
        }
    }

    static async Task CheckAndPrintBigKeyAsync(IDatabase db, string keyName, long threshold)
    {
        try
        {
            // Check the size of a Redis key and value
            RedisValue value = await db.StringGetAsync(keyName);
            if (keyName.Length > threshold || value.Length() > threshold)
            {
                Console.WriteLine($"Big Key {countKey}: {keyName}, Size: {value.Length()} bytes");
                Interlocked.Increment(ref countKey);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while checking key {keyName}: {ex.Message}");

        }
    }

}


