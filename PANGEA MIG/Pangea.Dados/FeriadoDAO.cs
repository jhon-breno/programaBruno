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
using Entidades.DTO;
using IBM.Data.Informix;
using Pangea.Entidades.Enumeracao;

namespace Pangea.Dados
{
    public class FeriadoDAO : BaseDAO
    {
        private Empresa empresa; 
        public FeriadoDAO(Empresa empresa)
            : base(empresa)
        {
            this.empresa = empresa;
        }


        /// <summary>
        /// Método(DAO) responsável por salvar o Feriado.
        /// </summary>
        /// <param name="feriado" ></param>
        /// <param name="temErro"></param>
        /// <returns>FeriadoDTO</returns>
        public FeriadoDTO Salvar(FeriadoDTO feriado, DBProviderInformix informix)
        {
            feriado.erro = new ErroArquivo();

            StringBuilder sql = new StringBuilder();
            try
            {
                sql.Append("INSERT INTO feriados(fecha, municipio) ");
                sql.AppendFormat("VALUES(TO_DATE('{0}','%d/%m/%Y'), '{1}')",feriado.dataDoFeriado.Replace('.','/'),feriado.codigoDoMunicipio);
                ExecutarSql(sql.ToString(), informix);
            }
            catch (IfxException ex) 
            {

                List<string> listaDeCodigosDeErrosSQL = new List<string>();

                for(int i=0; i < ex.Errors.Count; i++)
                {
                    listaDeCodigosDeErrosSQL.Add(ex.Errors[i].SQLState);
                }
                // Tratamento na inclusão de registro(UNIQUE) já existente no banco.
                const string codigoDeErroDuplicidadeDeRegistroUnique = "23000";
                if ( listaDeCodigosDeErrosSQL.Contains(codigoDeErroDuplicidadeDeRegistroUnique) )
                {
                    feriado.erro.descricaoDoErro = string.Format("Data do Feriado {0} já cadastrada para o município {1}", feriado.dataDoFeriado, feriado.siglaDoMunicipio); 
                }
            }
            catch (Exception e)
            {
                feriado.erro.descricaoDoErro = e.Message;
            }
            return feriado;
        }

    }
}
