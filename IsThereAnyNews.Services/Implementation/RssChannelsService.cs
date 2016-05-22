﻿using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using IsThereAnyNews.DataAccess;
using IsThereAnyNews.Dtos;
using IsThereAnyNews.EntityFramework.Models.Entities;
using IsThereAnyNews.ViewModels;

namespace IsThereAnyNews.Services.Implementation
{
    public class RssChannelsService : IRssChannelsService
    {
        private readonly IRssChannelsRepository channelsRepository;
        private readonly ISessionProvider sessionProvider;
        private readonly IRssChannelsSubscriptionsRepository channelsSubscriptionRepository;
        private readonly IRssEntriesToReadRepository rssEntriesToReadRepository;
        private readonly IUserAuthentication authentication;
        private readonly IRssChannelsSubscriptionsRepository rssSubscriptionRepository;
        private readonly ISessionProvider session;
        private readonly IMapper mapping;

        public RssChannelsService(
            IRssChannelsRepository channelsRepository,
            ISessionProvider sessionProvider,
            IRssChannelsSubscriptionsRepository channelsSubscriptionRepository,
            IRssEntriesToReadRepository rssEntriesToReadRepository,
            IUserAuthentication authentication,
            IRssChannelsSubscriptionsRepository rssSubscriptionRepository,
            ISessionProvider session,
            IMapper mapping)
        {
            this.channelsRepository = channelsRepository;
            this.sessionProvider = sessionProvider;
            this.channelsSubscriptionRepository = channelsSubscriptionRepository;
            this.rssEntriesToReadRepository = rssEntriesToReadRepository;
            this.authentication = authentication;
            this.rssSubscriptionRepository = rssSubscriptionRepository;
            this.session = session;
            this.mapping = mapping;
        }

        public RssChannelsIndexViewModel LoadAllChannels()
        {
            var loadAllChannels = this.channelsRepository.LoadAllChannelsWithStatistics()
                .Select(c => new RssChannelWithStatisticsViewModel(
                    c.Id,
                    c.Created,
                    c.RssEntriesCount,
                    c.SubscriptionsCount,
                    c.Title,
                    c.Updated))
                .ToList();
            var viewmodel = new RssChannelsIndexViewModel(loadAllChannels);
            return viewmodel;
        }

        public RssChannelsMyViewModel LoadAllChannelsOfCurrentUser()
        {
            var currentUserId = this.sessionProvider.GetCurrentUserId();
            var rssSubscriptions = this.channelsSubscriptionRepository.LoadAllSubscriptionsForUser(currentUserId);
            this.rssEntriesToReadRepository.CopyRssThatWerePublishedAfterLastReadTimeToUser(currentUserId, rssSubscriptions);
            var viewmodel = this.mapping.Map<RssChannelsMyViewModel>(rssSubscriptions);
            return viewmodel;
        }

        public RssChannelIndexViewModel GetViewModelFormChannelId(long id)
        {
            var rssChannel = this.channelsRepository.LoadRssChannel(id);


            var rssEntryViewModels = this.mapping.Map<List<RssEntryViewModel>>(rssChannel.RssEntries);

            var rssChannelIndexViewModel = new RssChannelIndexViewModel
            {
                Name = rssChannel.Title,
                Added = rssChannel.Created,
                ChannelId = rssChannel.Id,
                Entries = rssEntryViewModels
            };

            if (this.authentication.CurrentUserIsAuthenticated())
            {
                var userId = this.session.GetCurrentUserId();
                var subscriptionInfo = this.rssSubscriptionRepository.FindSubscriptionIdOfUserAndOfChannel(userId, id);
                rssChannelIndexViewModel.IsAuthenticatedUser = true;
                rssChannelIndexViewModel.SubscriptionInfo = new UserRssSubscriptionInfoViewModel(subscriptionInfo);
            }
            return rssChannelIndexViewModel;
        }

        public void CreateNewChannelIfNotExists(AddChannelDto dto)
        {
            var idByChannelUrl = this.channelsRepository.GetIdByChannelUrl(new List<string> { dto.RssChannelLink });
            if (!idByChannelUrl.Any())
            {
                var rssChannel = new RssChannel();
                rssChannel.Title = dto.RssChannelName;
                rssChannel.Url = dto.RssChannelLink;
                this.channelsRepository.SaveToDatabase(new List<RssChannel> { rssChannel });
            }
        }
    }
}