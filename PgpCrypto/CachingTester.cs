using Glav.CacheAdapter.Core.DependencyInjection;
using System;

namespace PgpCrypto
{
    public  class CachingTester
    {
        public void AddCache()
        {
            AppServices.Cache.ClearAll();
            var testData = AppServices.Cache.Get<MoreDummyData>("A key", DateTime.Now.AddMinutes(10), () =>
            {
                return new MoreDummyData();
            });
        }

    }

    public class MoreDummyData
    {
    }
}
