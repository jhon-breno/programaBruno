using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pangea.Entidades;


namespace Pangea.Dados.Base
{
    public abstract class BaseDAO
    {

        private string connectionString = string.Empty;

        public BaseDAO()
        {

        }

        public BaseDAO(Empresa empresa)
        {
            AppSettingsReader configurationAppSettings = new AppSettingsReader();
            switch (empresa)
            {
                case Empresa.Ampla:
                    connectionString = configurationAppSettings.GetValue("ServiceURIAmpla", typeof(String)).ToString();
                    break;

                case Empresa.Coelce:
                    connectionString = configurationAppSettings.GetValue("ServiceURICoelce", typeof(String)).ToString();
                    break;

            }

        }

        protected DataTable ConsultaSql(string sql)
        
        {
            return ConsultaSql(sql, true);
        }

        protected DataTable ConsultaSql(string sql, DBProviderInformix informix)
        {
            DataTable result = null;

            if (string.IsNullOrEmpty(sql))
            {
                throw new Exception("Query de consulta não pode ser null ou vazio.");
            }


            informix.PrepareCommand(sql);
            result = informix.ExecuteDataTable();


            return result;
        }

        protected DataTable ConsultaSql(string sql, bool useDirtyRead)
        {
            DataTable result = null;

            if (string.IsNullOrEmpty(sql))
            {
                throw new Exception("Query de consulta não pode ser null ou vazio.");
            }

            using (DBProviderInformix informix = ObterProviderInformix())
            {
                informix.PrepareCommand(sql, useDirtyRead);
                result = informix.ExecuteDataTable();
            }

            return result;
        }

        public DBProviderInformix ObterProviderInformix()
        {

            return new DBProviderInformix(this.connectionString);
        }

        #region Parser bd para c#
        internal string TratarString(DataTable dt, DataRow item, string coluna)
        {
            return (!dt.Columns.Contains(coluna) || item[coluna] == DBNull.Value || string.IsNullOrEmpty(item[coluna].ToString().Trim())) ? string.Empty : item[coluna].ToString().Trim();
        }

        internal int TratarInt(DataTable dt, DataRow item, string coluna, int valorPadrao)
        {
            int result = valorPadrao;

            if (dt.Columns.Contains(coluna) && !(item[coluna] == DBNull.Value) && !string.IsNullOrEmpty(item[coluna].ToString().Trim()))
            {
                int.TryParse(item[coluna].ToString().Trim(), out result);
            }

            return result;
        }

        internal short TratarShort(DataTable dt, DataRow item, string coluna, int valorPadrao)
        {
            short result = (short)valorPadrao;

            if (dt.Columns.Contains(coluna) && !(item[coluna] == DBNull.Value) && !string.IsNullOrEmpty(item[coluna].ToString().Trim()))
            {
                short.TryParse(item[coluna].ToString().Trim(), out result);
            }

            return result;
        }

        internal char TratarChar(DataTable dt, DataRow item, string coluna)
        {
            char result = '\0';

            if (dt.Columns.Contains(coluna) && !(item[coluna] == DBNull.Value) && !string.IsNullOrEmpty(item[coluna].ToString().Trim()))
            {
                char.TryParse(item[coluna].ToString().Trim(), out result);
            }

            return result;
        }

        internal DateTime TratarDateTime(DataTable dt, DataRow item, string coluna)
        {
            DateTime result = DateTime.MinValue;

            if (dt.Columns.Contains(coluna) && !(item[coluna] == DBNull.Value) && !string.IsNullOrEmpty(item[coluna].ToString().Trim()))
            {
                DateTime.TryParse(item[coluna].ToString().Trim(), out result);
            }

            return result;
        }

        internal DateTime? TratarDateTimeNullable(DataTable dt, DataRow item, string coluna)
        {
            DateTime? result = null;
            DateTime result1 = DateTime.MinValue;

            if (dt.Columns.Contains(coluna) && !(item[coluna] == DBNull.Value) && !string.IsNullOrEmpty(item[coluna].ToString().Trim()))
            {
                DateTime.TryParse(item[coluna].ToString().Trim(), out result1);
                return result1;
            }
            else
                return result;
        }

        internal float TratarFloat(DataTable dt, DataRow item, string coluna, int valorPadrao)
        {
            float result = (float)valorPadrao;

            if (dt.Columns.Contains(coluna) && !(item[coluna] == DBNull.Value) && !string.IsNullOrEmpty(item[coluna].ToString().Trim()))
            {
                float.TryParse(item[coluna].ToString().Trim(), out result);
            }

            return result;
        }

        internal double TratarDouble(DataTable dt, DataRow item, string coluna, int valorPadrao)
        {
            double result = (double)valorPadrao;

            if (dt.Columns.Contains(coluna) && !(item[coluna] == DBNull.Value) && !string.IsNullOrEmpty(item[coluna].ToString().Trim()))
            {
                double.TryParse(item[coluna].ToString().Trim(), out result);
            }

            return result;
        }

        internal bool TratarBool(DataTable dt, DataRow item, string coluna)
        {
            bool result = false;

            if (dt.Columns.Contains(coluna) && !(item[coluna] == DBNull.Value) && !string.IsNullOrEmpty(item[coluna].ToString().Trim()))
            {
                bool.TryParse(item[coluna].ToString().Trim(), out result);
            }

            return result;
        }
        #endregion

        protected bool ExecutarSql(string sql, DBProviderInformix informix)
        {
            if (string.IsNullOrEmpty(sql))
            {
                throw new Exception("Query de consulta não pode ser null ou vazio.");
            }

            if (informix == null)
            {
                throw new Exception("Informix não pode ser null ou vazio.");
            }

            bool result = false;

            informix.PrepareCommand(sql);

            result = informix.ExecuteNonQuery();

            return result;
        }

    }
}
