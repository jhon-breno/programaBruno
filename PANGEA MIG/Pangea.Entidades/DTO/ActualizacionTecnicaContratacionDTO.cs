using Entidades.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Entidades;
using Pangea.Entidades.Enumeracao;

namespace Pangea.Entidades.DTO
{
    [Serializable]
    public class ActualizacionTecnicaContratacionDTO
    {

        //public string marca_medidor { get; set; }
        //public string modelo_medidor { get; set; }
        //public int numero_medidor { get; set; }
        //public string propriedade_medidor { get; set; }
        //public string numero_alimentador { get; set; }


        public int numero_cliente { get; set; }
        public string numero_caso { get; set; }
        public string tipo_servico { get; set; }
                          
        public MedidDTO Medid { get; set; }
        public TecniDTO Tecni { get; set; }
        public ClienteDTO Cliente { get; set; }
        public OrdemServicoDTO Ordem { get; set; }
        public VisitaOrdemDTO VisitaOrdem { get; set; }
        public RetornoServico RetornoServico { get; set; }
        public Empresa empresa { get; set; }

        public string codigo_resultado { get; set; }
        public string descricao_resultado { get; set; }
        public string pais { get; set; }
        public string tipo_medida_condicion_instalacion { get; set; }


    }

}
