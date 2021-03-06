﻿namespace IsThereAnyNews.Services.Implementation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Xml;

    using IsThereAnyNews.DataAccess;
    using IsThereAnyNews.Dtos;
    using IsThereAnyNews.EntityFramework.Models.Entities;

    public class OpmlImporterService : IOpmlImporterService
    {
        private readonly IRssChannelsSubscriptionsRepository rssSubscriptionsRepository;
        private readonly ISessionProvider sessionProvider;
        private readonly IRssChannelsRepository rssChannelsRepository;
        private readonly IOpmlReader opmlHandler;

        public OpmlImporterService(
            IRssChannelsSubscriptionsRepository rssSubscriptionsRepository,
            ISessionProvider sessionProvider,
            IRssChannelsRepository rssChannelsRepository,
            IOpmlReader opmlHandler)
        {
            this.rssSubscriptionsRepository = rssSubscriptionsRepository;
            this.sessionProvider = sessionProvider;
            this.rssChannelsRepository = rssChannelsRepository;
            this.opmlHandler = opmlHandler;
        }


        public void AddToCurrentUserChannelList(List<RssChannel> importFromUpload)
        {
            var urlstoChannels = importFromUpload.Select(x => x.Url.ToLowerInvariant()).ToList();
            var listOfChannelsIds = this.rssChannelsRepository.GetIdByChannelUrl(urlstoChannels);

            var currentUserId = this.sessionProvider.GetCurrentUserId();
            var existringChannelIdSubscriptions = 
                this.rssSubscriptionsRepository.GetChannelIdSubscriptionsForUser(currentUserId);

            var rssChannelSubscriptions =
                listOfChannelsIds.Select(
                    id =>
                    new RssChannelSubscription(
                        id,
                        currentUserId,
                        importFromUpload.Single(x => x.Id == id).Title)).ToList();

            var subscriptionsToSave = rssChannelSubscriptions.Where(newSub => !existringChannelIdSubscriptions
                                                             .Contains(newSub.RssChannelId))
                                                             .ToList();
            this.rssSubscriptionsRepository.SaveToDatabase(subscriptionsToSave);
        }

        public void AddNewChannelsToGlobalSpace(List<RssChannel> channelList)
        {
            var loadUrlsForAllChannels = this.rssSubscriptionsRepository.LoadUrlsForAllChannels();
            var channelsNewToGlobalSpace =
                channelList.Where(channel => !loadUrlsForAllChannels.Contains(channel.Url.ToLowerInvariant())).ToList();
            this.rssChannelsRepository.SaveToDatabase(channelsNewToGlobalSpace);
        }

        public void Import(OpmlImporterIndexDto dto)
        {
            var toRssChannelList = this.ParseToRssChannelList(dto);
            this.AddNewChannelsToGlobalSpace(toRssChannelList);
            var existingChannels = toRssChannelList.Where(c => c.Id == 0).ToList();

            var existingChannelsIds =
                this.rssChannelsRepository
                    .GetIdByChannelUrl(existingChannels.Select(c => c.Url)
                    .ToList());

            var newChannelsIds = toRssChannelList.Where(c => c.Id != 0).Select(c => c.Id).ToList();
            existingChannelsIds.AddRange(newChannelsIds);

            var currentUserId = this.sessionProvider.GetCurrentUserId();

            var alreadySubscribedToChannelsId = this.rssSubscriptionsRepository.GetChannelIdSubscriptionsForUser(currentUserId);
            alreadySubscribedToChannelsId.ForEach(c => existingChannelsIds.Remove(c));

            existingChannelsIds.ForEach(
                c => this.rssSubscriptionsRepository.CreateNewSubscriptionForUserAndChannel(currentUserId, c));
        }

        public List<RssChannel> ParseToRssChannelList(OpmlImporterIndexDto dto)
        {
            var outlines = this.opmlHandler.GetOutlines(dto.ImportFile.InputStream);
            var ot = this.FilterOutInvalidOutlines(outlines);
            var urls = ot.Select(o =>
                      new RssChannel(
                          o.Attributes.GetNamedItem("xmlUrl").Value,
                          o.Attributes.GetNamedItem("title").Value))
                .ToList();
            return urls;
        }

        public List<XmlNode> FilterOutInvalidOutlines(IEnumerable<XmlNode> outlines)
        {
            var validoutlines = outlines.Where(o =>
                     o.Attributes.GetNamedItem("xmlUrl") != null
                  && o.Attributes.GetNamedItem("title") != null
                  && !String.IsNullOrWhiteSpace(o.Attributes.GetNamedItem("xmlUrl").Value)
                  && !String.IsNullOrWhiteSpace(o.Attributes.GetNamedItem("title").Value));

            return validoutlines.ToList();
        }
    }
}