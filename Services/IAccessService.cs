using As.Api.Dtos;
using As.Api.Models;
using System.Threading.Tasks;

namespace As.Api.Services
{
    public interface IAccessService
    {
        Task<RecompensaDto?> VerificarOuGerarAcessoAsync(int userId);
    }
}
