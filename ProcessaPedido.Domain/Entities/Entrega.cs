using ProcessaPedido.Domain.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessaPedido.Domain.Entities
{
    public class Entrega
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string PedidoId { get; set; }
        public Destinatario Destinatario { get; set; }
        public List<ItemEntrega> Itens { get; set; } = new();
        public StatusEntrega Status { get; set; } = StatusEntrega.Pendente;
        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;
    }

   
}
