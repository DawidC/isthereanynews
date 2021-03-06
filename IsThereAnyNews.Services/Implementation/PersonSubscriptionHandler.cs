using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using IsThereAnyNews.DataAccess;
using IsThereAnyNews.EntityFramework.Models.Entities;
using IsThereAnyNews.SharedData;
using IsThereAnyNews.ViewModels;

namespace IsThereAnyNews.Services.Implementation
{
    public class PersonSubscriptionHandler : ISubscriptionHandler
    {
        private readonly IRssEventRepository rssEventsRepository;
        private readonly ISessionProvider sessionProvider;
        private readonly IUsersSubscriptionRepository usersSubscriptionRepository;
        private readonly IMapper mapper;

        public PersonSubscriptionHandler(
            IRssEventRepository rssEventsRepository,
            ISessionProvider sessionProvider,
            IUsersSubscriptionRepository usersSubscriptionRepository,
            IMapper mapper)
        {
            this.rssEventsRepository = rssEventsRepository;
            this.sessionProvider = sessionProvider;
            this.usersSubscriptionRepository = usersSubscriptionRepository;
            this.mapper = mapper;
        }

        public RssSubscriptionIndexViewModel GetSubscriptionViewModel(long subscriptionId, ShowReadEntries showReadEntries)
        {
            return this.GetPersonSubscriptionIndexViewModel(subscriptionId, showReadEntries);
        }

        public void MarkRead(string displayedItems)
        {
            var ids =
                displayedItems.Split(new[] { ";" }, StringSplitOptions.RemoveEmptyEntries)
                    .Select(i => long.Parse(i))
                    .ToList();
            this.rssEventsRepository.MarkRead(ids);
        }

        public void AddEventViewed(long dtoId)
        {
            // do nothing for now
        }

        private RssSubscriptionIndexViewModel GetPersonSubscriptionIndexViewModel(long subscriptionId, ShowReadEntries showReadEntries)
        {
            var currentUserId = this.sessionProvider.GetCurrentUserId();

            if (!this.usersSubscriptionRepository.DoesUserOwnsSubscription(subscriptionId, currentUserId))
            {
                var ci = new ChannelInformationViewModel
                {
                    Title = "You are not subscribed to this user",
                    Created = DateTime.MaxValue
                };

                var rssSubscriptionIndexViewModel =
                    new RssSubscriptionIndexViewModel(subscriptionId,
                        ci,
                        new List<RssEntryToReadViewModel>(),
                        StreamType.Person);
                return rssSubscriptionIndexViewModel;
            }

            List<UserSubscriptionEntryToRead> loadAllUnreadEntriesFromSubscription;
            if (showReadEntries != ShowReadEntries.Show)
            {
                loadAllUnreadEntriesFromSubscription = this.usersSubscriptionRepository
                    .LoadAllUnreadEntriesFromSubscription(subscriptionId);
            }
            else
            {
                loadAllUnreadEntriesFromSubscription = this.usersSubscriptionRepository
                    .LoadAllEntriesFromSubscription(subscriptionId);
            }

            var channelInformation =
                this.usersSubscriptionRepository
                    .LoadChannelInformation(subscriptionId);

            var channelInformationViewModel = new ChannelInformationViewModel
            {
                Title = channelInformation.Title,
                Created = channelInformation.Created
            };

            var rssEntryToReadViewModels = this.mapper.Map<List<RssEntryToReadViewModel>>(loadAllUnreadEntriesFromSubscription);

            var viewModel = new RssSubscriptionIndexViewModel(
                subscriptionId,
                channelInformationViewModel,
                rssEntryToReadViewModels,
                StreamType.Person);

            return viewModel;
        }


    }
}