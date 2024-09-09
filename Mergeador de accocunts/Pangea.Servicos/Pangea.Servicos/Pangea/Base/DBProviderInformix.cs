using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IBM.Data.Informix;
using System.Configuration;
using System.Data;

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
                _connection.Open();
                if (_command == null)
                    _command = _connection.CreateCommand();
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
        
        public bool ExecuteNonQuery()
        {
            OpenConnection();

            if (_command == null)
                throw new Exception(erroNoCommand);

            _command.ExecuteNonQuery();

            CloseConnection();
            
            
            return (true);
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

        public DataTable ExecuteDataTable()
        {
            DataTable dt = new DataTable();

            using (IDataReader reader = ExecuteReader())
            {
                dt.Load(reader);
            }

            CloseConnection();
            Dispose();
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
