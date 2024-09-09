using Pangea.Entidades.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Pangea.Entidades.DTO
{
    public class CorplazoDTO : EntidadeBase
    {
        public string tipo { get; set; }
        public string sucursal { get; set; }
        public string rol { get; set; }
        public string motivo { get; set; }
        public int? dias { get; set; }
        public string numero_cliente { get; set; }
        public DateTime? fecha_solicitud { get; set; }
        public string oficina { get; set; }
        public string numero_ordem { get; set; }
        public string ind_notifica { get; set; }

        public CorplazoDTO()
        {
        }

        public CorplazoDTO(string rol = "", string motivoIngresso = "", string numeroCliente = null, int? dias = 0, string oficina = "", string tipo = "", DateTime? dataSolicitacao = null)
        {
            this.rol = rol;
            this.motivo = motivoIngresso;
            this.numero_cliente = numeroCliente;
            this.oficina = oficina;
            this.dias = dias;
            this.sucursal = sucursal;
            this.tipo = tipo;
            this.fecha_solicitud = dataSolicitacao;
        }
    }
}
