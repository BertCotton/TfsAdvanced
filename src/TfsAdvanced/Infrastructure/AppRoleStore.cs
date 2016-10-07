using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using TfsAdvanced.Data;

namespace TfsAdvanced.Infrastructure
{
    public class AppRoleStore : Microsoft.AspNetCore.Identity.IRoleStore<User>
    {
        public void Dispose()
        {
            
        }

        public Task<IdentityResult> CreateAsync(User role, CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                return new IdentityResult();
            });
        }

        public Task<IdentityResult> UpdateAsync(User role, CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                return new IdentityResult();
            });
        }

        public Task<IdentityResult> DeleteAsync(User role, CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                return new IdentityResult();
            }); throw new NotImplementedException();
        }

        public Task<string> GetRoleIdAsync(User role, CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                return role.Id;
            });
        }

        public Task<string> GetRoleNameAsync(User role, CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                return role.UserName;
            });
        }

        public Task SetRoleNameAsync(User role, string roleName, CancellationToken cancellationToken)
        {
            return Task.Run(() => {}); 
        }

        public Task<string> GetNormalizedRoleNameAsync(User role, CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                return role.NormalizedUserName;
            });
        }

        public Task SetNormalizedRoleNameAsync(User role, string normalizedName, CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                
            }); 
        }

        public Task<User> FindByIdAsync(string roleId, CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                return new User
                {
                    Id = roleId
                };
            });
        }

        public Task<User> FindByNameAsync(string normalizedRoleName, CancellationToken cancellationToken)
        {
            return Task.Run(() =>
            {
                return new User
                {
                    NormalizedUserName = normalizedRoleName
                };
            });
        }
    }
}
