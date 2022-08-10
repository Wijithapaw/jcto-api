using JCTO.Domain.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JCTO.Tests.Helpers
{
    internal static class DtoHelper
    {
        internal static UserDto CreateUserDto(string firstName, string lastName, string email, Guid? concurrencyKey = null)
        {
            return new UserDto
            {
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                ConcurrencyKey = concurrencyKey
            };
        }
    }
}
