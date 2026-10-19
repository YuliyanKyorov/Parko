using System.ComponentModel.DataAnnotations;

namespace Parko.Models
{
    public class Client
    {
        public int Id { get; set; }
        [Required, StringLength(50)] 
        public string FirstName { get; set; } = "";
        [Required, StringLength(50)] 
        public string LastName { get; set; } = "";
        [Required, StringLength(120)] 
        public string Address { get; set; } = "";
        [Required, StringLength(60)] 
        public string City { get; set; } = "";
        [Required, StringLength(60)] 
        public string Country { get; set; } = "";
        [Required, RegularExpression(@"^\+?[0-9 ]{7,15}$", ErrorMessage = "Invalid phone number")]
        public string Phone { get; set; } = "";
        [Required, EmailAddress] 
        public string Email { get; set; } = "";
        public VipPoint? VipPoint { get; set; }
        public List<VipCarRecord> VipCarRecords { get; set; } = new();



    }
}
