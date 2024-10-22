using Microsoft.EntityFrameworkCore;
using MeuProjetoAPI.Models;

namespace MeuProjetoAPI.Data;

public class AppDbContext: DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Produto> Produtos { get; set; }
}