using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Pangea.Entidades;
using Pangea.Entidades.Enumeracao;


namespace Pangea.Dados.Base
{
    public abstract class BaseDAO : IDisposable
    {
        #region Propriedades Privadas
        private string connectionString = string.Empty;
        protected Empresa empresa = Empresa.NaoIdentificada;
        #endregion
        public BaseDAO()
        {

        }

        public BaseDAO(Empresa empresa, string database)
        {
            AppSettingsReader configurationAppSettings = new AppSettingsReader();

            if (database=="BT")
            {
                switch (empresa)
                {
                    case Empresa.RJ:
                        //PRO
                        //connectionString = "Database=clientes;Host=10.14.45.66;Server=clientes;Service=12999;Protocol=onsoctcp;UID=synergia;Password=syn4pro;Pooling=true;Min Pool Size=0;Max Pool Size=4000;Enlist=true;Connection Lifetime=5;Connect Timeout=60000";
                        //connectionString = "Database=clientes;Host=10.14.45.66;Server=clientes;Service=12999;Protocol=onsoctcp;UID=avb2000;Password=avb4pro;Pooling=true;Min Pool Size=0;Max Pool Size=4000;Enlist=true;Connection Lifetime=5;Connect Timeout=360000";
                        connectionString = "Database=clientes;Host=10.14.45.66;Server=clientes;Service=12999;Protocol=onsoctcp;UID=clientes;Password=cli4pro;Pooling=true;Min Pool Size=0;Max Pool Size=4000;Enlist=true;Connection Lifetime=5;Connect Timeout=360000";
                        connectionString = "Database=clientes;Host=10.14.43.116;Server=clientes;Service=12999;Protocol=onsoctcp;UID=clientes;Password=clipre;Pooling=true;Min Pool Size=0;Max Pool Size=4000;Enlist=true;Connection Lifetime=5;Connect Timeout=360000";
                        //PRE
                        //connectionString = "Database=clientes;Host=10.14.41.17;Server=clientes;Service=60054;Protocol=onsoctcp;UID=clientes;Password=clipre;Pooling=true;Min Pool Size=0;Max Pool Size=4000;Enlist=true;Connection Lifetime=5;";
                        break;
                    case Empresa.CE:
                        //PROD
                        //connectionString = "Database=clientes;Host=10.14.45.45;Server=clientes;Service=60054;Protocol=onsoctcp;UID=synergia;Password=syn4pro;Pooling=true;Min Pool Size=0;Max Pool Size=4000;Enlist=true;Connection Lifetime=5;Connect Timeout=60000";
                        //connectionString = "Database=clientes;Host=10.14.45.45;Server=clientes;Service=60054;Protocol=onsoctcp;UID=avb2000;Password=avb4pro;Pooling=true;Min Pool Size=0;Max Pool Size=4000;Enlist=true;Connection Lifetime=5;Connect Timeout=360000";
                        ///connectionString = "Database=clientes;Host=10.154.78.75;Server=clientes;Service=60054;Protocol=onsoctcp;UID=avb2000;Password=avb4pro;Pooling=true;Min Pool Size=0;Max Pool Size=4000;Enlist=true;Connection Lifetime=5;Connect Timeout=360000";
                        connectionString = "Database=clientes;Host=10.154.78.75;Server=clientes;Service=60054;Protocol=onsoctcp;UID=clientes;Password=cli4pro;Pooling=true;Min Pool Size=0;Max Pool Size=4000;Enlist=true;Connection Lifetime=5;Connect Timeout=360000";
                        //PRE   
                        //connectionString = "Database=clientes;Host=10.14.43.114;Server=clientes;Service=60054;Protocol=onsoctcp;UID=clientes;Password=clipre;Pooling=true;Min Pool Size=0;Max Pool Size=4000;Enlist=true;Connection Lifetime=5;";
                        break;
                }
            }
            else
            {
                switch (empresa)
                {
                    case Empresa.RJ:
                        //PROD
                        connectionString = "Database=grandes;Host=10.14.45.66;Server=clientes;Service=12999;Protocol=onsoctcp;UID=clientes;Password=cli4pro;Pooling=true;Min Pool Size=0;Max Pool Size=4000;Enlist=true;Connection Lifetime=5;Connect Timeout=360000";
                        //PRE
                        break;
                    case Empresa.CE:
                        //PROD
                        //connectionString = "Database=grandes;Host=10.14.45.45;Server=clientes;Service=60054;Protocol=onsoctcp;UID=clientes;Password=cli4pro;Pooling=true;Min Pool Size=0;Max Pool Size=4000;Enlist=true;Connection Lifetime=5;Connect Timeout=360000";
                        connectionString = "Database=grandes;Host=10.154.78.75;Server=clientes;Service=60054;Protocol=onsoctcp;UID=clientes;Password=cli4pro;Pooling=true;Min Pool Size=0;Max Pool Size=4000;Enlist=true;Connection Lifetime=5;Connect Timeout=360000";
                        //PRE   
                        //connectionString = "Database=grandes;Host=10.14.43.114;Server=clientes;Service=60054;Protocol=onsoctcp;UID=clientes;Password=clipre;Pooling=true;Min Pool Size=0;Max Pool Size=4000;Enlist=true;Connection Lifetime=5;";

                        break;
                }
            }
            this.empresa = empresa;
        }

