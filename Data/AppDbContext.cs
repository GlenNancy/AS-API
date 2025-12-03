using Microsoft.EntityFrameworkCore;
using As.Api.Models;
using System.Collections.Generic;

namespace As.Api.Data {
    public class AppDbContext : DbContext {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // CORREÇÃO: DbSet<User> em vez de ISet<User>
        public DbSet<User> Users { get; set; }
        public DbSet<UserAcesso> UserAcessos { get; set; }
        public DbSet<Enquete> Enquetes { get; set; }
        public DbSet<Pergunta_Enquete> Perguntas { get; set; }
        public DbSet<Resposta> Respostas { get; set; }
        public DbSet<Comment> Comments { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();
            // relacionamentos
            modelBuilder.Entity<Pergunta_Enquete>()
                        .HasOne(p => p.Enquete)
                        .WithMany(e => e.Perguntas)
                        .HasForeignKey(p => p.EnqueteId);

            modelBuilder.Entity<Resposta>()
                        .HasOne(r => r.Pergunta)
                        .WithMany(p => p.Respostas)
                        .HasForeignKey(r => r.PerguntaId);

            modelBuilder.Entity<Resposta>()
                        .HasOne(r => r.User)
                        .WithMany(u => u.Respostas)
                        .HasForeignKey(r => r.UserId);

            modelBuilder.Entity<Comment>()
                        .HasOne(c => c.Enquete)
                        .WithMany(e => e.Comments)
                        .HasForeignKey(c => c.EnqueteId);


            modelBuilder.Entity<Comment>()
                        .HasOne(c => c.User)
                        .WithMany(u => u.Comments)
                        .HasForeignKey(c => c.UserId);
            
            modelBuilder.Entity<UserAcesso>()
                        .HasOne(a => a.User)
                        .WithMany() // se você não quer coleção inversa
                        .HasForeignKey(a => a.UserId);

        }
    }
}
