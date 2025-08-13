using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PropertyManagement.Core.Entities
{
    public class UserRole : Entity
    {
        public UserRole()
        {
        }

        public virtual string RoleName { get; set; }

        public virtual string Description { get; set; }

        public virtual IList<Right> Rights { get; set; }

        public virtual IList<User> Users { get; set; }

        public virtual void AddUser(User user)
        {
            if (Users == null)
            {
                Users = new List<User>();
            }

            Users.Add(user);
        }

        public virtual void AddRight(Right right)
        {
            if (Rights == null)
            {
                Rights = new List<Right>();
            }

            Rights.Add(right);
        }
    }
}
