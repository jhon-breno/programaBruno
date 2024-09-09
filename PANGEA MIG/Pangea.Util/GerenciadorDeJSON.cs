using Newtonsoft.Json;
using System;
using System.Text;

namespace Pangea.Util
{
    public static class GerenciadorDeJSON
    {
        public static String RemoverCamposNulos(String json)
        {
            #region TRATAMENTO PARA JSON COMPLEXO
            if (json.Split('{').Length > 2)
                return RemoverCamposNulosDeJsonComplexo(json);
            #endregion TRATAMENTO PARA JSON COMPLEXO

            String[] campos = json.Split(',');
            StringBuilder jsonRetorno = new StringBuilder();

            foreach (String item in campos)
            {
                if (!item.ToUpper().Contains("NULL"))
                {
                    jsonRetorno.Append(jsonRetorno.Length > 0 ? ", " : "");
                    jsonRetorno.Append(item);
                }
                else if (item.Contains("null"))
                {
                    jsonRetorno.Append(item.Substring(item.IndexOf("null") + 4));
                }
                else if (item.Contains("NULL"))
                {
                    jsonRetorno.Append(item.Substring(item.IndexOf("NULL") + 5));
                }
            }

            if (!Char.Equals(jsonRetorno[0], '{'))
                jsonRetorno.Insert(0, '{');
            if (!Char.Equals(jsonRetorno[jsonRetorno.Length -1], '}'))
                jsonRetorno.Insert(jsonRetorno.Length, '}');

            //return JsonConvert.SerializeObject(jsonRetorno.ToString(), Formatting.Indented);
            return jsonRetorno.ToString();
        }

        public static String RemoverCamposNulosDeJsonComplexo(String json)
        {
            return json.Replace(",",", ");
        }

        public static bool ValidarJSON(String json)
        {
            try { var obj = JsonConvert.DeserializeObject(json); }
            catch { return false; }

            return true;
        }
    }
}