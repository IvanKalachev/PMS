using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PropertyManagement.Web.Helpers;

namespace PropertyManagement.Web.Models
{
    public class ErrorModel
    {
        public ErrorType Type { get; set; }

        public string Message { get; set; }

        public ErrorModel(ErrorType type, string message)
        {
            Type = type;
            Message = message;
        }
    }
}