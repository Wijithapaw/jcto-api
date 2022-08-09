using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JCTO.Domain.Dtos
{
    public interface IUserContext
    {
        public string? UserId { get; set; }
        public string? Username { get; set; }
    }

    public class UserContext : IUserContext
    {
        public string? UserId { get; set; }
        public string? Username { get; set; }
    }
}
