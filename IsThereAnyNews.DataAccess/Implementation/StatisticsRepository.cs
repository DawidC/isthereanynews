namespace IsThereAnyNews.DataAccess.Implementation
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;

    using IsThereAnyNews.EntityFramework;
    using IsThereAnyNews.EntityFramework.Models.Entities;
    using IsThereAnyNews.EntityFramework.Models.Events;

    public class StatisticsRepository : IStatisticsRepository
    {
        private readonly ItanDatabaseContext database;

        public StatisticsRepository(ItanDatabaseContext database)
        {
            this.database = database;
        }

        public List<ChannelWithSubscriptionCount> GetToReadChannels(int count)
        {
            var list = this.database
                .RssChannels
                .Include(channel => channel.Subscriptions)
                .Select(this.ToChannelStatistics)
                .OrderByDescending(x => x.SubscriptionCount)
                .Take(count)
                .ToList();
            return list;
        }

        private ChannelWithSubscriptionCount ToChannelStatistics(RssChannel model)
        {
            var projection = new ChannelWithSubscriptionCount
            {
                Title = model.Title,
                Id = model.Id,
                SubscriptionCount = model.Subscriptions.Count
            };
            return projection;
        }

        public List<UserWithStatistics> GetUsersThatReadMostNews(int i)
        {
            var list = this.database.Users
                .Include(x => x.RssSubscriptionList)
                .Include(x => x.RssSubscriptionList.Select(c => c.RssEntriesToRead))
                .Select(this.ProjectToUserStatistics)
                .OrderByDescending(x => x.Count)
                .Take(i)
                .ToList();

            return list;
        }

        private UserWithStatistics ProjectToUserStatistics(User model)
        {
            var projection = new UserWithStatistics
            {
                Name = model.DisplayName,
                Id = model.Id,
                Count = model.RssSubscriptionList
                    .SelectMany(c => c.RssEntriesToRead)
                    .Count(r => r.IsRead)
            };
            return projection;
        }

        public List<RssStatistics> GetNewsThatWasReadMost(int i)
        {
            var list = this.database
                .RssEntriesToRead
                .Include(x => x.RssEntry)
                .Select(this.ToRssStatistics)
                .OrderByDescending(x => x.Count)
                .Take(i)
                .ToList();

            return list;
        }

        public List<EventRssUserInteraction> LoadAllEventsFromAndToDate(DateTime startDate, DateTime endDate)
        {
            var eventRssVieweds = this.database.EventsRssUserInteraction
                .Where(e => e.Created >= startDate)
                .Where(e => e.Created < endDate)
                .Include(e => e.RssEntry)
                .ToList();

            return eventRssVieweds;
        }

        private RssStatistics ToRssStatistics(RssEntryToRead model)
        {
            var projection = new RssStatistics
            {
                Id = model.RssEntry.Id,
                Name = model.RssEntry.Title,
                Preview = model.RssEntry.PreviewText,
            };

            return projection;
        }
    }
}