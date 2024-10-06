using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace EffiHR.Application.Interfaces
{
    public interface ITokenService
    {
        JwtSecurityToken GenerateJwtToken(IEnumerable<Claim> claims);
    }

}
