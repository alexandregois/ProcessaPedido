using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ProcessaPedido.Api.Models
{
    public class EntregaCreateDto
    {
        [Required]
        public string PedidoId { get; set; }
        [Required]
        public DestinatarioDto Destinatario { get; set; }
        [Required]
        [MinLength(1)]
        public List<ItemEntregaDto> Itens { get; set; }
    }

    public class DestinatarioDto
    {
        [Required]
        public string Nome { get; set; }
        [Required]
        public string Endereco { get; set; }
        [Required]
        public string Cep { get; set; }
    }

    public class ItemEntregaDto
    {
        [Required]
        public string Descricao { get; set; }
        [Range(1, int.MaxValue)]
        public int Quantidade { get; set; }
    }
} 