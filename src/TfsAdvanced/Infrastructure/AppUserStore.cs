using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using TfsAdvanced.Data;

namespace TfsAdvanced.Infrastructure
{
    public class AppUserStore : IUserStore<User>
    {
        public void Dispose()
        {
         
        }

        public Task<string> GetUserIdAsync(User user, CancellationToken cancellationToken)
        {
            return Task.Run(() => { return user.Id; });
        }

        public Task<string> GetUserNameAsync(User user, CancellationToken cancellationToken)
        {
            return Task.Run(() => { return user.UserName; });
        }

        public Task SetUserNameAsync(User user, string userName, CancellationToken cancellationToken)
        {
            return Task.Run(() => { user.UserName = userName; });
        }

        public Task<string> GetNormalizedUserNameAsync(User user, CancellationToken cancellationToken)
        {
            return Task.Run(() => { return user.UserName; });
        }

        public Task SetNormalizedUserNameAsync(User user, string normalizedName, CancellationToken cancellationToken)
        {
            return Task.Run(() => { user.NormalizedUserName = normalizedName; });
        }

        public Task<IdentityResult> CreateAsync(User user, CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                return new IdentityResult();
            });
        }

        public Task<IdentityResult> UpdateAsync(User user, CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                return new IdentityResult();
            });
        }

        public Task<IdentityResult> DeleteAsync(User user, CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                return new IdentityResult();
            });
        }

        public Task<User> FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                return new User
                {
                    Id = userId
                };
            });
        }

        public Task<User> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                return new User
                {
                    NormalizedUserName = normalizedUserName
                };
            });
        }
    }
}
