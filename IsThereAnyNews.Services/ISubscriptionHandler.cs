using IsThereAnyNews.SharedData;
using IsThereAnyNews.ViewModels;

namespace IsThereAnyNews.Services
{
    public interface ISubscriptionHandler
    {
        RssSubscriptionIndexViewModel GetSubscriptionViewModel(long subscriptionId, ShowReadEntries showReadEntries);
        void MarkRead(string displayedItems);
    }
}