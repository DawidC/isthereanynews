using System.Collections.Generic;
using IsThereAnyNews.EntityFramework.Models;

namespace IsThereAnyNews.DataAccess
{
    public interface IRssChannelsSubscriptionsRepository
    {
        void SaveToDatabase(List<RssChannelSubscription> rssChannelSubscriptions);
        List<string> LoadUrlsForAllChannels();
        List<long> GetChannelIdSubscriptionsForUser(long currentUserId);
        List<RssChannelSubscription> LoadAllSubscriptionsForUser(long currentUserId);
        List<RssChannelSubscription> LoadAllSubscriptionsWithRssEntriesToReadForUser(long currentUserId);
        bool DoesUserOwnsSubscription(long subscriptionId, long currentUserId);
        void DeleteSubscriptionFromUser(long subscriptionId, long userId);
        bool IsUserSubscribedToRssChannel(long userId, long channelId);
    }
}