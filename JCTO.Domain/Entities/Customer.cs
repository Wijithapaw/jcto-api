using JCTO.Domain.Entities.Base;
using System.ComponentModel.DataAnnotations;

namespace JCTO.Domain.Entities
{
    public class Customer : BaseEntity
    {
        [Required]
        [MaxLength(50)]
        public string Name { get; set; }
        public bool Inactive { get; set; }

        public ICollection<Entry> Entries { get; set; }
    }
}
