using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace apa.BOL
{
    //----------------------------------------------------------------------------------------------------------
    public class ContactUsViewModel
    {
        [Display(Name = "Name")]
        [Required(ErrorMessage = "Please Enter Name")]
        [RegularExpression(@"^([a-zA-Z0-9 \.\&\'\-]+)$", ErrorMessage = "Special characters are not allowed in the Name.")]
        [StringLength(100, ErrorMessage = "Name cannot be longer than 100 characters.")]
        public string Name { get; set; }

        [Display(Name = "Company")]
        [Required(ErrorMessage = "Please Enter Company")]
        [StringLength(50, ErrorMessage = "Company name cannot be longer than 50 characters.")]
        public string Company { get; set; }

        [Display(Name = "Email")]
        [Required(ErrorMessage = "Please Enter Email")]
        [StringLength(75, ErrorMessage = "Email cannot be longer than 75 characters.")]
        [RegularExpression(@"^([\w-]+(?:\.[\w-]+)*)@((?:[\w-]+\.)*\w[\w-]{0,66})\.([a-z]{2,6}(?:\.[a-z]{2})?)$", ErrorMessage = "Please enter an email in the format: email@host.com")]
        public string Email { get; set; }

        [Display(Name = "Phone")]
        [StringLength(20, ErrorMessage = "Your phone number must be between 10 and 20 characters.", MinimumLength = 10)]
        public string Phone { get; set; }

        [Display(Name = "Subject")]
        public IEnumerable<SelectListItem> SubjectList
        {
            get
            {
                return new[] {
                 new SelectListItem{ Value="-ATTN: APA",Text="-ATTN: APA"},
                 new SelectListItem{ Value="Cloud Development",Text="Cloud Development"},
                 new SelectListItem{ Value="General Inquiry",Text="General Inquiry"},
                 new SelectListItem{ Value="IT Consulting",Text="IT Consulting"},
                 new SelectListItem{ Value="New Business Inquiry",Text="New Business Inquiry"},
                 new SelectListItem{ Value="Partnership Inquiry",Text="Partnership Inquiry"},
                 new SelectListItem{ Value="Programming",Text="Programming"},
                 new SelectListItem{ Value="Other Services",Text="Other Services"}
                };
            }
        }
        public string SelectedItem { get; set; }

        [Display(Name = "Subject")]
        [StringLength(100)]
        public string SubjectList1 { get; set; }
        public string SelectedItem1 { get; set; }

        [Display(Name = "Subject")]
        public List<SelectListItem> SubjectList2 { get; set; }
        public string SelectedItem2 { get; set; }

        public List<SelectListItem> getSubjectList()
        {
            List<SelectListItem> sList = new List<SelectListItem>();
            var data = new[]{
                 new SelectListItem{ Value="-ATTN: APA",Text="-ATTN: APA"},
                 new SelectListItem{ Value="Cloud Development",Text="Cloud Development"},
                 new SelectListItem{ Value="General Inquiry",Text="General Inquiry"},
                 new SelectListItem{ Value="IT Consulting",Text="IT Consulting"},
                 new SelectListItem{ Value="New Business Inquiry",Text="New Business Inquiry"},
                 new SelectListItem{ Value="Partnership Inquiry",Text="Partnership Inquiry"},
                 new SelectListItem{ Value="Programming",Text="Programming"},
                 new SelectListItem{ Value="Other Services",Text="Other Services"}
             };
            sList = data.ToList();
            return sList;
        }

        [Display(Name = "Inquiry")]
        [StringLength(250, ErrorMessage = "Inquiry can not be longer than 250 characters.")]
        public string Inquiry { get; set; }

        public string CurrentDate { get; set; }

        public ContactUsViewModel()
        {
            //Name = "Azurestream";
            //Company = "Azurestream";
            //Email = "test@test.com";
            //Phone = "212.777.1111";
            //SelectedItem = "Programming";
            //SelectedItem1 = "New Business Inquiry";
            //SelectedItem2 = "IT Consulting";
            //Inquiry = "Inquiry";
            //SubjectList1 = "SubjectBox";
            CurrentDate = System.DateTime.Now.ToString("F");
        }
    }
}