        public BaseDAO(Empresa empresa)
        {
            AppSettingsReader configurationAppSettings = new AppSettingsReader();
            switch (empresa)
            {
                case Empresa.RJ:
                    //connectionString = "Database=clientes;Host=10.14.45.66;Server=clientes;Service=12999;Protocol=onsoctcp;UID=synergia;Password=syn4pro;Pooling=true;Min Pool Size=0;Max Pool Size=4000;Enlist=true;Connection Lifetime=5;Connect Timeout=60000";
                    //connectionString = "Database=clientes;Host=10.14.45.66;Server=clientes;Service=12999;Protocol=onsoctcp;UID=avb2000;Password=avb4pro;Pooling=true;Min Pool Size=0;Max Pool Size=4000;Enlist=true;Connection Lifetime=5;Connect Timeout=60000";
                break;

                case Empresa.CE:
                    //connectionString = "Database=grandes;Host=10.14.45.45;Server=clientes;Service=60054;Protocol=onsoctcp;UID=clientes;Password=cli4pro;Pooling=true;Min Pool Size=0;Max Pool Size=4000;Enlist=true;Connection Lifetime=5;Connect Timeout=60000";
                    //connectionString = "Database=grandes;Host=10.14.45.45;Server=clientes;Service=60054;Protocol=onsoctcp;UID=avb2000;Password=avb4pro;Pooling=true;Min Pool Size=0;Max Pool Size=4000;Enlist=true;Connection Lifetime=5;Connect Timeout=60000";
                break;

            }
            this.empresa = empresa;
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
            result = informix.ExecuteNonQuery() > 0;

            return result;
        }

        protected bool ExecutarSqlValidando(string sql, DBProviderInformix informix)
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
            result = informix.ExecuteQuery();

            return result;
        }
   

        protected int ExecutarSql(string sql)
        {
            using (DBProviderInformix informix = ObterProviderInformix())
            {
                informix.PrepareCommand(sql);

                if (string.IsNullOrEmpty(sql))
                {
                    throw new Exception("Query de consulta não pode ser null ou vazio.");
                }

                if (informix == null)
                {
                    throw new Exception("Informix não pode ser null ou vazio.");
                }

                informix.PrepareCommand(sql);
                return informix.ExecuteNonQuery();
            }
        }

        public void Dispose()
        {
            connectionString = null;
        }

        protected DataTable ConsultaSqlDinamico(string sql)
        {
            DataTable result = null;

            if (string.IsNullOrEmpty(sql))
            {
                throw new Exception("Query de consulta não pode ser null ou vazio.");
            }

            using (DBProviderInformix informix = ObterProviderInformix())
            {
                informix.PrepareCommand(sql, true);
                result = informix.ExecuteDataReader();
            }

            return result;
        }
    }
}
