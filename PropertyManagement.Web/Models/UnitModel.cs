using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using PropertyManagement.Core.Entities;

namespace PropertyManagement.Web.Models
{
    public class UnitModel
    {
        public Int64 Id { get; set; }

        [Display(Name="Номер на апартамент")]
        public virtual string Number { get; set; }

        [Required(ErrorMessage="*")]
        [Display(Name="Семейство")]
        public string FamilyName { get; set; }

        [Required(ErrorMessage = "*")]
        [Display(Name = "Брой членове")]
        [Range(0, 10, ErrorMessage = "Невалидна стойност!")]
        public int MembersCount { get; set; }

        [Required(ErrorMessage = "*")]
        [Display(Name = "Процент идеални части")]
        [Range(0, 100, ErrorMessage = "Невалидна стойност!")]
        [DisplayFormat(DataFormatString = "{0:F3}", ApplyFormatInEditMode = true)]
        public decimal PercentIdealParts { get; set; }

        [Display(Name="Етаж")]
        [Range(1, 30, ErrorMessage="Невалидна стойност!")]
        public int? Floor { get; set; }

        public IList<Service> NoCharegeServices { get; set; }

        public decimal Balance { get; set; }

        public bool Deactivated { get; set; }

    }
}