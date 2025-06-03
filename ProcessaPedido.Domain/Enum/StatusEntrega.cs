using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProcessaPedido.Domain.Enum
{
    public enum StatusEntrega
    {
        Pendente,
        SaiuParaEntrega,
        Entregue,
        FalhaNaEntrega
    }
}
