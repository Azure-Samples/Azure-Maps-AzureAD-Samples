// ---------------------------------------------------------------------------------
// <copyright company="Microsoft">
//   Copyright (c) 2017 Microsoft Corporation. All rights reserved.
// </copyright>
// ---------------------------------------------------------------------------------

namespace Microsoft.AspNetCore.Authentication
{
    using Microsoft.Extensions.Caching.Distributed;
    using Microsoft.IdentityModel.Clients.ActiveDirectory;

    /// <summary>
    /// Simple opinionated sample of storing session state for access tokens based on a user.
    /// </summary>
    /// <remarks>
    /// Before implementing a cache like this for production, keep in mind the security responsibility 
    /// of encrypting the cache as it represents tokens which can be used to access secure resources.
    /// </remarks>
    public class NaiveSessionCache : TokenCache
    {
        private static readonly object FileLock = new object();
        readonly string UserObjectId = string.Empty;
        readonly string CacheId = string.Empty;
        readonly IDistributedCache cache = null;

        public NaiveSessionCache(string userId, IDistributedCache cache)
        {
            UserObjectId = userId;
            CacheId = UserObjectId + "_TokenCache";
            this.cache = cache;
            this.AfterAccess = AfterAccessNotification;
            this.BeforeAccess = BeforeAccessNotification;
            Load();
        }

        public void Load()
        {
            lock (FileLock)
            {
                this.DeserializeAdalV3(cache.Get(CacheId));
            }
        }

        public void Persist()
        {
            lock (FileLock)
            {
                // reflect changes in the persistent store
                cache.Set(CacheId, this.SerializeAdalV3());

                // once the write operation took place, restore the HasStateChanged bit to false
                this.HasStateChanged = false;
            }
        }

        public override void Clear()
        {
            base.Clear();
            cache.Remove(CacheId);
        }

        public override void DeleteItem(TokenCacheItem item)
        {
            base.DeleteItem(item);
            Persist();
        }

        // Triggered right before ADAL needs to access the cache.
        // Reload the cache from the persistent store in case it changed since the last access.
        void BeforeAccessNotification(TokenCacheNotificationArgs args)
        {
            Load();
        }

        // Triggered right after ADAL accessed the cache.
        void AfterAccessNotification(TokenCacheNotificationArgs args)
        {
            // if the access operation resulted in a cache update
            if (this.HasStateChanged)
            {
                Persist();
            }
        }
    }
}
