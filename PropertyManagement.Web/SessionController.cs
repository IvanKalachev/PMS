using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NHibernate;

namespace PropertyManagement.Web
{
    public class SessionController : Controller
    {
        protected ISession _currentSession;
        public ISession CurrentSession
        {
            get
            {
                if (_currentSession == null)
                {
                    _currentSession = MvcApplication.CurrentSession;
                }

                return _currentSession;
            }
        } 
    }
}