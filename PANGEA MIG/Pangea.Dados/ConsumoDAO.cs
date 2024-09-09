using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pangea.Entidades;
using Pangea.Dados.Base;
using Pangea.Entidades.DTO;
using System.Globalization;
using Pangea.Entidades.Enumeracao;

namespace Pangea.Dados
{
    public class ConsumoDAO : BaseDAO
    {
        public ConsumoDAO(Empresa empresa)
            : base(empresa)
        {
        }

        public List<ConsumoFaturadoDTO> GravaConsumoFaturadoEDevolveFalhas(List<ConsumoFaturadoDTO> listaEntity)
        {
            StringBuilder sql = new StringBuilder();
            DBProviderInformix informix;
            List<ConsumoFaturadoDTO> erros = new List<ConsumoFaturadoDTO>();

            foreach (ConsumoFaturadoDTO item in listaEntity)
            {
                informix = ObterProviderInformix();
                try
                {
                    sql = new StringBuilder();
                    DateTime dtAnt = DateTime.Parse(item.DataLeituraAnterior);
                    DateTime dtAtu = DateTime.Parse(item.DataLeituraAtual);
                    DateTime dtFat = DateTime.Parse(item.DataFaturamento);

                    sql.AppendFormat("INSERT INTO pangea_historico_consumo ");
                    sql.AppendFormat("VALUES ( 1, '{0}', NULL, ", item.NumeroCliente);
                    sql.AppendFormat("MDY({0}, {1}, {2}), ", dtAnt.Month, dtAnt.Day, dtAnt.Year );
                    sql.AppendFormat("MDY({0}, {1}, {2}), ", dtAtu.Month, dtAtu.Day, dtAtu.Year );
                    sql.AppendFormat("'{0}', ", item.AnoMesReferencia);
                    sql.AppendFormat("'{0}', ", item.ConsumoAtivoHP);
                    sql.AppendFormat("'{0}', ", item.ConsumoReativoHP);
                    sql.AppendFormat("'{0}', ", item.DmcrReativoHP);
                    sql.AppendFormat("'{0}', ", item.DemandaHP);
                    sql.AppendFormat("'{0}', ", item.ConsumoAtivoFP);
                    sql.AppendFormat("'{0}', ", item.ConsumoReativoFP);
                    sql.AppendFormat("'{0}', ", item.DmcrReativoFP);
                    sql.AppendFormat("'{0}', ", item.DemandaFP);
                    sql.AppendFormat("'{0}', ", item.ConsumoAtivoHR);
                    sql.AppendFormat("'{0}', ", item.ConsumoReativoHR);
                    sql.AppendFormat("'{0}', ", item.DmcrReativoHR);
                    sql.AppendFormat("'{0}', ", item.DemandaHR);
                    sql.AppendFormat("'{0}', ", item.ConsumoMedioDia);
                    sql.AppendFormat("MDY({0}, {1}, {2}) )", dtFat.Month, dtFat.Day, dtFat.Year );

                    informix.BeginTransacion();
                    if (ExecutarSqlValidando(sql.ToString(), informix))
                        informix.Commit();
                    else
                    {
                        item.PossuiErro = true;
                        item.DescricaoErro = "Não foi possível atualizar.";
                        erros.Add(item);
                        informix.Rollback();
                    }
                }
                catch (Exception ex)
                {
                    item.PossuiErro = true;
                    item.DescricaoErro = ex.Message;
                    erros.Add(item);
                    informix.Rollback();
                }
            }
            return erros;
        }

