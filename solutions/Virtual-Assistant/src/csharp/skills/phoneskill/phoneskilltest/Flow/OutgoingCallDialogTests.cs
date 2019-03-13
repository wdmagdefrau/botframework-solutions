using Microsoft.VisualStudio.TestTools.UnitTesting;
using PhoneSkill.Dialogs.OutgoingCall.Resources;
using PhoneSkillTest.Flow.Utterances;
using System.Collections.Specialized;
using System.Threading.Tasks;

namespace PhoneSkillTest.Flow
{
    [TestClass]
    public class OutgoingCallDialogTests : PhoneSkillTestBase
    {
        [TestMethod]
        public async Task Test_OutgoingCall_PhoneNumber()
        {
            await GetTestFlow()
               .Send(OutgoingCallDialogUtterances.OutgoingCallPhoneNumber)
               .AssertReply(ShowAuth())
               .Send(GetAuthResponse())
               .AssertReply(Message(OutgoingCallResponses.ExecuteCall, new StringDictionary() {
                   { "contactOrPhoneNumber", "0118 999 88199 9119 725 3" },
               }))
               .StartTestAsync();
        }

        [TestMethod]
        public async Task Test_OutgoingCall_RecipientPromptPhoneNumber()
        {
            await GetTestFlow()
               .Send(OutgoingCallDialogUtterances.OutgoingCallNoEntities)
               .AssertReply(ShowAuth())
               .Send(GetAuthResponse())
               .AssertReply(Message(OutgoingCallResponses.RecipientPrompt))
               .Send(OutgoingCallDialogUtterances.RecipientPhoneNumber)
               .AssertReply(Message(OutgoingCallResponses.ExecuteCall, new StringDictionary() {
                   { "contactOrPhoneNumber", "0118 999 88199 9119 725 3" },
               }))
               .StartTestAsync();
        }

        [TestMethod]
        public async Task Test_OutgoingCall_ContactName()
        {
            await GetTestFlow()
               .Send(OutgoingCallDialogUtterances.OutgoingCallContactName)
               .AssertReply(ShowAuth())
               .Send(GetAuthResponse())
               .AssertReply(Message(OutgoingCallResponses.ExecuteCall, new StringDictionary() {
                   { "contactOrPhoneNumber", "Bob Botter" },
               }))
               .StartTestAsync();
        }

        [TestMethod]
        public async Task Test_OutgoingCall_RecipientPromptContactName()
        {
            await GetTestFlow()
               .Send(OutgoingCallDialogUtterances.OutgoingCallNoEntities)
               .AssertReply(ShowAuth())
               .Send(GetAuthResponse())
               .AssertReply(Message(OutgoingCallResponses.RecipientPrompt))
               .Send(OutgoingCallDialogUtterances.RecipientContactName)
               .AssertReply(Message(OutgoingCallResponses.ExecuteCall, new StringDictionary() {
                   { "contactOrPhoneNumber", "Bob Botter" },
               }))
               .StartTestAsync();
        }
    }
}
