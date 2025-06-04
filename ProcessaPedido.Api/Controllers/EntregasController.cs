using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProcessaPedido.Application.Messages;
using ProcessaPedido.Domain.Entities;
using ProcessaPedido.Infrastructure.Persistence;
using System;
using Microsoft.AspNetCore.Authorization;
using ProcessaPedido.Api.Models;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using System.Linq;

namespace ProcessaPedido.Api.Controllers
{
    [ApiController]
    [Route("entregas")]
    [Authorize]
    public class EntregasController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly IPublishEndpoint _bus;

        public EntregasController(AppDbContext db, IPublishEndpoint bus)
        {
            _db = db;
            _bus = bus;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] EntregaCreateDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var entrega = new Entrega
            {
                PedidoId = dto.PedidoId,
                Destinatario = new Destinatario
                {
                    Nome = dto.Destinatario.Nome,
                    Endereco = dto.Destinatario.Endereco,
                    Cep = dto.Destinatario.Cep
                },
                Itens = dto.Itens.Select(i => new ItemEntrega
                {
                    Descricao = i.Descricao,
                    Quantidade = i.Quantidade
                }).ToList()
            };

            _db.Entregas.Add(entrega);
            await _db.SaveChangesAsync();
            await _bus.Publish(new EntregaMessage { EntregaId = entrega.Id });
            return Accepted(new { entrega.Id });
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(Guid id)
        {
            var entrega = await _db.Entregas.Include(e => e.Itens).FirstOrDefaultAsync(e => e.Id == id);
            if (entrega == null) return NotFound();
            return Ok(entrega);
        }
    }
}
