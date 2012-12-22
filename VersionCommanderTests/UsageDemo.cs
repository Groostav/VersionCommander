using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VersionCommander.Extensions;

namespace VersionCommander.Tests
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

        public event UndoHandler UsedHitCtrlZ;

        private DomainModel _domainModel;

        public UsageDemo()
        {
            _domainModel = New.Versioning<DomainModel>();
        }

        public void UsingVersionCommander(object sender, EventArgs args)
        {
            _domainModel.UndoLastAssignment();
        }
    }
}

