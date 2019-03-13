using PhoneSkill.Common;
using PhoneSkill.Model;
using PhoneSkill.ServiceClients;

namespace PhoneSkillTest.TestDouble
{
    public class FakeServiceManager : IServiceManager
    {
        private IContactProvider contactProvider;

        public FakeServiceManager()
        {
            contactProvider = new StubContactProvider();
        }

        public IContactProvider GetContactProvider(string token, ContactSource source)
        {
            return contactProvider;
        }
    }
}
