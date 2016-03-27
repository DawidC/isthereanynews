﻿using IsThereAnyNews.DataAccess;
using IsThereAnyNews.DataAccess.Implementation;
using IsThereAnyNews.ViewModels;

namespace IsThereAnyNews.Services.Implementation
{
    public class RssChannelService : IRssChannelService
    {
        private readonly IRssChannelRepository rssChannelRepository;
        private readonly ISessionProvider session;
        private readonly IUserAuthentication authentication;
        private readonly IRssChannelsSubscriptionsRepository rssSubscriptionRepository;

        public RssChannelService(
            IRssChannelRepository rssChannelRepository, 
            ISessionProvider session, 
            IUserAuthentication authentication, 
            IRssChannelsSubscriptionsRepository rssSubscriptionRepository)
        {
            this.rssChannelRepository = rssChannelRepository;
            this.session = session;
            this.authentication = authentication;
            this.rssSubscriptionRepository = rssSubscriptionRepository;
        }

        public RssChannelIndexViewModel GetViewModelFormChannelId(long id)
        {
            var rssChannel = this.rssChannelRepository.LoadRssChannel(id);
            

            var rssChannelIndexViewModel = new RssChannelIndexViewModel
            {
                Name = rssChannel.Title,
                Added = rssChannel.Created
            };

            if (this.authentication.CurrentUserIsAuthenticated())
            {
                var userId = this.session.GetCurrentUserId();
                var isUserSubscribedToRssChannel = this.rssSubscriptionRepository.IsUserSubscribedToRssChannel(userId, id);
                rssChannelIndexViewModel.IsAuthenticatedUser = true;
                rssChannelIndexViewModel.IsUserSubscribedToRssChannel = isUserSubscribedToRssChannel;
            }
            return rssChannelIndexViewModel;
        }
    }
}