using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace TodoApi.Models
{
    // IdentityUser already has Id (string), Username, Email and PasswordHash
    public class User : IdentityUser
    {
    }
}
