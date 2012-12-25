using System;
using VersionCommander.Implementation;
using New = VersionCommander.New;

namespace UnitTests
{

    public class UsageDemo
    {
        public class DomainModel : IVersionablePropertyBag
        {
            public virtual string Address { get; set; }
            public virtual string FirstName { get; set; }
            public virtual string LastName { get; set; }
            public virtual bool DoesAnybodysDomainActuallyLookLikeThis { get { return false; } }
        }

        public delegate void UndoHandler(object sender, EventArgs args);

        private event UndoHandler _userHitCtrlZ;
        public event UndoHandler UserHitCtrlZ
        {
            add { _userHitCtrlZ += value; }
            remove { _userHitCtrlZ -= value; }
        }

        private readonly DomainModel _domainModel;

        public UsageDemo()
        {
            _domainModel = New.Versioning<DomainModel>();
        }

        public void UsingVersionCommander(object sender, EventArgs args)
        {
            _domainModel.UndoLastAssignment();
        }

        public void ElsewhereInVancouver()
        {
            UserHitCtrlZ += UsingVersionCommander;
        }
    }
}

