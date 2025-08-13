using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PropertyManagement.Core.Entities;
using System.ComponentModel.DataAnnotations;

namespace PropertyManagement.Web.Models
{
    public class NomenclaturesModel
    {
        public IList<ChargeType> ChargeTypes { get; set; }

        public IList<Service> Services { get; set; }

        [Required(ErrorMessage = "Веведете име на услугата!")]
        public string AddedService { get; set; }

        public string AddedServiceChargeType { get; set; }

        public bool AddedServiceIsProfit { get; set; }

        public bool AddedServiceIsCost { get; set; }

        public IList<DetectionModel> Detections { get; set; }

        public NomenclaturesModel()
        {
            this.AddedServiceIsProfit = true;
        }
    }

}