using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PropertyManagement.Core.Entities
{
    public class User : Entity
    {
        public User()
        {
        }

        public virtual string UserName { get; set; }

        public virtual string Email { get; set; }

        public virtual string Password { get; set; }

        public virtual bool IsApproved { get; set; }

        public virtual bool IsDeleted { get; set; }

        public virtual DateTime CreationDate { get; set; }

        public virtual string ProviderUserKey { get; set; }

        // public virtual UserType UserType { get; set; }

        protected IList<UserRole> _roles;
        public virtual IList<UserRole> Roles
        {
            get
            {
                if (_roles == null)
                {
                    _roles = new List<UserRole>();
                }

                return _roles;
            }
            set
            {
                _roles = value;
            }
        }
    }
}
