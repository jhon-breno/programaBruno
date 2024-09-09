using Pangea.Entidades.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Pangea.Entidades
{
    public class Sucursal : EntidadeBase
    {
        public string sucursal { get; set; }
        public string descripcion { get; set; }
        public string endereco { get; set; }
        public string municipio_sede { get; set; }
        public string cep { get; set; }
        public string telefono { get; set; }
        public string tipo_sucursal { get; set; }
        public string fax { get; set; }
        public string nome_chefe { get; set; }
        public string email_chefe { get; set; }
        public string departamento { get; set; }
        public string regional { get; set; }
        public string gerencia { get; set; }
        public string gerencia_tec { get; set; }
        public string regional_tec { get; set; }
        public DateTime data_ativacao { get; set; }
        public string rol_ativacao { get; set; }
        public string dir_ip_ativacao { get; set; }
        public DateTime data_desativacao { get; set; }
        public string rol_desativacao { get; set; }
        public string dir_ip_desativacao { get; set; }
        public int carga_limite_trafo { get; set; }

        public string regionalDescricao { get; set; }
        public string areaDescricao { get; set; }
    }
}