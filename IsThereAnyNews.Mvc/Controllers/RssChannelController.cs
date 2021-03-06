﻿namespace IsThereAnyNews.Mvc.Controllers
{
    using System.Net;
    using System.Web.Mvc;

    using IsThereAnyNews.Dtos;
    using IsThereAnyNews.Mvc.Infrastructure;
    using IsThereAnyNews.Services;
    using IsThereAnyNews.SharedData;

    [RoleAuthorize(Roles = new[] { ItanRole.User })]
    public class RssChannelController : BaseController
    {
        private readonly IRssChannelsService rssChannelsService;
        private readonly IRssSubscriptionService rssSubscriptionService;
        private readonly IUserSubscriptionService userSubscriptionServiceService;

        public RssChannelController(
            IUserAuthentication authentication,
            ILoginService loginService,
            ISessionProvider sessionProvider,
            IRssChannelsService rssChannelsService,
            IRssSubscriptionService rssSubscriptionService,
            IUserSubscriptionService userSubscriptionServiceService)
            : base(authentication, loginService, sessionProvider)
        {
            this.rssChannelsService = rssChannelsService;
            this.rssSubscriptionService = rssSubscriptionService;
            this.userSubscriptionServiceService = userSubscriptionServiceService;
        }

        [HttpGet]
        public ActionResult Index()
        {
            return this.View("Index");
        }

        [HttpGet]
        public ActionResult PublicChannels()
        {
            var viewmodel = this.rssChannelsService.LoadAllChannels();
            return this.Json(viewmodel, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult Public(long id)
        {
            var viewmodel = this.rssChannelsService.GetViewModelFormChannelId(id);
            return this.Json(viewmodel, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult My()
        {
            return this.View("My");
        }

        [HttpGet]
        public JsonResult MyChannelList()
        {
            var viewmodel = this.rssChannelsService.LoadAllChannelsOfCurrentUser();
            var listOfUsers = this.userSubscriptionServiceService.LoadAllObservableSubscription();
            viewmodel.Users = listOfUsers;
            return this.Json(viewmodel, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult Unsubscribe(long channelId)
        {
            this.rssSubscriptionService.UnsubscribeCurrentUserFromChannelId(channelId);
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

        [HttpPost]
        public ActionResult Subscribe(long channelId)
        {
            this.rssSubscriptionService.SubscribeCurrentUserToChannel(channelId);
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

        [HttpPost]
        public HttpStatusCodeResult MarkRssEntryViewed(long channelId)
        {
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

        [HttpPost]
        public HttpStatusCodeResult MarkAllReadForSubscription(MarkReadForSubscriptionDto model)
        {
            this.rssSubscriptionService.MarkAllRssReadForSubscription(model);
            return new HttpStatusCodeResult(HttpStatusCode.OK);
        }

        [HttpGet]
        public ActionResult Add()
        {
            return this.View("Add");
        }

        [HttpPost]
        public ActionResult Add(AddChannelDto dto)
        {
            if (this.ModelState.IsValid == false)
            {
                return this.View("Add", dto);
            }

            this.rssChannelsService.CreateNewChannelIfNotExists(dto);
            this.rssSubscriptionService.SubscribeCurrentUserToChannel(dto);
            return this.RedirectToAction("My");
        }
    }
}