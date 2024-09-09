using Pangea.Dados.Base;
using Pangea.Entidades;
using Pangea.Entidades.DTO;
using Pangea.Entidades.Enumeracao;
using System;
using System.Collections.Generic;
using System.Text;

namespace Pangea.Dados
{
    public class CalendFacturacionDAO : BaseDAO
    {
        private Empresa empresa;
        public CalendFacturacionDAO(Empresa empresa)
            : base(empresa)
        {
            this.empresa = empresa;
        }

        public List<CalendFacturacionDTO> AtualizaCaledarioFaturacaoEDevolveFalhas(List<CalendFacturacionDTO> listaEntity)
        {
            StringBuilder sql = new StringBuilder();
            DBProviderInformix informix; //= ObterProviderInformix();
            List<CalendFacturacionDTO> erros = new List<CalendFacturacionDTO>();

            MunicipioSapDAO municipioSapDAO = new MunicipioSapDAO(empresa);
            Dictionary<string, string> deSiglaSapParaMunicipio = new Dictionary<string, string>();
            deSiglaSapParaMunicipio = municipioSapDAO.RetornaSiglaEMunicipioSap();

            foreach (CalendFacturacionDTO item in listaEntity)
            {
                informix = ObterProviderInformix();
                try
                {
                    sql = new StringBuilder();

                    if (!deSiglaSapParaMunicipio.ContainsKey(item.Municipio))
                    {
                        item.PossuiErro = true;
                        item.DescricaoErro = "Código de município não consta na tabela municipio_sap.";
                        continue;
                    }

                    item.Municipio = deSiglaSapParaMunicipio[item.Municipio];
                    DateTime dt = DateTime.Parse(item.ScheduleBillingDate);

                    sql.AppendFormat("UPDATE agenda1 ");
                    sql.AppendFormat("SET fecha_factura = MDY({0}, {1}, {2}) ", dt.Month, dt.Day, dt.Year);
                    sql.AppendFormat("WHERE municipio = '{0}' ", item.Municipio);
                    sql.AppendFormat("AND localidade = '{0}' ", item.Localidade);
                    sql.AppendFormat("AND sector = '{0}' ", item.Sector);

                    informix.BeginTransacion();
                    if (ExecutarSqlValidando(sql.ToString(), informix))
                        informix.Commit();
                    else
                    {
                        item.PossuiErro = true;
                        item.DescricaoErro = "(DB) Não foi possível atualizar.";
                        erros.Add(item);
                        informix.Rollback();
                    }
                }
                catch(Exception ex)
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