using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace PeopleApp.ViewModels
{
    public class AddEditDelete
    {
        public Guid PersonId { get; set; }
        [Display(Name = "Name"), MaxLength(100), Required(AllowEmptyStrings = false, ErrorMessage = "Name is required.")]
        public string Name { get; set; }
        [Display(Name = "Email"), MaxLength(100), Required(AllowEmptyStrings = false, ErrorMessage = "Email is required.")]
        [Remote(action: "ValidateEmail", controller: "Home")]
        public string Email { get; set; }
        [Display(Name = "Active")]
        public bool IsActive { get; set; }
        public DateTime LastUpdatedDttm { get; set; }
    }
}
