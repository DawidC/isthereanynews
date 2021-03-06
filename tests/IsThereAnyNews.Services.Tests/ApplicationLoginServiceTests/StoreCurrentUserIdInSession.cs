namespace IsThereAnyNews.Services.Tests.ApplicationLoginServiceTests
{
    using AutoMapper;

    using AutoMoq;

    using IsThereAnyNews.DataAccess;
    using IsThereAnyNews.EntityFramework.Models.Entities;
    using IsThereAnyNews.Services.Implementation;
    using IsThereAnyNews.SharedData;

    using Moq;

    using NUnit.Framework;

    [TestFixture]
    public class StoreCurrentUserIdInSession
    {
        private AutoMoqer moqer;
        private ApplicationLoginService sut;
        private Mock<ISessionProvider> mockSessionProvider;
        private Mock<IRssChannelsSubscriptionsRepository> mockRssSubscriptionRepository;
        private Mock<IMapper> mockMapper;
        private Mock<IRssChannelsRepository> mockChannelRepository;
        private Mock<IUserAuthentication> mockAuthentication;
        private Mock<ISocialLoginRepository> mockSocialLoginRepository;
        private Mock<IUserRepository> mockUserRepository;

        [SetUp]
        public void Setup()
        {
            this.moqer = new AutoMoq.AutoMoqer();
            this.sut = this.moqer.Resolve<ApplicationLoginService>();
            this.mockSessionProvider = this.moqer.GetMock<ISessionProvider>();
            this.mockRssSubscriptionRepository = this.moqer.GetMock<IRssChannelsSubscriptionsRepository>();
            this.mockMapper = this.moqer.GetMock<IMapper>();
            this.mockChannelRepository = this.moqer.GetMock<IRssChannelsRepository>();
            this.mockAuthentication = this.moqer.GetMock<IUserAuthentication>();
            this.mockSocialLoginRepository = this.moqer.GetMock<ISocialLoginRepository>();
            this.mockUserRepository = this.moqer.GetMock<IUserRepository>();
        }

        [Test]
        public void T001_When_Storing_Then_Must_Read_Id_And_Provider_Type_From_Authentication()
        {
            // arrange      
            this.mockSocialLoginRepository
                .Setup(s => s.FindSocialLogin(It.IsAny<string>(), It.IsAny<AuthenticationTypeProvider>()))
                .Returns(new SocialLogin());

            // act            
            this.sut.StoreCurrentUserIdInSession();

            // assert
            this.mockAuthentication
                .Verify(v => v.GetCurrentUserLoginProvider(),
                Times.Once());

            this.mockAuthentication
                .Verify(v => v.GetCurrentUserSocialLoginId(),
                Times.Once());
        }


        [Test]
        public void T002_When_Storing_Then_Load_Social_Login_From_Database()
        {
            // arrange      
            this.mockSocialLoginRepository
                .Setup(s => s.FindSocialLogin(It.IsAny<string>(), It.IsAny<AuthenticationTypeProvider>()))
                .Returns(new SocialLogin());

            // act            
            this.sut.StoreCurrentUserIdInSession();

            // assert
            this.mockSocialLoginRepository
                .Verify(v => v.FindSocialLogin(It.IsAny<string>(), It.IsAny<AuthenticationTypeProvider>()),
                    Times.Once());
        }


        [Test]
        public void T002_When_Found_Social_Login_In_Repository_Then_Must_Store_It_In_Session()
        {
            // arrange      
            this.mockSocialLoginRepository
                .Setup(s => s.FindSocialLogin(It.IsAny<string>(), It.IsAny<AuthenticationTypeProvider>()))
                .Returns(new SocialLogin());

            // act            
            this.sut.StoreCurrentUserIdInSession();

            // assert
            this.mockSessionProvider
                .Verify(v => v.SetUserId(It.IsAny<long>()),
                    Times.Once());
        }
    }
}