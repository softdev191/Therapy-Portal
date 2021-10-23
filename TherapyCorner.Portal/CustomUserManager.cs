using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace TherapyCorner.Portal
{
    public class CustomUserManager : UserManager<Models.ApplicationUser>
    {
        public CustomUserManager() : base(new CustomUserStore<Models.ApplicationUser>())
        {

        }

    }

    public class CustomUserStore<T> : IUserStore<T> where T : Models.ApplicationUser
    {
        public Task CreateAsync(T user)
        {
            throw new NotImplementedException();
        }

        public Task DeleteAsync(T user)
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {

        }

        public Task<T> FindByIdAsync(string userId)
        {
            throw new NotImplementedException();
        }

        public Task<T> FindByNameAsync(string userName)
        {
            throw new NotImplementedException();
        }

        public Task UpdateAsync(T user)
        {
            throw new NotImplementedException();
        }

        Task IUserStore<T, string>.CreateAsync(T user)
        {
            throw new NotImplementedException();
        }

        Task IUserStore<T, string>.DeleteAsync(T user)
        {
            throw new NotImplementedException();
        }

        Task<T> IUserStore<T, string>.FindByIdAsync(string userId)
        {
            throw new NotImplementedException();
        }

        Task<T> IUserStore<T, string>.FindByNameAsync(string userName)
        {
            throw new NotImplementedException();
        }

        Task IUserStore<T, string>.UpdateAsync(T user)
        {
            throw new NotImplementedException();
        }
    }
}