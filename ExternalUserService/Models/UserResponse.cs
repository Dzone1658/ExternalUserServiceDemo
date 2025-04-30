using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExternalUserService.Models
{
    public class UserResponse
    {
        public int Page { get; set; }
        public int TotalPages { get; set; }
        public List<User> Data { get; set; }
    }
}