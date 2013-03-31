using VersionCommander.Implementation;

namespace VersionCommander.UnitTests.TestingAssists
{
    public abstract class FakeFlatPropertyBag : FlatPropertyBag, IVersionControlledObject
    {
        private static int nextId;
        public int Id { get; private set; }

        protected FakeFlatPropertyBag()
        {
            Id = nextId++;
        }

        public abstract IVersionControlNode GetVersionControlNode();
        public abstract IVersionController<TSubject> GetVersionController<TSubject>();
        public TSubject GetNativeObject<TSubject>() where TSubject : class
        {
            return this as TSubject;
        }
    }
}