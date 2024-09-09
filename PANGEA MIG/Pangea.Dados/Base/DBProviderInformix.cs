using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IBM.Data.Informix;
using System.Configuration;
using System.Data;
using System.Collections;

namespace Pangea.Dados.Base
{
    public class DBProviderInformix : IDisposable
    {
        private IfxConnection _connection = null;
        private IfxCommand _command = null;
        private IfxDataReader _reader = null;
        private IfxTransaction _transaction = null;
        private bool _isDirtyRead;

        public DBProviderInformix(string connectionString)
        {
            try
            {
                _connection = new IfxConnection(connectionString);
            }
            catch (Exception ex)
            {
                _connection = null;
                throw ex;
            }
        }

        private static string setIsolation = "SET ISOLATION TO DIRTY READ";
        private void SetIsolationToDirtyRead()
        {
            _command.CommandText = setIsolation;
            OpenConnection();
            _command.ExecuteNonQuery();

            _isDirtyRead = true;
        }

        public void OpenConnection()
        {
            if (_connection != null && _connection.State == ConnectionState.Closed)
            {
                try
                {
                    _connection.Open();
                    if (_command == null)
                        _command = _connection.CreateCommand();
                }
                catch(Exception ex)
                {
                    throw ex;
                }
            }
        }

        public void CloseConnection()
        {
            if (_connection != null)
            {

                if (_connection.State == ConnectionState.Open)
                {
                    _connection.Close();
                }

                _connection.Dispose();
            }
        }

        public void BeginTransacion()
        {
            OpenConnection();
            _transaction = _connection.BeginTransaction(IsolationLevel.ReadUncommitted);
            _command.Transaction = _transaction;
        }

        public void BeginTransacion(IsolationLevel isolamento)
        {
            OpenConnection();
            _transaction = _connection.BeginTransaction(isolamento);
            _command.Transaction = _transaction;
        }

        public bool Commit()
        {
            _transaction.Commit();
            CloseConnection();
            return true;
        }

        public bool Rollback()
        {
            _transaction.Rollback();
            CloseConnection();
            return true;
        }

        public IfxCommand Command { get { return _command; } }
        public IfxDataReader DataReader { get { return _reader; } }

        public bool PrepareCommand(StringBuilder commandText)
        {
            OpenConnection();

            if (_command == null)
            {
                _command = _connection.CreateCommand();
            }

            if (!_isDirtyRead)
                this.SetIsolationToDirtyRead();

            if (_command.Parameters != null || _command.Parameters.Count > 0)
                _command.Parameters.Clear();

            _command.CommandText = commandText.ToString();

            return (true);
        }

        public bool PrepareCommand(string commandText)
        {
            return this.PrepareCommand(commandText, true);
        }

        public bool PrepareCommand(string commandText, bool useDirtyRead)
        {
            OpenConnection();

            if (_command == null)
            {
                _command = _connection.CreateCommand();
            }

            if (useDirtyRead && !_isDirtyRead)
                this.SetIsolationToDirtyRead();

            if (_command.Parameters != null || _command.Parameters.Count > 0)
                _command.Parameters.Clear();

            _command.CommandText = commandText;

            return (true);
        }

        public void AddParameter(string name, object value)
        {
            _command.Parameters.Add(new IfxParameter(name, value));
        }

        private static string erroNoCommand = "Para executar uma query é necessário utilizar a execução do comando através do método PrepareCommand";
        private static string nadaAlterado = "Nenhum dado alterado";

        public int ExecuteNonQuery()
        {
            OpenConnection();

            if (_command == null)
                throw new Exception(erroNoCommand);

            return _command.ExecuteNonQuery();
        }


        public DataSet ExecuteProcedure(string storedProcedureName, Dictionary<string, object> parameters)
        {
            DataSet ds = new DataSet();

            this.PrepareCommand(storedProcedureName);
            _command.CommandType = CommandType.StoredProcedure;

            foreach (KeyValuePair<string, object> entry in parameters)
            {
                AddParameter(entry.Key, entry.Value);
            }
            IfxDataAdapter _dataAdapter = new IfxDataAdapter(_command);

            try
            {
                _dataAdapter.Fill(ds);

                if (_command != null && _command.Transaction != null)
                    _command.Transaction.Commit();
            }
            catch
            {
                if (_command != null && _command.Transaction != null)
                    _command.Transaction.Rollback();
            }

            return ds;
        }

        public bool ExecuteQuery()
        {
            OpenConnection();

            if (_command == null)
                throw new Exception(erroNoCommand);

            int retorno = _command.ExecuteNonQuery();


            return (retorno > 0);
        }

        public IDataReader ExecuteReader()
        {
            OpenConnection();

            if (_command == null)
                throw new Exception(erroNoCommand);

            _reader = _command.ExecuteReader();
            if (_reader == null)
                throw new Exception(nadaAlterado);

            return (_reader);
        }

        public DataTable ExecuteDataReader()
        {
            try
            {
                List<string> repeticoes = new List<string>();
                DataTable dt = new DataTable();
                using (IDataReader reader = ExecuteReader())
                {
                    foreach (DataRow dr in reader.GetSchemaTable().Rows)
                    {
                        DataColumn dc = new DataColumn(dr[0].ToString());

                        if (!dt.Columns.Contains(dr[0].ToString()))
                            dt.Columns.Add(dc);
                        else
                        {
                            string valor = dr[0].ToString();
                            repeticoes.Add(valor);
                            dt.Columns.Add(string.Concat(dc, "_", repeticoes.Where(x => x.Equals(valor)).Count() + 1));
                        }
                    }

                    int a = reader.FieldCount;
                    while (reader.Read())
                    {
                        DataRow dr = dt.NewRow();
                        for (int i = 0; i < a; i++)
                        {
                            dr[i] = reader.GetValue(i) == DBNull.Value ? string.Empty : reader.GetValue(i);
                        }
                        dt.Rows.Add(dr);

                    }
                    reader.Close();
                }
                repeticoes = null;
                return dt;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                CloseConnection();
            }
        }


        public DataTable ExecuteDataTable()
        {
            DataTable dt = new DataTable();

            using (IDataReader reader = ExecuteReader())
            {
                dt.BeginLoadData();
                dt.Load(reader, LoadOption.OverwriteChanges);
                dt.EndLoadData();
            }
            CloseConnection();

            return dt;
        }

        bool disposed = false;
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed)
                return;

            if (disposing)
            {
                if (_transaction != null)
                {
                    _transaction.Dispose();
                }

                if (_reader != null)
                {
                    if (!_reader.IsClosed)
                    {
                        _reader.Close();
                    }
                    _reader.Dispose();
                }

                if (_command != null)
                {
                    if (_command.Transaction != null)
                    {
                        _command.Transaction.Dispose();
                    }

                    if (_command.Connection != null)
                    {
                        _command.Connection.Close();
                        _command.Connection.Dispose();
                    }

                    _command.Dispose();
                }

                this.CloseConnection();
            }

            disposed = true;
        }

        ~DBProviderInformix()
        {
            Dispose(false);
        }


    }
}
