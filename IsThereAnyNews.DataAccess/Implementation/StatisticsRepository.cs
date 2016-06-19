using System;
using System.Collections.Generic;
using IsThereAnyNews.EntityFramework;
using System.Data.Entity;
using System.Linq;
using IsThereAnyNews.EntityFramework.Models;
using IsThereAnyNews.EntityFramework.Models.Entities;
using IsThereAnyNews.EntityFramework.Models.Events;

namespace IsThereAnyNews.DataAccess.Implementation
{
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
                .Select(ToChannelStatistics)
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
                .Select(ProjectToUserStatistics)
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
                    .Where(r => r.IsRead)
                    .Count()
            };
            return projection;
        }

        public List<RssStatistics> GetNewsThatWasReadMost(int i)
        {
            throw new NotImplementedException("GetNewsThatWasReadMost - db");
            var list = this.database
                .RssEntriesToRead
                .Include(x => x.RssEntry)
                .Select(ToRssStatistics)
                .OrderByDescending(x => x.Count)
                .Take(i)
                .ToList();

            return list;
        }

        public List<EventRssViewed> LoadAllEventsFromAndToDate(DateTime startDate, DateTime endDate)
        {
            var eventRssVieweds = this.database.EventsRssViewed
                .Where(e => e.Created >= startDate)
                .Where(e => e.Created < endDate)
                .Include(e=>e.RssEntry)
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