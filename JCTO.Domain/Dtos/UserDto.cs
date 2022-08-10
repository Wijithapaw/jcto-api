using JCTO.Domain.Dtos.Base;

namespace JCTO.Domain.Dtos
{
    public class UserDto : ConcurrencyHandledDto
    {
        public Guid? Id { get; set; }
        public string? Email { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
    }
}
