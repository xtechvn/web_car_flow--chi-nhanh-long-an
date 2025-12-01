using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Entities.Models
{
    public partial class User
    {
        public User()
        {
            UserRoles = new HashSet<UserRole>();
        }
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public int? UserMapId { get; set; }
        public string UserName { get; set; }
        public string FullName { get; set; }
        public string Password { get; set; }
        public string ResetPassword { get; set; }
        public string Phone { get; set; }
        public DateTime? BirthDay { get; set; }
        public int? Gender { get; set; }
        public string Email { get; set; }
        public string Avata { get; set; }
        public string Address { get; set; }
        public int Status { get; set; }
        public string Note { get; set; }
        public int? CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public int? ModifiedBy { get; set; }
        public DateTime? ModifiedOn { get; set; }
        public int? Manager { get; set; }
        public int? DepartmentId { get; set; }
        public int? Level { get; set; }
        public int? UserPositionId { get; set; }
        public int? CompanyType { get; set; }
        public string NickName { get; set; }
        public double? DebtLimit { get; set; }

        public virtual ICollection<UserRole> UserRoles { get; set; }
    }
}
