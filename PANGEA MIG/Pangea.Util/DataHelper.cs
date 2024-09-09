using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;

namespace Pangea.Util
{
    public class DataHelper
    {
        public static List<T> ConvertDataTableToList<T>(DataTable resultDt)
        {
            List<T> lista = new List<T>();

            if (resultDt != null && resultDt.Rows.Count > 0)
            {
                Type type = typeof(T);
                foreach (DataRow dr in resultDt.Rows)
                {
                    object entity = (T)Activator.CreateInstance(typeof(T));
                    foreach (PropertyInfo pi in type.GetProperties())
                    {
                        if (!resultDt.Columns.Contains(pi.Name)) continue;
                        if (DBNull.Value.Equals(dr[pi.Name]) || string.IsNullOrWhiteSpace(dr[pi.Name].ToString())) continue;

                        object valor = dr[pi.Name].GetType() == typeof(TimeSpan) ? dr[pi.Name].ToString() : dr[pi.Name];
                        object valorTipado = null;
                        if (pi.PropertyType.IsGenericType && pi.PropertyType.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
                        {
                            if (valor == null)
                            {
                                valorTipado = default(T);
                            }

                            type = Nullable.GetUnderlyingType(pi.PropertyType);
                            valorTipado = Convert.ChangeType(valor, type);
                        }
                        else
                            valorTipado = Convert.ChangeType(valor, pi.PropertyType);

                        //Tratamento de Espaços no final
                        if (valorTipado.GetType() == typeof(string))
                            valorTipado = valor.ToString().Trim();
                            

                        pi.SetValue(entity, valorTipado, null);
                    }

                    lista.Add((T)entity);
                }
            }

            return lista;
        }

        public static T ConvertDataTableToEntity<T>(DataTable resultDt)
        {
            List<T> lista = new List<T>();
            lista = ConvertDataTableToList<T>(resultDt);
            return lista.Count > 0 ? lista.First() : Activator.CreateInstance<T>();
        }

        //As propriedades da classe T precisam estar na mesma ordem que vêm no arquivo.
        public static T ConvertStringToEntity<T>(string strLine, char separator)
        {
            Type type = typeof(T);
            object DTO = (T)Activator.CreateInstance(typeof(T));
            
            string[] arrayDTO = strLine.Split(separator);
            int index = 0;

            foreach (PropertyInfo pi in type.GetProperties())
            {
                object valor = arrayDTO[index];
                object valorTipado = Convert.ChangeType(valor, pi.PropertyType);

                pi.SetValue(DTO, valorTipado);

                index++;
                if (arrayDTO.Length <= index)
                    break;
            }

            return (T)DTO;
        }

        public static String GetConstantName<T>(object constValue)
        {
            Type type = typeof(T);
            foreach (var pi in type.GetFields())
            {
                object a = new object();
                if (pi.GetValue(a).Equals(constValue))
                    return pi.Name;
                else continue;
            }

            return String.Empty;
        }
    }
}