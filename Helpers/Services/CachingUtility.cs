using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PTAUpdater.Helpers.Services
{
    public static class CachingUtility
    {
        private static readonly IMemoryCache _memoryCache = new MemoryCache(new MemoryCacheOptions());

        public static void AddValueToCache(string channel, string key)
        {
            try
            {
                var cacheExpiryOptions = new MemoryCacheEntryOptions();
                cacheExpiryOptions.AbsoluteExpiration = DateTime.Now.AddSeconds(600);
                cacheExpiryOptions.Priority = CacheItemPriority.High;
                cacheExpiryOptions.SlidingExpiration = TimeSpan.FromSeconds(450);

                _memoryCache.Set(channel, key, cacheExpiryOptions);

            }
            catch (Exception ex)
            {

            }
        }

        public static string GetValueFromCache(string channel)
        {
            try
            {
                var result = _memoryCache.Get(channel);
                return result?.ToString();
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
