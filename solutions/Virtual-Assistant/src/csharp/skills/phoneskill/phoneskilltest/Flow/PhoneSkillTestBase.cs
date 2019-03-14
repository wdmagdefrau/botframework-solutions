using Autofac;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Adapters;
using Microsoft.Bot.Configuration;
using Microsoft.Bot.Schema;
using Microsoft.Bot.Builder.Solutions.Authentication;
using Microsoft.Bot.Builder.Solutions.Proactive;
using Microsoft.Bot.Builder.Solutions.Responses;
using Microsoft.Bot.Builder.Solutions.Skills;
using Microsoft.Bot.Builder.Solutions.TaskExtensions;
using Microsoft.Bot.Builder.Solutions.Telemetry;
using Microsoft.Bot.Builder.Solutions.Testing;
using Microsoft.Bot.Builder.Solutions.Testing.Mocks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PhoneSkill.Common;
using PhoneSkill.Dialogs.Main.Resources;
using PhoneSkill.Dialogs.OutgoingCall.Resources;
using PhoneSkill.Dialogs.Shared.Resources;
using PhoneSkill.ServiceClients;
using PhoneSkillTest.Flow.LuisTestUtils;
using PhoneSkillTest.TestDouble;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Threading;

namespace PhoneSkillTest.Flow
{
    public class PhoneSkillTestBase : BotTestBase
    {
        public SkillConfigurationBase Services { get; set; }

        public EndpointService EndpointService { get; set; }

        public ConversationState ConversationState { get; set; }

        public UserState UserState { get; set; }

        public ProactiveState ProactiveState { get; set; }

        public IBotTelemetryClient TelemetryClient { get; set; }

        public IBackgroundTaskQueue BackgroundTaskQueue { get; set; }

        public IServiceManager ServiceManager { get; set; }

        [TestInitialize]
        public override void Initialize()
        {
            var builder = new ContainerBuilder();

            ConversationState = new ConversationState(new MemoryStorage());
            UserState = new UserState(new MemoryStorage());
            TelemetryClient = new NullBotTelemetryClient();
            Services = new MockSkillConfiguration();

            Services.LocaleConfigurations.Add("en", new LocaleConfiguration()
            {
                Locale = "en-us",
                LuisServices = new Dictionary<string, ITelemetryLuisRecognizer>
                {
                    { "general", GeneralTestUtil.CreateRecognizer() },
                    { "phone", PhoneSkillTestUtil.CreateRecognizer() }
                }
            });

            Services.AuthenticationConnections.Add("Azure Active Directory v2", "Azure Active Directory v2");

            var fakeServiceManager = new FakeServiceManager();
            builder.RegisterInstance<IServiceManager>(fakeServiceManager);
            ServiceManager = fakeServiceManager;

            builder.RegisterInstance(new BotStateSet(UserState, ConversationState));

            Container = builder.Build();

            ResponseManager = new ResponseManager(
                Services.LocaleConfigurations.Keys.ToArray(),
                new MainResponses(),
                new SharedResponses(),
                new OutgoingCallResponses());
        }

        public TestFlow GetTestFlow()
        {
            var adapter = new TestAdapter()
                .Use(new AutoSaveStateMiddleware(ConversationState));

            var testFlow = new TestFlow(adapter, async (context, token) =>
            {
                var bot = BuildBot() as PhoneSkill.PhoneSkill;
                await bot.OnTurnAsync(context, CancellationToken.None);
            });

            return testFlow;
        }

        public override IBot BuildBot()
        {
            return new PhoneSkill.PhoneSkill(Services, EndpointService, ConversationState, UserState, ProactiveState, TelemetryClient, BackgroundTaskQueue, true, ResponseManager, ServiceManager);
        }

        protected Action<IActivity> ShowAuth()
        {
            return activity =>
            {
                var eventActivity = activity.AsEventActivity();
                Assert.AreEqual(eventActivity.Name, "tokens/request");
            };
        }

        protected Activity GetAuthResponse()
        {
            var providerTokenResponse = new ProviderTokenResponse
            {
                TokenResponse = new TokenResponse(token: "test"),
                AuthenticationProvider = OAuthProvider.AzureAD
            };
            return new Activity(ActivityTypes.Event, name: "tokens/response", value: providerTokenResponse);
        }

        protected Action<IActivity> Message(string templateId, StringDictionary tokens = null)
        {
            return activity =>
            {
                Assert.AreEqual("message", activity.Type);
                var messageActivity = activity.AsMessageActivity();

                // Work around a bug in ParseReplies.
                if (tokens == null)
                {
                    tokens = new StringDictionary();
                }

                var expectedTexts = ParseReplies(templateId, tokens);
                var actualText = messageActivity.Text;
                CollectionAssert.Contains(expectedTexts, actualText, $"Expected one of: {expectedTexts.ToPrettyString()}\nActual: {actualText}\n");
            };
        }
    }
}
