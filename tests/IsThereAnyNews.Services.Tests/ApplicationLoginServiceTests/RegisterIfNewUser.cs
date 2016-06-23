using System.Collections.Generic;
using System.Security.Claims;
using AutoMapper;
using AutoMoq;
using IsThereAnyNews.DataAccess;
using IsThereAnyNews.EntityFramework.Models.Entities;
using IsThereAnyNews.Services.Implementation;
using IsThereAnyNews.SharedData;
using Moq;
using Xunit;

namespace IsThereAnyNews.Services.Tests.ApplicationLoginServiceTests
{
    public class RegisterIfNewUser
    {
        private readonly AutoMoqer moqer;
        private readonly ApplicationLoginService sut;
        private readonly Mock<ISessionProvider> mockSessionProvider;
        private readonly Mock<IRssChannelsSubscriptionsRepository> mockRssSubscriptionRepository;
        private readonly Mock<IMapper> mockMapper;
        private readonly Mock<IRssChannelsRepository> mockChannelRepository;
        private readonly Mock<IUserAuthentication> mockAuthentication;
        private readonly Mock<ISocialLoginRepository> mockSocialLoginRepository;
        private readonly Mock<IUserRepository> mockUserRepository;

        public RegisterIfNewUser()
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

        [Fact]
        public void T001_When_Checking_If_User_Is_Not_Registered_Then_Must_Read_Current_Logged_User_Social_Provider_Type()
        {
            // arrange
            this.mockSocialLoginRepository
                .Setup(s => s.FindSocialLogin(It.IsAny<string>(), It.IsAny<AuthenticationTypeProvider>()))
                .Returns(new SocialLogin());

            // act
            this.sut.RegisterIfNewUser();

            // assert
            this.mockAuthentication
                .Verify(v => v.GetCurrentUserLoginProvider(), Times.Once());
        }

        [Fact]
        public void T002_When_Checking_If_User_Is_Not_Registered_Then_Must_Read_Current_User_SocialId_From_Authentication()
        {
            // arrange
            this.mockSocialLoginRepository
                .Setup(s => s.FindSocialLogin(It.IsAny<string>(), It.IsAny<AuthenticationTypeProvider>()))
                .Returns(new SocialLogin());

            // act
            this.sut.RegisterIfNewUser();

            // assert
            this.mockAuthentication
                .Verify(v => v.GetCurrentUserSocialLoginId(), Times.Once());
        }

        [Fact]
        public void T003_When_User_Is_Already_Registered_In_Application_Then_Must_Not_Create_And_Save_New_Account_In_Repository()
        {
            // arrange
            this.mockSocialLoginRepository
                .Setup(s => s.FindSocialLogin(It.IsAny<string>(), It.IsAny<AuthenticationTypeProvider>()))
                .Returns(new SocialLogin());


            // act
            this.sut.RegisterIfNewUser();

            // assert
            this.mockSocialLoginRepository
                .Verify(v => v.SaveToDatabase(It.IsAny<SocialLogin>()),
                Times.Never());
        }


        [Fact]
        public void T003_When_User_Is_Not_Registered_In_Application_Then_Must_Create_And_Save_New_Account_In_Repository()
        {
            // arrange
            this.mockSocialLoginRepository
                .Setup(s => s.FindSocialLogin(It.IsAny<string>(), It.IsAny<AuthenticationTypeProvider>()))
                .Returns((SocialLogin)null);

            this.mockAuthentication
                .Setup(s => s.GetCurrentUser())
                .Returns(new ClaimsPrincipal(new List<ClaimsIdentity>
                {
                    new ClaimsIdentity(new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier,"test")
                    })
                }));

            this.mockUserRepository
                .Setup(s => s.CreateNewUser())
                .Returns(new User());


            // act
            this.sut.RegisterIfNewUser();

            // assert
            this.mockSocialLoginRepository
                .Verify(v => v.SaveToDatabase(It.IsAny<SocialLogin>()),
                Times.Once());
        }
    }
}