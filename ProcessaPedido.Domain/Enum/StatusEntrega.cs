using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessaPedido.Domain.Enum
{
    public enum StatusEntrega
    {
        Pendente = 0,
        SaiuParaEntrega = 1,
        Entregue = 2,
        FalhaNaEntrega = 3
    }
}