        public List<AjusteFaturadoDTO> GravaAjusteFaturadoEDevolveFalhas(List<AjusteFaturadoDTO> listaEntity)
        {
            StringBuilder sql = new StringBuilder();
            DBProviderInformix informix;
            List<AjusteFaturadoDTO> erros = new List<AjusteFaturadoDTO>();

            foreach (AjusteFaturadoDTO item in listaEntity)
            {
                informix = ObterProviderInformix();
                try
                {
                    sql = new StringBuilder();
                    DateTime dtAnt = DateTime.Parse(item.DataLeituraAnterior);
                    DateTime dtAtu = DateTime.Parse(item.DataLeituraAtual);
                    DateTime dtFat = DateTime.Parse(item.DataFaturamento);

                    sql.AppendFormat("INSERT INTO pangea_historico_consumo ");
                    sql.AppendFormat("VALUES ( 2, '{0}', ", item.NumeroCliente);
                    sql.AppendFormat("'{0}', ", item.TipoOperacao);
                    sql.AppendFormat("MDY({0}, {1}, {2}), ", dtAnt.Month, dtAnt.Day, dtAnt.Year);
                    sql.AppendFormat("MDY({0}, {1}, {2}), ", dtAtu.Month, dtAtu.Day, dtAtu.Year);
                    sql.AppendFormat("'{0}', ", item.AnoMesReferencia);
                    sql.AppendFormat("'{0}', ", item.ConsumoAtivoHP);
                    sql.AppendFormat("'{0}', ", item.ConsumoReativoHP);
                    sql.AppendFormat("'{0}', ", item.DmcrReativoHP);
                    sql.AppendFormat("'{0}', ", item.DemandaHP);
                    sql.AppendFormat("'{0}', ", item.ConsumoAtivoFP);
                    sql.AppendFormat("'{0}', ", item.ConsumoReativoFP);
                    sql.AppendFormat("'{0}', ", item.DmcrReativoFP);
                    sql.AppendFormat("'{0}', ", item.DemandaFP);
                    sql.AppendFormat("'{0}', ", item.ConsumoAtivoHR);
                    sql.AppendFormat("'{0}', ", item.ConsumoReativoHR);
                    sql.AppendFormat("'{0}', ", item.DmcrReativoHR);
                    sql.AppendFormat("'{0}', ", item.DemandaHR);
                    sql.AppendFormat("'{0}', ", item.ConsumoMedioDia);
                    sql.AppendFormat("MDY({0}, {1}, {2}) )", dtFat.Month, dtFat.Day, dtFat.Year);

                    //sql.AppendFormat(
                    //    "VALUES (2, '{0}', '{1}', '{2}', '{3}', '{4}', {5}, {6}, {7}, {8}, {9}, {10}, {11}, {12}, {13}, {14}, {15}, {16}, {17}, '{18}')",//, null)",
                    //    item.NumeroCliente,
                    //    item.TipoOperacao,
                    //    DateTime.Parse(item.DataLeituraAnterior).ToString("yyyy-MM-dd"),
                    //    DateTime.Parse(item.DataLeituraAtual).ToString("yyyy-MM-dd"),
                    //    item.AnoMesReferencia,
                    //    item.ConsumoAtivoHP,
                    //    item.ConsumoReativoHP,
                    //    item.DmcrReativoHP,
                    //    item.DemandaHP,
                    //    item.ConsumoAtivoFP,
                    //    item.ConsumoReativoFP,
                    //    item.DmcrReativoFP,
                    //    item.DemandaFP,
                    //    item.ConsumoAtivoHR,
                    //    item.ConsumoReativoHR,
                    //    item.DmcrReativoHR,
                    //    item.DemandaHR,
                    //    item.ConsumoMedioDia,
                    //    DateTime.Parse(item.DataFaturamento).ToString("yyyy-MM-dd")
                    //    );

                    informix.BeginTransacion();
                    if (ExecutarSqlValidando(sql.ToString(), informix))
                        informix.Commit();
                    else
                    {
                        item.PossuiErro = true;
                        item.DescricaoErro = "Não foi possível atualizar.";
                        erros.Add(item);
                        informix.Rollback();
                    }
                }
                catch (Exception ex)
                {
                    item.PossuiErro = true;
                    item.DescricaoErro = ex.Message;
                    erros.Add(item);
                    informix.Rollback();
                }
            }
            return erros;
        }
    }
}
