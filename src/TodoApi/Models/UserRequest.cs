using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TodoApi.Models
{
    public class UserRequest
    {
        public string UserName { get; set; }

        public string Password { get; set; }

        public string Mail { get; set; }
    }
}
