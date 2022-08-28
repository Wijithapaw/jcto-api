using JCTO.Domain.Entities.Base;
using System.ComponentModel.DataAnnotations;

namespace JCTO.Domain.Entities
{
    public class User : BaseEntity
    {
        [Required]
        [MaxLength(200)]
        public string Email { get; set; }
        [Required]
        [MaxLength(50)]
        public string FirstName { get; set; }
        [Required]
        [MaxLength(50)]
        public string LastName { get; set; }
    }
}
