using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Pangea.Entidades.Base;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace Pangea.Entidades.DTO
{
    [DataContract]
    public class AgendamentoDTO
    {
        public AgendamentoDTO()
        {
        }

        public AgendamentoDTO(OrdemServicoDTO ordemServico)
        {
            this.ordemServico = ordemServico;
            this.CodEmpresa = ordemServico.CodEmpresa;
            this.TipoOrdem = ordemServico.tipo_ordem;
            this.TipoServico = ordemServico.tipo_servico;
            this.Etapa = ordemServico.etapa;
            this.Municipio = ordemServico.municipio;
            
        }

        [Required(ErrorMessage = "Código da empresa não foi enviado.")]
        [Range(1, int.MaxValue, ErrorMessage = "Código da empresa não foi enviado.")]
        [DataMember]
        public int CodEmpresa { get; set; }

        [Required(ErrorMessage = "Tipo ordem, preenchimento obrigatório.")]
        [DataMember]
        public string TipoOrdem { get; set; }
        
        [Required(ErrorMessage = "Tipo serviço, preenchimento obrigatório.")]
        [DataMember]
        public string TipoServico { get; set; }

        [Required(ErrorMessage = "Etapa, preenchimento obrigatório.")]
        [DataMember]
        public string Etapa { get; set; }

        [Required(ErrorMessage = "Municipio, preenchimento obrigatório.")]
        [DataMember]
        public string Municipio { get; set; }

        [Required(ErrorMessage = "Informe uma data válida.")]
        [DataType(DataType.DateTime, ErrorMessage = "Data, preenchdientto obrigatório.")]
        [DataMember]
        public DateTime Dia { get; set; }

        [DataMember]
        public int Qtd { get; set; }

        [DataMember]
        public int Periodo { get; set; }

        [Required(ErrorMessage = "Hora de início do agendamento é obrigatória.")]
        [DataMember]
        public string HoraInicio { get; set; }
        
        [Required(ErrorMessage = "Hora de fim do agendamento é obrigatória.")]
        [DataMember]
        public string HoraFim { get; set; }

        public OrdemServicoDTO ordemServico { get; set; }



    }
}