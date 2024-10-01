using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PRN231.ExploreNow.BusinessObject.Entities;

namespace PRN231.ExploreNow.Repositories.Context;

public abstract class BaseDbContext : IdentityDbContext<ApplicationUser, IdentityRole, string>
{
    protected BaseDbContext(DbContextOptions options)
        : base(options)
    {
    }

    public override int SaveChanges()
    {
        return SaveChanges();
    }
}