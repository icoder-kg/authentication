﻿using System.ComponentModel.DataAnnotations;

namespace Identity.ViewModels
{
    public class UserVm
    {
        [Required(ErrorMessage = "User Name required")]
        [Display(Name = "User Name")]
        public string UserName { get; set; }

        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "Email required")]
        [Display(Name = "Email")]
        [EmailAddress(ErrorMessage = "Email not true formatted")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password required")]
        [Display(Name = "Password")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}