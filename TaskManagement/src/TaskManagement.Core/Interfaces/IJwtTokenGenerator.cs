using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManagement.Core.Entities;

namespace TaskManagement.Core.Interfaces
{
    public interface IJwtTokenGenerator
    {
        string GenerateAccessToken(User User);
        string GenerateRefreshToken();


    }
}
