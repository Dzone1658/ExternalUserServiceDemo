using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExternalUserService.Models;

namespace ExternalUserService.Clients
{
    public interface IUserApiClient
    {
       Task<User?> GetUserByIdAsync(int id);
        Task<List<User>> GetAllUsersAsync(); 
    }
}