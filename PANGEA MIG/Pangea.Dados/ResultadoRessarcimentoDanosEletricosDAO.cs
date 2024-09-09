using Pangea.Entidades;
using Pangea.Dados.Base;
using Pangea.Entidades.Base;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pangea.Util;
using System.Web.Script.Serialization;
using Pangea.Entidades.Enumeracao;

namespace Pangea.Dados
{
    public class ResultadoRessarcimentoDanosEletricosDAO : BaseDAO
    {
        public ResultadoRessarcimentoDanosEletricosDAO(Empresa empresa)
            : base(empresa)
        {
            this.empresa = empresa;
        }

        public RessarcimentoDanosEletricosDTO RetornaDadosResultadoRessarcimentoDanosEletricos(String paramNumeroOrdem, String paramCorrVisita)
        {

            String sql = String.Format(@"SELECT v.cod_retorno 
                                               ,v.eletricista 
                                               ,v.data_exec_visita 
                                               ,v.hora_exec_visita
                                               ,v.rol_ret_visita
                                               ,v.data_ret_visita
                                               ,v.hora_fim_prevista
                                               ,v.rol_responsavel
                                               ,v.dat_lib_def_tec
                                               ,v.rol_lib_def_tec
                                               ,v.codigo_cargo
                                               ,v.valor_cargo
                                               ,v.numero_ordem_filha
                                               ,v.numero_form_venda
                                               ,v.serie_form_venda
                                               ,v.data_despacho
                                               ,v.numero_tarefa
                                               ,v.data_inic_visita
                                               ,v.periodo_agend
                                               ,v.data_agend
                                               ,v.turno_agend
                                               ,os.numero_cliente
                                          FROM visita_ordem v, ordem_servico os
                                          WHERE v.numero_ordem = '{0}'
                                            AND v.corr_visita = {1}
                                            AND os.numero_ordem = v.numero_ordem
                                            AND os.corr_visita = v.corr_visita", paramNumeroOrdem, paramCorrVisita);

            DataTable dtResultado = ConsultaSql(sql);

            return DataHelper.ConvertDataTableToEntity<RessarcimentoDanosEletricosDTO>(dtResultado);
        }
    }
}
