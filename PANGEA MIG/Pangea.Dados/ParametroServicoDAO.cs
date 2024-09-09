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

namespace Pangea.Dados
{
    public class ParametroServicoDAO : BaseDAO
    {
        private string _empresa;

        public ParametroServicoDAO(Empresa empresa)
            : base(empresa)
        {
            if (empresa != null)
                this._empresa = empresa.ToString();
        }


        public IList<ParametroServico> Consultar(ParametroServico obj)
        {
            if (obj == null)
                return new List<ParametroServico>();

            if (string.IsNullOrEmpty(this._empresa))
            {
                //TODO: gerar log antes de lançar erro
                throw new ArgumentException("É obrigatório informar uma empresa válida para a consulta dos parâmetro de serviços.");
            }

            #region Prepara a consulta básica 

            StringBuilder sql = new StringBuilder(@"SELECT ps.*, 
                                                           extend(current, year to second) as  data_atual_bd
                                                      FROM parm_servicos ps");
            #endregion

            return dtToListObject<ParametroServico>(ConsultaSql(sql.ToString()));
        }


        public  IList<TEntidade> dtToListObject<TEntidade>(DataTable dt)
        {
            List<ParametroServico> lstParametroServico = new List<ParametroServico>();
            ParametroServico parametroServico = new ParametroServico();

            if (dt == null || dt.Rows.Count == 0)
                return (IList<TEntidade>)lstParametroServico;

            foreach (DataRow linha in dt.Rows)
            {
                if (!DBNull.Value.Equals(dt.Rows[0]["hora_ini_rel_aut"]))
                    parametroServico.hora_inicio_religacao_automatica = DateTime.Parse(linha["hora_ini_rel_aut"].ToString());

                if (!DBNull.Value.Equals(dt.Rows[0]["hora_fim_rel_aut"]))
                    parametroServico.hora_fim_religacao_automatica = DateTime.Parse(linha["hora_fim_rel_aut"].ToString());

                if (!DBNull.Value.Equals(dt.Rows[0]["data_atual_bd"]))
                    parametroServico.data_atual_bd = DateTime.Parse(linha["data_atual_bd"].ToString());

                lstParametroServico.Add(parametroServico);
            }

            return (IList<TEntidade>)lstParametroServico;
        }
    }
}
