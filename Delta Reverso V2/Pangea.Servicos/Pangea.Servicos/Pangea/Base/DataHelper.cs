using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

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
                    object DTO = (T)Activator.CreateInstance(typeof(T));
                    
                    foreach (PropertyInfo pi in type.GetProperties())
                    {
                        if (!resultDt.Columns.Contains(pi.Name)) continue;
                        if (DBNull.Value.Equals(dr[pi.Name]) || string.IsNullOrEmpty(dr[pi.Name].ToString())) continue;
                        
                        object valor = dr[pi.Name];
                        object valorTipado = Convert.ChangeType(valor, pi.PropertyType);

                        pi.SetValue(DTO, valorTipado, null);
                        pi.SetValue(DTO, valorTipado, null);
                    }

                    lista.Add((T)DTO);
                }
            }
            
            return lista;
        }

        public static T ConvertDataTableToDTO<T>(DataTable resultDt)
        {
            List<T> lista = new List<T>();
            lista = ConvertDataTableToList<T>(resultDt);
            return lista.First();
        }
    }
}
