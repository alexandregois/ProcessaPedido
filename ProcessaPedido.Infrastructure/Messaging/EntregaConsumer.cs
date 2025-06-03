using MassTransit;
using Microsoft.EntityFrameworkCore;
using ProcessaPedido.Application.Messages;
using ProcessaPedido.Domain.Enum;
using ProcessaPedido.Infrastructure.Persistence;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessaPedido.Infrastructure.Messaging
{
    public class EntregaConsumer : IConsumer<EntregaMessage>
    {
        private readonly AppDbContext _db;
        public EntregaConsumer(AppDbContext db) => _db = db;

        public async Task Consume(ConsumeContext<EntregaMessage> context)
        {
            var entrega = await _db.Entregas.Include(e => e.Itens)
                                            .FirstOrDefaultAsync(e => e.Id == context.Message.EntregaId);
            if (entrega == null) return;

            entrega.Status = StatusEntrega.SaiuParaEntrega;
            await _db.SaveChangesAsync();

            await Task.Delay(3000);
            entrega.Status = new Random().Next(2) == 0 ? StatusEntrega.Entregue : StatusEntrega.FalhaNaEntrega;
            await _db.SaveChangesAsync();
        }
    }
}
