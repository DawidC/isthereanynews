namespace IsThereAnyNews.Services.Implementation
{
    using System;

    public class SyndicationItemAdapter
    {
        public string Id { get; set; }
        public DateTimeOffset PublishDate { get; set; }
        public string Title { get; set; }
        public string Summary { get; set; }
        public string Url { get; set; }
    }
}