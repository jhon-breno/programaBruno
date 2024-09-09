using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Pangea.Swat.UI
{
    public class GeoProcess
    {
        /// <summary>
        /// Longitude e Latitude separados por vírgula.
        /// </summary>
        /// <param name="coordenadas"></param>
        /// <returns></returns>
        public static string GetEndereco(string coordenadas)
        {
            XmlDocument xDoc = new XmlDocument();
            XmlNodeList xNodelst = null;
            XmlNode xNode = null;
            try
            {
                string coordinate = coordenadas;

                xDoc.Load("https://maps.googleapis.com/maps/api/geocode/xml?latlng=" + coordinate);
                xNodelst = xDoc.GetElementsByTagName("result");
                xNode = xNodelst.Item(0);
                string FullAddress = xNode.SelectSingleNode("formatted_address").InnerText;
                string Number = xNode.SelectSingleNode("address_component[1]/long_name").InnerText;
                string Street = xNode.SelectSingleNode("address_component[2]/long_name").InnerText;
                string Village = xNode.SelectSingleNode("address_component[3]/long_name").InnerText;
                string Area = xNode.SelectSingleNode("address_component[4]/long_name").InnerText;
                string County = xNode.SelectSingleNode("address_component[5]/long_name").InnerText;
                string State = xNode.SelectSingleNode("address_component[6]/long_name").InnerText;
                string Zip = xNode.SelectSingleNode("address_component[8]/long_name").InnerText;
                string Country = xNode.SelectSingleNode("address_component[7]/long_name").InnerText;

                return Street;
            }
            catch (Exception ex)
            {
                string ab = string.Empty;
            }

            return string.Empty;
        }
    }
}
