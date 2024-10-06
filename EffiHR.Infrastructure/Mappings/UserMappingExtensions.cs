using EffiHR.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EffiHR.Infrastructure.Mappings
{
    public static class UserMappingExtensions
    {
        //public static User ToDomainUser(this IdentityUser identityUser)
        //{
        //    return new User
        //    {
        //        Id = Guid.Parse(identityUser.Id),
        //        Username = identityUser.UserName,
        //        Email = identityUser.Email
        //    };
        //}

        //public static IdentityUser ToIdentityUser(this User user)
        //{
        //    return new IdentityUser
        //    {
        //        Id = user.Id.ToString(),
        //        UserName = user.Username,
        //        Email = user.Email
        //    };
        //}
    }

}
