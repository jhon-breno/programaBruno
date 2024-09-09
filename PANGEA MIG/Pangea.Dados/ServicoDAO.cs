using Pangea.Entidades.DTO;
using Pangea.Dados.Base;
using Pangea.Entidades;
using Pangea.Entidades.Enumeracao;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using Pangea.Util;

namespace Pangea.Dados
{
    public class ServicoDAO : BaseDAO
    {
        //private string _empresa;

        public ServicoDAO(Empresa empresa)
            : base(empresa)
        {
        }

        public ServicoDAO(Empresa empresa, string DataBase)
            : base(empresa, DataBase)
        {
        }

        public List<Servico> Consultar(Servico obj)
        {
            if (obj == null)
                return new List<Servico>();

            #region Prepara a consulta básica 

            StringBuilder sql = new StringBuilder(@"SELECT *
                                                      FROM servicos s WHERE 1=1 ");
            #endregion

            if (!string.IsNullOrEmpty(obj.tipo_ordem))
                sql.AppendFormat("AND tipo_ordem = '{0}' ", obj.tipo_ordem);

            if (!string.IsNullOrEmpty(obj.cod_servico))
                sql.AppendFormat("AND cod_servico = '{0}' ", obj.cod_servico);

            return DataHelper.ConvertDataTableToList<Servico>(ConsultaSql(sql.ToString()));
            //return Consultar(ConsultaSql(sql.ToString()));
        }

        public List<Servico> Consultar(DataTable dt)
        {
            List<Servico> lstServico = new List<Servico>();
            Servico _servico = new Servico();

            if (dt == null || dt.Rows.Count == 0)
                return null;

            foreach (DataRow linha in dt.Rows)
            {
                //TODO: acrescentar os demais campos da tabela SERVICOS
                _servico = new Servico();

                if (!DBNull.Value.Equals(linha["tipo_ordem"]))
                    _servico.tipo_ordem = linha["tipo_ordem"].ToString();

                if (!DBNull.Value.Equals(linha["cod_servico"]))
                    _servico.cod_servico = linha["cod_servico"].ToString();

                if (!DBNull.Value.Equals(linha["des_servico"]))
                    _servico.des_servico = linha["des_servico"].ToString();

                if (!DBNull.Value.Equals(linha["ind_dias_verifica_debito"]))
                    _servico.ind_dias_verifica_debito = Convert.ToInt32(linha["ind_dias_verifica_debito"].ToString());

                if (!DBNull.Value.Equals(linha["ind_verif_deb"]))
                    _servico.ind_verif_deb = linha["ind_verif_deb"].ToString();

                if (!DBNull.Value.Equals(linha["grupo"]))
                    _servico.grupo = Convert.ToInt32(linha["grupo"].ToString());

                if (!DBNull.Value.Equals(linha["ind_cliente_desper"]))
                    _servico.ind_cliente_desper = linha["ind_cliente_desper"].ToString();

                if (!DBNull.Value.Equals(linha["ind_irregularidade"]))
                    _servico.ind_irregularidade = linha["ind_irregularidade"].ToString();

                lstServico.Add(_servico);
            }

            return lstServico.ToList();
        }

        public DataTable Consultar(ServicoDTO obj)
        {
            if (obj == null)
                return new DataTable();

            if (Empresa.NaoIdentificada.Equals(base.empresa))
            {
                //TODO: gerar log antes de lançar erro
                throw new ArgumentException("Parâmetro empresa obrigatório para a consulta de serviço.");
            }

            #region Prepara a consulta básica 

            StringBuilder sql = new StringBuilder("SELECT * FROM servicos WHERE 1=1 ");

            if(!string.IsNullOrEmpty(obj.tipo_ordem))
                sql.AppendFormat("AND tipo_ordem = '{0}' ", obj.tipo_ordem);

            if (!string.IsNullOrEmpty(obj.cod_servico))
                sql.AppendFormat("AND cod_servico = '{0}' ", obj.cod_servico);

            if (!string.IsNullOrEmpty(obj.grupo))
                sql.AppendFormat("AND grupo = '{0}' ", obj.grupo);

            if (!string.IsNullOrEmpty(obj.subgrupo))
                sql.AppendFormat("AND subgrupo = '{0}' ", obj.subgrupo);

            if (!string.IsNullOrEmpty(obj.ind_zona))
                sql.AppendFormat("AND ind_zona = '{0}' ", obj.ind_zona);

            #endregion

            return ConsultaSql(sql.ToString());
        }

        public string ObterAreaServico(string pTipoOrdem, string pTipoServico, string pEtapa, string pSucursal)
        {
            if (Empresa.NaoIdentificada.Equals(base.empresa))
            {
                //TODO: gerar log antes de lançar erro
                throw new ArgumentException("Parâmetro empresa obrigatório para a consulta da área.");
            }

            #region Consulta

            StringBuilder sql = new StringBuilder("select area from distribuicao_etapa ");
            sql.AppendFormat("where tipo_ordem = '{0}' ", pTipoOrdem);
            sql.AppendFormat("and tipo_servico = '{0}' ", pTipoServico);
            sql.AppendFormat("and etapa = '{0}' ", pEtapa);
            sql.AppendFormat("and sucursal = '{0}' ", pSucursal);

            #endregion

            DataTable _dt = ConsultaSql(sql.ToString());
            if (_dt == null || _dt.Rows.Count == 0)
                return string.Empty;

            if (DBNull.Value.Equals(_dt.Rows[0]["area"]))
                return string.Empty;

            return _dt.Rows[0]["area"].ToString().Trim();
        }

        public List<Servico> Consultar(ContratoDTO pContrato)
        {
            if (pContrato == null)
                return new List<Servico>();

            StringBuilder sql = new StringBuilder();

            #region Consulta
            sql.Append("SELECT va_tipo_orden, va_tipo_servico ");
            sql.Append("FROM ct_validaciones ");
            sql.Append("WHERE 1=1 ");
            sql.AppendFormat("AND va_motivo_empresa = '{0}' ", pContrato.motivo);
            sql.AppendFormat("AND va_motivo_cliente = '{0}' ", pContrato.motivo);
            sql.AppendFormat("AND va_submotivo = '{0}' ", pContrato.submotivo);
            sql.Append("AND va_trancode = 'T_INGOUTRO' ");
            sql.Append("AND va_tipo_orden <> '' ");
            sql.Append("AND va_tipo_servico <> '' ");
            sql.Append("AND va_tipo_orden IS NOT NULL ");
            sql.Append("AND va_tipo_servico IS NOT NULL ");
            #endregion

            DataTable dt = ConsultaSql(sql.ToString());
            Servico servico = new Servico();

            if (dt != null && dt.Rows.Count > 0)
            {
                foreach (DataRow linha in dt.Rows)
                {
                    servico = new Servico();
                    servico.tipo_ordem = DBNull.Value.Equals(linha["va_tipo_orden"]) ? string.Empty : linha["va_tipo_orden"].ToString();
                    servico.cod_servico = DBNull.Value.Equals(linha["va_tipo_servico"]) ? string.Empty : linha["va_tipo_servico"].ToString();
                }
            }

            return Consultar(servico);
        }


        /// <summary>
        /// Recupera uma lista de serviços do Synergia correspondentes aos serviços do SalesForce, vinculados via tipo_ordem e tipo_servico.
        /// </summary>
        /// <param name="motivoIntegracao"></param>
        /// <returns></returns>
        public List<ServicosIntegracaoDTO> ObterServicosSynergia(ServicosIntegracaoDTO motivoIntegracao)
        {
            StringBuilder sql = new StringBuilder(@"select  id_servico, empresa, canal_atendimento, tipo_cliente, cod_motivo,
                                                            cod_submotivo, tipo_ordem, tipo_servico, data_ativacao, data_desativacao
                                         from servicos_integracao
                                         where 1=1 ");

            if (motivoIntegracao != null && !string.IsNullOrEmpty(motivoIntegracao.canal_atendimento))
                sql.AppendFormat(" AND CANAL_ATENDIMENTO = '{0}' ", motivoIntegracao.canal_atendimento);

            if (motivoIntegracao != null && !string.IsNullOrEmpty(motivoIntegracao.tipo_cliente))
                sql.AppendFormat(" AND TIPO_CLIENTE = '{0}' ", motivoIntegracao.tipo_cliente);

            if (motivoIntegracao != null && !string.IsNullOrEmpty(motivoIntegracao.cod_motivo))
                sql.AppendFormat(" AND COD_MOTIVO = '{0}' ", motivoIntegracao.cod_motivo);

            if (motivoIntegracao != null && !string.IsNullOrEmpty(motivoIntegracao.cod_submotivo))
                sql.AppendFormat(" AND COD_SUBMOTIVO = '{0}' ", motivoIntegracao.cod_submotivo);

            if (motivoIntegracao != null && !string.IsNullOrEmpty(motivoIntegracao.tipo_ordem))
                sql.AppendFormat(" AND TIPO_ORDEM = '{0}' ", motivoIntegracao.tipo_ordem);

            if (motivoIntegracao != null && !string.IsNullOrEmpty(motivoIntegracao.tipo_servico))
                sql.AppendFormat(" AND TIPO_SERVICO = '{0}' ", motivoIntegracao.tipo_servico);

            return DataHelper.ConvertDataTableToList<ServicosIntegracaoDTO>(ConsultaSql(sql.ToString()));
        }
    }
}
