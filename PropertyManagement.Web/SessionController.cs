using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using NHibernate;
using System.Configuration;

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
        
        public bool ShowBGNEquivalent
        {
            get
            {
                bool showBgnEquivalent = false;
                Boolean.TryParse(ConfigurationManager.AppSettings["showBGNEquivalent"], out showBgnEquivalent);

                return showBgnEquivalent;
            }
        } 
    }
}