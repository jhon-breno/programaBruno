using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Entidades.DTO
{
    public class RespuestaCorteRepoDTO
    {
        public RespuestaCorteRepoDTO()
        {
            ListaCortesReposSolicitados = new List<CorteRepoSolicitadoDTO>();
        }

        public RespuestaCorteRepoDTO(int pNumeroSuministro, string pCodigoEmpresa, string pIDCorteRepo)
        {
            this.NumeroSuministro = pNumeroSuministro;
            this.CodigoEmpresa = pCodigoEmpresa;

            ListaCortesReposSolicitados = new List<CorteRepoSolicitadoDTO>();
            ListaCortesReposSolicitados.Add(new CorteRepoSolicitadoDTO()
                {
                    NumeroOrden = "0000000000",
                    IDCorteRepo = pIDCorteRepo,
                    TipoRegistro = "00",
                    Motivo = "00",
                    Estado = "00",
                    FechaEjecucion = "1900-01-01T00:00:00",
                    FechaSolicitud = "1900-01-01T00:00:00",
                    AccionRealizada = "00",
                    Tipo = "00"
                });
        }
        
        [DataMember]
        [MaxLength(9)]
        public Int64 NumeroSuministro { get; set; }

        [DataMember]
        [MaxLength(4)]
        public string CodigoEmpresa { get; set; }

        [DataMember]
        public List<CorteRepoSolicitadoDTO> ListaCortesReposSolicitados { get; set; }
    }
}
