using Pangea.Dados.Base;
using Pangea.Entidades;
using Pangea.Entidades.Enumeracao;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Text;
using Tamir.SharpSsh;

namespace Pangea.Dados
{
    public class DenunciaDAO :BaseDAO
    {
        public DenunciaDAO(Empresa empresa)
            : base(empresa)
        {
        }

        public String ExecutaProcessoC(string[] parametros, string EnderecoCompletoExecutavel, string Empresa)
        {
            try
            {
                String enderecoCompletoExecutavel = EnderecoCompletoExecutavel;
                StringBuilder comando = new StringBuilder();
                String ipServidor, login, senha;
                comando.AppendFormat("{0} ", enderecoCompletoExecutavel);

                for (int i = 0; i <= parametros.Count() - 1; i++)
                {
                    comando.Append(parametros[i] + " ");
                }

                if (Empresa.Equals("2003"))
                {
                    ipServidor = ConfigurationManager.AppSettings["ServidorComercial_Coelce"].ToString();
                    login = ConfigurationManager.AppSettings["LoginComercial_Coelce"].ToString();
                    senha = ConfigurationManager.AppSettings["SenhaComercial_Coelce"].ToString();

                }
                else
                {
                    ipServidor = ConfigurationManager.AppSettings["ServidorComercial_Ampla"].ToString();
                    login = ConfigurationManager.AppSettings["LoginComercial_Ampla"].ToString();
                    senha = ConfigurationManager.AppSettings["SenhaComercial_Ampla"].ToString();
                }

                Int32 portaSSH = Convert.ToInt32(ConfigurationManager.AppSettings["PortaSSH"].ToString());

                string output = String.Empty;
                SshShell ssh = new SshShell(ipServidor, login, senha);
                ssh.Connect(portaSSH);
                ssh.ExpectPattern = ">";
                ssh.RemoveTerminalEmulationCharacters = true;
                ssh.Expect(">");

                if (ssh.ShellOpened)
                {
                    ssh.WriteLine(comando.ToString());
                    output = ssh.Expect(">");
                }
                ssh.Close();

                return output;
                
            }
            catch (Exception ex)
            {

                return "";
            }
            
        }

        public DataTable consultaCliente (int numero_cliente)
        {
            string sql = String.Format(@"SELECT c.dv_numero_cliente,                          
                                                 c.nombre,                                   
                                                 c.direccion,                                
                                                 c.municipio, loc.descripcion,               
                                                 c.localidade,                               
                                                 c.sector,                                   
                                                 c.zona,                                     
                                                 c.correlativo_ruta, c.dv_ruta_lectura,      
                                                c.sucursal,                    
                                                cn.num_cliente 
        
                                                 FROM cliente c, 
                                                      OUTER localidades loc,                       
                                                      OUTER ordem_norm cn        
        
        
                                                 WHERE c.numero_cliente = {0}
                                                 AND loc.municipio = c.municipio 
                                                 AND loc.localidad = loc.municipio * 10           
                                                 AND cn.num_cliente = c.numero_cliente         
                                                 AND cn.cod_situacao not in ('F','C','E') ", numero_cliente);

            var dt = ConsultaSql(sql);

            return dt;
        }

        public bool existeDenuncia(int numCliente)
        {
            bool retorno;
            string sql = String.Format(@"SELECT o.num_cliente
                                                FROM ordem_norm o
                                                WHERE o.num_cliente ={0}
                                                AND o.cod_situacao not in ('F','C','E') ",numCliente);
            var dt = ConsultaSql(sql);

            if (dt.Rows.Count > 0)
                retorno = false;
            else
                retorno = true;

            return retorno;
        }
    }
}
