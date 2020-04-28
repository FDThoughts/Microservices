using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace eShop.Services.Account.MVC.Infrastructure {
    public class IdentityDataContext : 
        IdentityDbContext<IdentityUser> {
        public IdentityDataContext(
            DbContextOptions<IdentityDataContext> options
        )
            : base(options) { }
    }
}