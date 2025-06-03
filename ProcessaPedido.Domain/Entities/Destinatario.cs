using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessaPedido.Domain.Entities
{
    public class Destinatario
    {
        public string Nome { get; set; }
        public string Endereco { get; set; }
        public string Cep { get; set; }
    }
}
