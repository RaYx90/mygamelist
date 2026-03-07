using GameList.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GameList.Infrastructure.Persistence.Configurations;

internal sealed class GroupConfiguration : IEntityTypeConfiguration<GroupEntity>
{
    public void Configure(EntityTypeBuilder<GroupEntity> builder)
    {
        builder.ToTable("Groups");
        builder.HasKey(g => g.Id);
        builder.Property(g => g.Id).ValueGeneratedOnAdd();
        builder.Property(g => g.Name).IsRequired().HasMaxLength(100);
        builder.Property(g => g.InviteCode).IsRequired().HasMaxLength(20);
        builder.HasIndex(g => g.InviteCode).IsUnique();
        builder.Property(g => g.CreatedAt).IsRequired();
    }
}
