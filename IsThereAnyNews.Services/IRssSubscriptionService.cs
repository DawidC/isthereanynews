using IsThereAnyNews.Dtos;
using IsThereAnyNews.SharedData;
using IsThereAnyNews.ViewModels;

namespace IsThereAnyNews.Services
{
    public interface IRssSubscriptionService
    {
        RssSubscriptionIndexViewModel LoadAllUnreadRssEntriesToReadForCurrentUserFromSubscription(StreamType streamType, long id, ShowReadEntries showReadEntries);
        void MarkAllRssReadForSubscription(MarkReadForSubscriptionDto id);
        void MarkEntryViewed(long rssToReadId);
        void UnsubscribeCurrentUserFromChannelId(long subscriptionId);
        void SubscribeCurrentUserToChannel(long channelId);
        void MarkRead(MarkReadDto dto);
        void SubscribeCurrentUserToChannel(AddChannelDto channelId);
    }
}