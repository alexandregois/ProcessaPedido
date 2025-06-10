using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ProcessaPedido.Api.Models
{
    public class EntregaCreateDto
    {
        [Required]
        public required string PedidoId { get; set; }
        [Required]
        public required DestinatarioDto Destinatario { get; set; }
        [Required]
        [MinLength(1)]
        public required List<ItemEntregaDto> Itens { get; set; }
    }

    public class DestinatarioDto
    {
        [Required]
        public required string Nome { get; set; }
        [Required]
        public required string Endereco { get; set; }
        [Required]
        public required string Cep { get; set; }
    }

    public class ItemEntregaDto
    {
        [Required]
        public required string Descricao { get; set; }
        [Range(1, int.MaxValue)]
        public int Quantidade { get; set; }
    }
} 