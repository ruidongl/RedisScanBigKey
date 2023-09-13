# RedisScanBigKey
C# code sample to scan through an Azure Redis cache and output big keys using .Net 6.0

Usage: RedisScanBigKey <redisConnectionString (required, get from Azure portal)> <bigkey Threshold in bytes (optional, default 100KB)> <pageSize per scan of cache (optional, default 1000)>

This is a sample code that demonstrates how to utilize the Redis "SCAN" command to browse through all keys within an Azure Redis Cache and display those keys that exceed a specified threshold value in size. It's important to note that while this code provides a functional example, it might not be the most efficient method for extracting large keys from the Redis Cache. Additionally, it may not cover all possible edge cases or scenarios.


