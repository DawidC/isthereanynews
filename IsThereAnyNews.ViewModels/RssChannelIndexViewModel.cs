﻿using System;

namespace IsThereAnyNews.ViewModels
{
    public class RssChannelIndexViewModel
    {
        public string Name { get; set; }
        public DateTime Added { get; set; }
        public bool IsAuthenticatedUser { get; set; }
        public bool IsUserSubscribedToRssChannel { get; set; }
    }
}