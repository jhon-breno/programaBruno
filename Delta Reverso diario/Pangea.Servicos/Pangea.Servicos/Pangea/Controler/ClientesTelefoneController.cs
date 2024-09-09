using Pangea.Dados;
using Pangea.Entidades;
using Pangea.Util;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Pangea.Controler
{
    public class ClientesTelefoneController
    {

        public String verificarTipoTelefone(string numero_telefone)
        {
            string ret;

            if (numero_telefone.Length == 11 || numero_telefone.Length == 12)
            {
                var n = int.Parse(numero_telefone.Substring(3, 1));

                if (n >= 6)
                    ret = "03";
                else
                    ret = "01";
            }
            else
                ret = "";

            return ret;
        }

        public bool validarNumeroTelefone(string numero_telefone)
        {
            if (numero_telefone.Trim().Length == 10 || numero_telefone.Trim().Length == 11 || numero_telefone.Trim().All(ch => ch == numero_telefone[0]))
            {
                if (numero_telefone.Trim().Substring(0, 2).Equals("00") || numero_telefone.Trim().Substring(0, 2).Equals("10") || numero_telefone.Trim().Substring(0, 2).Equals("20") || numero_telefone.Trim().Substring(0, 2).Equals("30") || numero_telefone.Trim().Substring(0, 2).Equals("40") || numero_telefone.Trim().Substring(0, 2).Equals("40"))
                {
                    return false;
                }

                if (numero_telefone.Substring(2, numero_telefone.Length - 2).All(ch => ch == numero_telefone.Substring(2, numero_telefone.Length - 2)[0]))
                {
                    return false;
                }

                if (Convert.ToInt32(numero_telefone.Substring(2, numero_telefone.Length - 2).Substring(0, 1)) > 5 && numero_telefone.Substring(2, numero_telefone.Length - 2).Length < 9)
                {
                    return false;
                }
                else if (numero_telefone.Substring(2, numero_telefone.Length - 2).Length > 8 && Convert.ToInt32(numero_telefone.Substring(2, numero_telefone.Length - 2).Substring(0, 1)) != 9)
                {
                    return false;
                }
                else if (Convert.ToInt32(numero_telefone.Substring(2, numero_telefone.Length - 2).Substring(0, 1)) > 5 && Convert.ToInt32(numero_telefone.Substring(2, numero_telefone.Length - 2).Substring(1, 1)) < 6)
                {
                    return false;
                }
                else if (Convert.ToInt32(numero_telefone.Substring(2, numero_telefone.Length - 2).Substring(0, 1)) == 0 || Convert.ToInt32(numero_telefone.Substring(2, numero_telefone.Length - 2).Substring(0, 1)) == 1)
                {
                    return false;
                }
                else if (Convert.ToInt32(numero_telefone.Substring(2, numero_telefone.Length - 2).Substring(0, 1)) < 6 && numero_telefone.Substring(2, numero_telefone.Length - 2).Length > 8)
                {
                    return false;
                }

                //else if(Convert.ToInt32(numero_telefone.Substring(2, numero_telefone.Length - 2).Substring(0, 1)) > 5
            }
            else
            {
                return false;
            }
            return true;
        }

        public string pesquisaTelefoneBase(string param_NumeroDocumento, string tipo_documento, List<String> paramlstTelefones, string paramAcao, string empresa)
        {
            ClienteTelefoneDAO clienteDAO;

            String data_atual = DateTime.Now.ToShortDateString();

            if (empresa.Equals("AMPLA"))
                clienteDAO = new ClienteTelefoneDAO(Empresa.Ampla);
            else
                clienteDAO = new ClienteTelefoneDAO(Empresa.Coelce);

            StringBuilder sb = new StringBuilder();

            string dv = param_NumeroDocumento.Substring(param_NumeroDocumento.Length - 2, 2);
            string rut = param_NumeroDocumento.Substring(0, param_NumeroDocumento.Length - 2);// param_NumeroDocumento.Substring(0, param_NumeroDocumento.Length - 2);

            List<String> lstNumeroClientes = new List<string>();

            /*foreach (String telefone in paramlstTelefones)
            {
                if (validarNumeroTelefone(telefone))
                {
                   
                }

               
            }

            return "";*/

            if (tipo_documento.Equals("002"))
            {
                return "";
            }
            
            if (tipo_documento.Equals("numero_cliente"))
            {
                lstNumeroClientes.Add(param_NumeroDocumento);
            }
            else
            {
                lstNumeroClientes = clienteDAO.GetNumeroCliente(tipo_documento, rut, dv);
            }

            foreach (String numeroCliente in lstNumeroClientes)
            {
                List<ClientesTelefone> lstCliTelefones = clienteDAO.ListTelefones(numeroCliente);

                int tipo_tel = 6;

                foreach (String telefone in paramlstTelefones)
                {
                    if (validarNumeroTelefone(telefone))
                    {
                        string tipoTelefone = verificarTipoTelefone(telefone);

                        string DddTelefone = telefone.Substring(0, 2);
                        string NumeroTelefone = telefone.Substring(2, telefone.Length - 2);
                        string acaoDB = "";

                        List<ClientesTelefone> lstClientesComTel = lstCliTelefones.Where(p => p.numero_telefone == NumeroTelefone && p.prefixo_ddd == DddTelefone).ToList();

                        if (lstClientesComTel.Count.Equals(0))
                        {
                            ClientesTelefone cli = new ClientesTelefone();

                            cli.numero_cliente = numeroCliente;
                            cli.numero_telefone = NumeroTelefone;
                            cli.prefixo_ddd = DddTelefone;

                            //BLAW FEZ AO VIVO
                            if (tipoTelefone.Equals("03"))
                                cli.tipo_telefone = "03";
                            else
                                cli.tipo_telefone = tipo_tel.ToString().PadLeft(2, '0');

                            tipo_tel++;
                            lstCliTelefones.Add(cli);

                            if (paramAcao == "E")
                            {
                                acaoDB = "inserir";

                                if (clienteDAO.InsertTelefoneCliente(cli))
                                    sb.Append(numeroCliente + ";" + cli.prefixo_ddd + ";" + cli.numero_telefone + ";" + data_atual + " \r\n");
                            }
                            else
                            {
                                sb.Append(numeroCliente + ";" + DddTelefone + ";" + NumeroTelefone + ";" + data_atual + " \r\n");
                            }
                        }
                    }
                }
            }

            return sb.ToString();
        }

        internal string atualizarEmails(string param_NumeroDocumento, string tipo_documento, string email, string operacao, string empresa)
        {

            ClienteTelefoneDAO clienteDAO;

            if (empresa.Equals("AMPLA"))
                clienteDAO = new ClienteTelefoneDAO(Empresa.Ampla);
            else
                clienteDAO = new ClienteTelefoneDAO(Empresa.Coelce);

            StringBuilder sb = new StringBuilder();

            string dv = param_NumeroDocumento.Substring(param_NumeroDocumento.Length - 2, 2);
            string rut = param_NumeroDocumento.Substring(0, param_NumeroDocumento.Length - 2);// param_NumeroDocumento.Substring(0, param_NumeroDocumento.Length - 2);

            List<String> lstNumeroClientes = new List<string>();

            if (tipo_documento.Equals("002"))
            {
                if (param_NumeroDocumento !="20013155000250")
                    return "";
            }

            if (tipo_documento.Equals("numero_cliente"))
            {                
                lstNumeroClientes.Add(param_NumeroDocumento);
            }
            else
            {
                lstNumeroClientes = clienteDAO.GetNumeroCliente(tipo_documento, rut, dv);
            }

            foreach (String numeroCliente in lstNumeroClientes)
            {
                string email_antigo = clienteDAO.getEmail(numeroCliente);

                StringBuilder sql = new StringBuilder();
                StringBuilder sql1 = new StringBuilder();

                if(email_antigo.Trim().Equals(email.Trim()))
                {
                    continue;
                }

                try
                {

                    if (operacao == "E")
                    {
                        sql1.Append("UPDATE cliente set mail = '" + email.Trim() + "' where numero_cliente=" + numeroCliente + ";\n");

                        clienteDAO.InsertGenerico(sql1);

                        sql.Append(" INSERT INTO clientes@clientes:Modif ");
                        sql.Append(" (numero_cliente, ");
                        sql.Append("  Tipo_Orden     , ");
                        sql.Append("  Numero_Orden   , ");
                        sql.Append("  Ficha          , ");
                        sql.Append("  Fecha_Modif    , ");
                        sql.Append("  Tipo_Cliente   , ");
                        sql.Append("  Codigo_Modif   , ");
                        sql.Append("  Dato_Anterior  , ");
                        sql.Append("  Dato_Nuevo     , ");
                        sql.Append("  Proced ) ");
                        sql.Append(" VALUES ( ");
                        sql.Append(numeroCliente + ", ");
                        sql.Append("  'MOD',  ");
                        sql.Append("  0, ");
                        sql.Append("  '', ");
                        sql.Append("  Current, ");
                        sql.Append("  'A', ");
                        sql.Append("  '26', ");
                        sql.Append("  '" + email_antigo.Trim() + "', ");
                        sql.Append("  '" + email.Trim() + "', ");
                        sql.Append("  'DELTAREVERSO');\n ");

                        clienteDAO.InsertGenerico(sql);
                    }

                }
                catch
                {
                    Console.WriteLine("Cliente " + numeroCliente + " não atualizado!");
                    return "Cliente " + numeroCliente + " não atualizado!";
                }

                Console.WriteLine("Cliente " + numeroCliente + " email anterior " + email_antigo + " atualizado com email " + email + " para a empresa " + empresa + " !\n");
                sb.Append(numeroCliente + ";" + email_antigo + ";" + email +"\r\n");
            }

            return sb.ToString();
        }
    }
}
