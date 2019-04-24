using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;

namespace FacialRecordIdentification.Models
{
    /// <summary>
    /// View model used to register new Patient
    /// </summary>
    public class RegisterPatientViewModel
    {
        [Required]
        public string Title { get; set; }
        
        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        public string Gender { get; set; }

        public string DateOfBirth { get; set; }

        public string NIC { get; set; }

        [Required]
        public string CivilStatus { get; set; }

        [Required]
        public HttpPostedFileBase WebCam { get; set; }
    }

    public class CreatePatientViewModel
    {
        [Required]
        public string Title { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        public string Gender { get; set; }

        public string DateOfBirth { get; set; }

        public string NIC { get; set; }

        [Required]
        public string CivilStatus { get; set; }
    }
}