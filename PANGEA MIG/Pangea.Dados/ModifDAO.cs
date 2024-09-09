using Entidades.DTO;
using Pangea.Dados.Base;
using Pangea.Entidades;
using Pangea.Entidades.Enumeracao;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pangea.Dados
{
    public class ModifDAO : BaseDAO
    {
        public ModifDAO(Empresa empresa)
            : base(empresa)
        {
        }

        public bool InsereModif(ModifDTO modifDto, DBProviderInformix informix)
        {            
            string sql = string.Format(@"INSERT INTO Modif
                                          (Numero_Cliente ,
                                           Tipo_Orden     ,
                                           Numero_Orden   ,
                                           Ficha          ,
                                           Fecha_Modif    ,
                                           Tipo_Cliente   ,
                                           Codigo_Modif   ,
                                           Dato_Anterior  ,
                                           Dato_Nuevo     ,
                                           Proced         ,
                                           Dir_Ip         ,
                                           Motivo) 
                                           VALUES ({0},'{1}','{2}','{3}',current,'{4}','{5}','{6}','{7}','{8}','{9}','{10}')"
                                           ,modifDto.numero_cliente
                                           ,modifDto.tipo_ordem
                                           ,modifDto.numero_ordem
                                           ,modifDto.ficha                                           
                                           ,modifDto.tipo_cliente
                                           ,modifDto.codigo_modif
                                           ,modifDto.dato_anterior
                                           ,modifDto.Dato_Nuevo
                                           ,modifDto.proced
                                           ,modifDto.dir_ip
                                           ,modifDto.motivo);
            
            return ExecutarSql(sql.ToString(), informix);
        }

//        public bool InsereModif(ModifDTO modifDto, DBProviderInformix informix)
//        {
//            string sql = string.Format(@"INSERT INTO Modif
//                                          (Numero_Cliente ,
//                                           Tipo_Orden     ,
//                                           Numero_Orden   ,
//                                           Ficha          ,
//                                           Fecha_Modif    ,
//                                           Tipo_Cliente   ,
//                                           Codigo_Modif   ,
//                                           Dato_Anterior  ,
//                                           Dato_Nuevo     ,
//                                           Proced         ,
//                                           Dir_Ip         ,
//                                           Motivo) 
//                                           VALUES ('{0}','{1}','{2},'{3}',{4},'{5}','{6}','{7}','{8}','{9}','{10}','{11}'"
//                                           , modifDto.numero_cliente
//                                           , modifDto.tipo_ordem
//                                           , modifDto.numero_ordem
//                                           , modifDto.ficha
//                                           , modifDto.fecha_Modif
//                                           , modifDto.tipo_cliente
//                                           , modifDto.codigo_modif
//                                           , modifDto.dato_anterior
//                                           , modifDto.Dato_Nuevo
//                                           , modifDto.proced
//                                           , modifDto.dir_ip
//                                           , modifDto.motivo);

//            return ExecutarSql(sql.ToString(), informix);
//        }
    }
}
