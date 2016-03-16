using System.Collections.Generic;
using System.Linq;
using IsThereAnyNews.EntityFramework;
using IsThereAnyNews.EntityFramework.Models;

namespace IsThereAnyNews.DataAccess.Implementation
{
    public class UpdateRepository : IUpdateRepository
    {
        private readonly ItanDatabaseContext database;

        public UpdateRepository(ItanDatabaseContext database)
        {
            this.database = database;
        }

        public List<RssChannel> LoadAllGlobalRssChannels()
        {
            var rssChannels = this.database.RssChannels.ToList();
            return rssChannels;
        }
    }
}