using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UserAccessManagement.Domain.Users;

namespace UserAccessManagement.Infrastructure.Persistence.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("Users");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.Lastname)
                .IsRequired()
                .HasMaxLength(100);


            builder.OwnsOne(u => u.Email, email =>
            {
                email.Property(e => e.Value)
                     .HasColumnName("Email")
                     .IsRequired();

                email.HasIndex(e => e.Value)
                    .IsUnique();
            });

            builder.Property(x => x.PasswordHash)
                .IsRequired();

            builder.Property(x => x.Address)
                  .HasMaxLength(250)
                  .IsRequired(false);

            builder.Property(x => x.IsActive)
                .IsRequired()
                .HasDefaultValue(true);


            builder.Property(x => x.CreatedAt)
                .IsRequired();

            builder.Property(x => x.UpdatedAt)
            .IsRequired(false);

            builder.Property(x => x.DeletedAt)
                .IsRequired(false);

            builder.HasQueryFilter(x => x.DeletedAt == null);

        }
    }
}
