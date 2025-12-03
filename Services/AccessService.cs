using As.Api.Data;
using As.Api.Dtos;
using As.Api.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using BCrypt.Net;

namespace As.Api.Services
{
    public class AccessService : IAccessService
    {
        private readonly AppDbContext _db;

        public AccessService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<RecompensaDto?> VerificarOuGerarAcessoAsync(int userId)
        {
            // 0) Se já tem registro de acesso, retorna os dados (mas não retorna senha já armazenada)
            var existente = await _db.UserAcessos
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.UserId == userId);

            if (existente != null)
            {
                // não retornamos a senha novamente por segurança; devolvemos apenas login e data
                return new RecompensaDto
                {
                    Login = existente.LoginGerado,
                    Senha = null, 
                    DataGeracao = existente.DataGeracao
                };
            }

            // 1) Buscar enquetes que contam para o acesso e trazer perguntas
            var enquetesObrigatorias = await _db.Enquetes
                .Where(e => e.ContaParaAcesso)  
                .Include(e => e.Perguntas)
                .ToListAsync();

            // Se não há enquetes obrigatórias definidas, não gera nada
            if (!enquetesObrigatorias.Any())
                return null;

            // 2) Para cada enquete obrigatória, checar se o usuário respondeu **todas** as perguntas desta enquete
            foreach (var enquete in enquetesObrigatorias)
            {
                var perguntasIds = enquete.Perguntas.Select(p => p.Id).ToList();

                // contar quantas perguntas distintas desta enquete o usuário respondeu
                var totalRespondidas = await _db.Respostas
                    .Where(r => r.UserId == userId && perguntasIds.Contains(r.PerguntaId))
                    .Select(r => r.PerguntaId)
                    .Distinct()
                    .CountAsync();

                if (totalRespondidas < perguntasIds.Count)
                {
                    // ainda não completou esta enquete — não gera acesso
                    return null;
                }
            }

            var login = $"usr{Guid.NewGuid().ToString("N").Substring(0, 6)}";
            var senhaPlain = Guid.NewGuid().ToString("N").Substring(0, 10);

            // hash da senha para armazenar
            var senhaHash = BCrypt.Net.BCrypt.HashPassword(senhaPlain);

            var acesso = new UserAcesso
            {
                UserId = userId,
                LoginGerado = login,
                SenhaHash = senhaHash
            };

            // utilizar transação para segurança
            using var trx = await _db.Database.BeginTransactionAsync();
            try
            {
                _db.UserAcessos.Add(acesso);
                await _db.SaveChangesAsync();
                await trx.CommitAsync();
            }
            catch
            {
                await trx.RollbackAsync();
                throw;
            }

            return new RecompensaDto
            {
                Login = login,
                Senha = senhaPlain,
                DataGeracao = acesso.DataGeracao
            };
        }
    }
}
