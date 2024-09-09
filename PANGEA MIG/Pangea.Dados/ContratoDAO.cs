using Entidades.DTO;
using Pangea.Dados.Base;
using Pangea.Entidades;
using Pangea.Entidades.DTO;
using Pangea.Entidades.Enumeracao;
using Pangea.Util;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pangea.Dados
{
    public class ContratoDAO : BaseDAO
    {
        public ContratoDAO(Empresa empresa)
            : base(empresa)
        {

        }

        public int verificaAcda(ContratoDTO contrato)
        {
            int result = 0;

            string sql = string.Format(@"Select count(*)                                            
										 From acda
										 Where llave = 63747 
											And numero_cliente = {0}
											And fecha is null;", contrato.numero_cliente);

            DataTable dt = ConsultaSql(sql.ToString());

            if (dt.Rows.Count > 0)
            {
                result = Convert.ToInt32(dt.Rows[0][0]);
                return result;
            }

            else
                return result;
        }

        public bool AtualizaAcda(ContratoDTO contrato, DBProviderInformix informix)
        {
            string sql = string.Format(@"Update acda set fecha = today
											Where llave = 63747
											And   numero_cliente;", contrato.numero_cliente);

            return ExecutarSql(sql.ToString(), informix);
        }

        //        public bool insertModif(ContratoDTO contrato, DBProviderInformix informix, string numero_ordem,string data)
        //        {
        //            string sql = string.Format(@"INSERT INTO modif (numero_cliente, 
        //                                                            tipo_orden, 
        //                                                            numero_orden, 
        //                                                            ficha,
        //                                                            fecha_modif, 
        //                                                            tipo_cliente, 
        //                                                            codigo_modif, 
        //                                                            dato_anterior,
        //                                                            dato_nuevo, 
        //                                                            proced, 
        //                                                            dir_ip, 
        //                                                            motivo)
        //                                                  VALUES (, {0}
        //                                                          , 'MOD'
        //                                                          , '{1}'
        //                                                          ,'{2}'
        //                                                          , current
        //                                                          , 'B'
        //                                                          , '195'
        //                                                          , ''
        //                                                          , '{3}'
        //                                                          ,'TROCTITU'
        //                                                          , ''
        //                                                          ,'DESATIVACAO CLIENTE RES 195'",contrato.numero_cliente,numero_ordem,contrato.rol_creacion,data);

        //            return ExecutarSql(sql.ToString(), informix);

        //        }

        public TecniDTO RetornaTecni(ContratoDTO contrato)
        {
            string sql = string.Format(@"select numero_cliente,
											  ult_fec_mant_empal,
											  codigo_voltaje,
											  nro_subestacion,
											  tipo_subestacion,
											  numero_equipo,
											  tipo_contrato,
											  tipo_tranformador,
											  propiedad_trafo,
											  tension_suministro,
											  caja_empalme,
											  conductor_empalme,
											  acometida_retirada,
											  ult_tipo_contrato,
											  nro_ult_contrato,
											  fecha_ult_contrato,
											  fecha_ult_aplicac,
											  numero_contrato,
											  nro_orden_conexion,
											  fecha_conexion,
											  nro_sol_servicio,
											  dv_sol_servicio,
											  aux_sol_servicio,
											  mod_tipo_orden,
											  mod_nro_orden,
											  subestac_transmi,
											  alimentador,
											  subestac_distrib,
											  llave,
											  numero_poste,
											  dist_poste_med,
											  medida_tecnica,
											  trilha,
											  fase_conexion,
											  cod_transformador,
											  fecha_parafuso,
											  coordenada_lat_gps,
											  coordenada_lon_gps,
											  fase,
											  pto_transf,
											  ind_local_med,
											  ind_padrao_agrup,
											  nro_caixa,
											  nro_caixa_tampa,
											  tipo_caixa_med,
											  coord_lat_gps_lida,
											  coord_lon_gps_lida,
											  data_atu_coord_sda,
											  coord_lat_trafo,
											  coord_lon_trafo,
											  utmx,
											  utmy,
											  ind_cred_medid,
											  dispositivo_seguranca 
										FROM tecni                                        
										where numero_cliente ={0}", contrato.numero_cliente);

            var dt = ConsultaSql(sql.ToString());
            if (dt.Rows.Count > 0)
            {
                TecniDTO resultado = DataHelper.ConvertDataTableToEntity<TecniDTO>(dt);

                return (TecniDTO)resultado;
            }
            else
                return null;

        }

        public bool insertTecni(TecniDTO tecni, DBProviderInformix informix)
        {
            string sql = string.Format(@"insert into tecni
													( numero_cliente,
													  ult_fec_mant_empal,
													  codigo_voltaje,
													  nro_subestacion,
													  tipo_subestacion,
													  numero_equipo,
													  tipo_contrato,
													  tipo_tranformador,
													  propiedad_trafo,
													  tension_suministro,
													  caja_empalme,
													  conductor_empalme,
													  acometida_retirada,
													  ult_tipo_contrato,
													  nro_ult_contrato,
													  fecha_ult_contrato,
													  fecha_ult_aplicac,
													  numero_contrato,
													  nro_orden_conexion,
													  fecha_conexion,
													  nro_sol_servicio,
													  dv_sol_servicio,
													  aux_sol_servicio,
													  mod_tipo_orden,
													  mod_nro_orden,
													  subestac_transmi,
													  alimentador,
													  subestac_distrib,
													  llave,
													  numero_poste,
													  dist_poste_med,
													  medida_tecnica,
													  trilha,
													  fase_conexion,
													  cod_transformador,
													  fecha_parafuso,
													  coordenada_lat_gps,
													  coordenada_lon_gps,
													  fase,
													  pto_transf,
													  ind_local_med,
													  ind_padrao_agrup,
													  nro_caixa,
													  nro_caixa_tampa,
													  tipo_caixa_med,
													  coord_lat_gps_lida,
													  coord_lon_gps_lida,
													  data_atu_coord_sda,
													  coord_lat_trafo,
													  coord_lon_trafo,
													  utmx,
													  utmy,
													  ind_cred_medid,
													  dispositivo_seguranca) Values 
														({0},'{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}','{12}','{13}','{14}','{15}','{16}','{17}','{18}','{19}','{20}','{21}','{22}','{23}','{24}','{25}','{26}','{27}','{28}','{29}','{30}','{31}','{32}','{33}','{34}','{35}','{36}','{37}','{38}','{39}','{40}','{41}','{42}','{43}','{44}','{45}','{46}','{47}','{48}','{49}','{50}','{51}','{52}','{53}')"
                                                      , tecni.numero_cliente
                                                      , tecni.ult_fec_mant_empal
                                                      , tecni.codigo_voltaje
                                                      , tecni.nro_subestacion
                                                      , tecni.tipo_subestacion
                                                      , tecni.numero_equipo
                                                      , tecni.tipo_contrato
                                                      , tecni.tipo_tranformador
                                                      , tecni.propiedad_trafo
                                                      , tecni.tension_suministro
                                                      , tecni.caja_empalme
                                                      , tecni.conductor_empalme
                                                      , tecni.acometida_retirada
                                                      , tecni.ult_tipo_contrato
                                                      , tecni.nro_ult_contrato
                                                      , tecni.fecha_ult_contrato
                                                      , tecni.fecha_ult_aplicac
                                                      , tecni.numero_contrato
                                                      , tecni.nro_orden_conexion
                                                      , tecni.fecha_conexion
                                                      , tecni.nro_sol_servicio
                                                      , tecni.dv_sol_servicio
                                                      , tecni.aux_sol_servicio
                                                      , tecni.mod_tipo_orden
                                                      , tecni.mod_nro_orden
                                                      , tecni.subestac_transmi
                                                      , tecni.alimentador
                                                      , tecni.subestac_distrib
                                                      , tecni.llave
                                                      , tecni.numero_poste
                                                      , tecni.dist_poste_med
                                                      , tecni.medida_tecnica
                                                      , tecni.trilha
                                                      , tecni.fase_conexion
                                                      , tecni.cod_transformador
                                                      , tecni.fecha_parafuso
                                                      , tecni.coordenada_lat_gps
                                                      , tecni.coordenada_lon_gps
                                                      , tecni.fase
                                                      , tecni.pto_transf
                                                      , tecni.ind_local_med
                                                      , tecni.ind_padrao_agrup
                                                      , tecni.nro_caixa
                                                      , tecni.nro_caixa_tampa
                                                      , tecni.tipo_caixa_med
                                                      , tecni.coord_lat_gps_lida
                                                      , tecni.coord_lon_gps_lida
                                                      , tecni.data_atu_coord_sda
                                                      , tecni.coord_lat_trafo
                                                      , tecni.coord_lon_trafo
                                                      , tecni.utmx
                                                      , tecni.utmy
                                                      , tecni.ind_cred_medid
                                                      , tecni.dispositivo_seguranca);

            return ExecutarSql(sql.ToString(), informix);
        }

        public string ExecutaProcedureTrocaMedidorTitularidade(ContratoDTO contrato, DBProviderInformix informix)
        {
            string resultado = string.Empty;
            string sql = string.Format("EXECUTE PROCEDURE sp_troca_medidor_titularidade({0},{1})", contrato.numero_cliente, contrato.numero_cliente_novo);

            var dt = ConsultaSql(sql.ToString(),informix);
            if (dt.Rows.Count > 0)
            {
                resultado = dt.Rows[0][0].ToString();
            }

            return resultado;
        }

        public bool AtualizaCliMedProcesso(ContratoDTO contrato, DBProviderInformix informix)
        {
            string sql = string.Format(@" update cli_med_processo
											set numero_cliente = {0}
										  where numero_cliente = {1}
											and ja_processado = 'N'", contrato.numero_cliente_novo, contrato.numero_cliente);

            return ExecutarSql(sql.ToString(), informix);
        }

        public int ConsultaUtiDomiciliar(ContratoDTO contrato)
        {
            int resultado = 0;
            string sql = string.Format(@"Select Count(*)	                                       
										   From  uti_domiciliar
										  Where numero_cliente = {0}
											And (dt_desativacao is null or dt_desativacao = '')", contrato.numero_cliente);

            DataTable dt = ConsultaSql(sql.ToString());

            if (dt.Rows.Count > 0)
            {
                resultado = Convert.ToInt32(dt.Rows[0][0]);
                return resultado;
            }

            else
                return resultado;

        }

        public bool AtualizautiDomiciliar(ContratoDTO contrato, DBProviderInformix informix)
        {
            string sql = string.Format(@"update uti_domiciliar
												set dt_desativacao 	= TODAY,
													motivo 			= 'TROCA DE TITULARIDADE'
											  where numero_cliente 	=", contrato.numero_cliente);

            return ExecutarSql(sql.ToString(), informix);
        }

        public bool AtualizarClienteInsentoTip(ContratoDTO contrato, DBProviderInformix informix)
        {
            string sql = string.Format(@"update cliente_isento_TIP set isencao_atual = 'N'
										where numero_cliente = {0};", contrato.numero_cliente);

            return ExecutarSql(sql.ToString(), informix);
        }

        public DataTable ConsultaSolucoesCliente(ContratoDTO contrato)
        {
            string sql = string.Format(@"select sc.empresa_parc, sc.cod_produto, sp.codigo_facilidad, sp.tipo_cobranca, sp.cod_encargo_cob 
										 from solucoes_cliente sc, solucoes_produto sp 
										 where sc.numero_cliente ={0}
										 AND sc.empresa_parc = sp.empresa_parc 
										 AND sc.cod_produto = sp.cod_produto
										 AND sp.fecha_desactivac is null 
										 AND sc.estado = '2'", contrato.numero_cliente);

            var dt = ConsultaSql(sql.ToString());

            return dt;

        }

        public bool AtualizaFacCli(ContratoDTO contrato, string codigo_facilidad, DBProviderInformix informix)
        {
            string sql = string.Format(@"UPDATE fac_cli SET fecha_termino = today, usuario_termino = 'RETIRO', estado = 'C'
										  WHERE numero_cliente = {0} 
										  AND codigo_facilidad = '{1}'
										  AND estado = 'V'", contrato.numero_cliente, codigo_facilidad);

            return ExecutarSql(sql.ToString(), informix);
        }

        public bool AtualizarSolucoesCliente(ContratoDTO contrato, string empresa_parc, string cod_produto, DBProviderInformix informix)
        {
            string sql = string.Format(@"UPDATE solucoes_cliente SET estado = '9', fecha_exclusao = today
										 WHERE numero_cliente = {0} 
										 AND empresa_parc = '{1}'
										 AND cod_produto = '{2}' 
										 AND fecha_exclusao is null", contrato.numero_cliente, empresa_parc, cod_produto);

            return ExecutarSql(sql.ToString(), informix);
        }

        public bool AtualizaSolucoesEncargos(string empresa_parc, string cod_produto, ContratoDTO contrato, string codigo_cargo, DBProviderInformix informix)
        {
            string sql = string.Format(@"UPDATE solucoes_encargos SET estado = 'C' , fecha_termino = today
										  WHERE empresa_parc = '{0}'
											AND cod_produto = '{1}' 
											AND numero_cliente = '{2}'
											AND estado = 'V' 
											AND codigo_cargo = '{3}'", empresa_parc, cod_produto, contrato.numero_cliente, codigo_cargo);

            return ExecutarSql(sql.ToString(), informix);
        }

        public bool InsereSolucoesOcorr(string empresa_parc, string cod_produto, ContratoDTO contrato, string cod_ocorr, string rol_inclusao, DBProviderInformix informix)
        {
            string sql = string.Format(@"INSERT INTO solucoes_ocorr(empresa_parc,cod_produto,numero_cliente,cod_ocorr,fecha_inclusao,rol_inclusao)
															 VALUES('{0}','{1},'{2}','{3}','today','{5}')", empresa_parc, cod_produto, contrato.numero_cliente, cod_ocorr, rol_inclusao);

            return ExecutarSql(sql.ToString(), informix);
        }

        public DataTable RetornaSeguroCliente(ContratoDTO contrato)
        {
            string sql = string.Format(@"select codigo_produto 
										 from seguro_cliente 
										 where (data_exclusao is null or data_exclusao = '') 
										 and numero_cliente = {0}", contrato.numero_cliente);

            var dt = ConsultaSql(sql.ToString());

            return dt;
        }

        public bool AtualizaSeguroCliente(string codigo_produto, ContratoDTO contrato, DBProviderInformix informix)
        {
            string sql = string.Format(@"Update seguro_cliente set data_exclusao = today, motivo_exclusao = '20'
										where codigo_produto = '{0}' 
										and numero_cliente = {1}
										and (data_exclusao is null or data_exclusao = '')",codigo_produto,contrato.numero_cliente);

            return ExecutarSql(sql.ToString(), informix);
        }

        public bool insertSeguroOcorr(ContratoDTO contrato, DBProviderInformix informix)
        {
            string sql = string.Format(@"insert INTO seguro_ocorr(numero_cliente,codigo_ocorr,data_inclusao) values ({0},'20',today)", contrato.numero_cliente);

            return ExecutarSql(sql.ToString(), informix);
        }

        public bool InsereCuadraSaldo(ClienteDTO cliente, double cua_saldo_afecto, double cua_saldo_noafecto, double cua_intereses, double cua_multas, string tipo_movimiento, DBProviderInformix informix)
        {
            string sql = string.Format(@"insert into    cuadra_saldo (
														sucursal,
														sector,
														localidade,
														zona,
														cant_clientes,
														saldo_afecto,
														saldo_noafecto,
														Intereses,
														multas,
														tipo_movimiento,
														fecha_movimiento) values 
														('{0}','{1}','{2}','{3}',-1,{4},{5},{6},{7},'{8}',current)", cliente.sucursal, cliente.sector, cliente.localidade, cliente.zona, cua_saldo_afecto, cua_saldo_noafecto, cua_intereses, cua_multas, tipo_movimiento);

            return ExecutarSql(sql.ToString(), informix);

        }

        public bool InsertCuadraSaldoRet(ClienteDTO cliente, string tipo_movimiento, DBProviderInformix informix)
        {
            string sql = string.Format(@"insert into cuadra_saldo_ret (
													sucursal,
													sector,
													localidade,
													zona,
													cant_clientes,
													saldo_afecto,
													saldo_noafecto,
													intereses,
													multas,
													tipo_movimiento,
													fecha_movimiento)
												  values ('{0}','{1}','{2}','{3}',-1,{4},{5},{6},{7},'{8}',current)", cliente.sucursal, cliente.sector, cliente.localidade, cliente.zona, cliente.saldo_afecto, cliente.saldo_noafecto, cliente.intereses, cliente.multas, tipo_movimiento);

            return ExecutarSql(sql.ToString(), informix);
        }

        public bool InsertClire(ClienteDTO cliente, string numOrdem, ContratoDTO contrato, DBProviderInformix informix)
        {
            string sql = string.Format(@"insert into clire (
												numero_cliente,
												dv_numero_cliente,
												codigo_retiro,
												fecha_retiro,
												numero_orden,
												numero_mensaje,
												rol_retiro,
												saldo_afecto,
												saldo_noafecto,
												intereses,
												multas,
												deuda_convenida,
												tipo_ident,
												rut,
												antiguedad_saldo)
											  values ('{0}','{1}','TITUL',current,'{2}',0,'{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}')"
                                                        , contrato.numero_cliente
                                                        , cliente.dv_numero_cliente
                                                        , numOrdem
                                                        , contrato.rol_creacion
                                                        , cliente.saldo_afecto
                                                        , cliente.saldo_noafecto
                                                        , cliente.intereses
                                                        , cliente.multas
                                                        , cliente.deuda_convenida
                                                        , cliente.tipo_ident
                                                        , cliente.rut
                                                        , cliente.antiguedad_saldo);

            return ExecutarSql(sql.ToString(), informix);
        }

        public DataTable BuscaIndicadores(string tarifa)
        {
            string sql = string.Format(@"select tiene_cobro_int,
												tiene_cobro_corte,
												tiene_corte_rest,
												tiene_at_med_bt,
												tiene_calma       
										   from tarifa
										  where tarifa ='{0}'", tarifa);


            var dt = ConsultaSql(sql.ToString());

            return dt;
        }

        public int RetornaMaxLectuAgrup(string sector, string localidade, string zona)
        {
            int retorno = 0;
            string sql = string.Format(@"SELECT max (referencia) as referencia			
												from lectu_agrup
												WHERE sector={0}
												AND localidade={1}
												AND zona={2}", sector, localidade, zona);

            var dt = ConsultaSql(sql.ToString());
            if (dt.Rows.Count > 0)
            {
                retorno = Convert.ToInt32(dt.Rows[0]["referencia"]);
            }

            return retorno;

        }

        public bool InsereControlTrocaTituFat(string cliente_anterior, string sector, int numero_cliente, int referemcia_fat, DBProviderInformix informix)
        {

            string sql = string.Format(@"INSERT INTO control_troca_titu_fat
													(numero_cliente_ant,
													lote_real,
													numero_cliente_novo,
													referencia,
													ind_processado)
													VALUES({0},{1},{2},{3},'N')", cliente_anterior, sector, numero_cliente, referemcia_fat);

            return ExecutarSql(sql.ToString(), informix);
        }

        public bool InsereCuadraSaldo(string sucursal, string sector, string localidade, string zona)
        {
            DBProviderInformix informix = ObterProviderInformix();

            string sql = string.Format(@"insert into cuadra_saldo (
													sucursal,
													sector,
													localidade,
													zona,
													saldo_afecto,
													saldo_noafecto,
													intereses,
													multas,
													cant_clientes,
													tipo_movimiento,
													fecha_movimiento)
													values ('{0}',{1},{2},{3},0,0,0,0,1,'NU',current)", sucursal, sector, localidade, zona);

            return ExecutarSql(sql.ToString(), informix);
        }

        public bool InsereHifco(HifcoDTO hifco, DBProviderInformix informix)
        {
            string sql = string.Format(@"INSERT INTO hifco (
													numero_cliente,
													corr_facturacion,
													fecha_lectura,
													fecha_facturacion,
													fecha_vencimiento,
													clave_lectura,
													antiguedad_saldo,
													tipo_docto_asocia,
													nro_docto_asocia,
													suma_importe,
													suma_intereses,
													suma_convenio,
													suma_impuestos,
													suma_cargos_man,
													suma_corte_repos,
													saldo_afecto_ant,
													saldo_noafec_ant,
													intereses_ant,
													mal_factor_pot,
													porc_rec_malfac,
													multas_ant,
													total_facturado,
													tiene_cobro_int,
													indica_refact,
													fact_calma,
													saldo_moroso,
													saldo_mora,
													suma_moras
													)
												  VALUES
													({0},
													'{1}',
													'{2}',
													'{3}',
													'{4}',
													'{5}',
													'{6}',
													'{7}',
													'{8}',
													'{9}',
													'{10}',
													'{11}',
													'{12}',
													'{13}',
													'{14}',
													'{15}',
													'{16}',
													'{17}',
													'{18}',
													'{19}',
													'{20}',
													'{21}',
													'{22}',
													'{23}',
													'{24}',
													'{25}',
													'{26}',
													'{27}')", hifco.numero_cliente
                                                            , hifco.corr_facturacion
                                                            , hifco.fecha_lectura
                                                            , hifco.fecha_facturacion
                                                            , hifco.fecha_vencimiento
                                                            , hifco.clave_lectura
                                                            , hifco.antiguedad_saldo
                                                            , hifco.tipo_docto_asocia
                                                            , hifco.nro_docto_asocia
                                                            , hifco.suma_importe
                                                            , hifco.suma_intereses
                                                            , hifco.suma_convenio
                                                            , hifco.suma_impuestos
                                                            , hifco.suma_cargos_man
                                                            , hifco.suma_corte_repos
                                                            , hifco.saldo_afecto_ant
                                                            , hifco.saldo_noafec_ant
                                                            , hifco.intereses_ant
                                                            , hifco.mal_factor_pot
                                                            , hifco.porc_rec_malfac
                                                            , hifco.multas_ant
                                                            , hifco.total_facturado
                                                            , hifco.tiene_cobro_int
                                                            , hifco.indica_refact
                                                            , hifco.fact_calma
                                                            , hifco.saldo_moroso
                                                            , hifco.saldo_mora
                                                            , hifco.suma_moras);

            return ExecutarSql(sql.ToString(), informix);
        }

        public string RetornaIdUnicoDisplay(string numero_cliente)
        {
            string retorno = string.Empty;

            string sql = string.Format(@"select id_unico from smc_associa_display where numero_cliente = {0};", numero_cliente);

            var dt = ConsultaSql(sql.ToString());
            if (dt.Rows.Count > 0)
            {
                retorno = dt.Rows[0]["id_unico"].ToString();
            }

            return retorno;
        }

        public bool InsereDisplay(string id_display, string codigo_cp, string codigo_cs, string estado, string accion, string obs, string numero_novo)
        {
            string sql = string.Format(@"INSERT INTO SMC_MOVIMENTA_DISPLAY (id_unico, codigo_cp, codigo_cs, estado,
										accion, obs, numero_cliente, data_movimentacao) VALUES('{0}','{1}','{2}','{3}','{4}','{5}',{6},TODAY)", id_display, codigo_cp, codigo_cs, estado, accion, obs, numero_novo);

            DBProviderInformix informix = ObterProviderInformix();
            return ExecutarSql(sql.ToString(), informix);
        }

        public bool AtualizaDisplay(string numero_cliente_ant, string numero_cliente_nov)
        {
            string sql = string.Format(@"UPDATE smc_associa_display set numero_cliente = {0} where numero_cliente = {1};", numero_cliente_nov, numero_cliente_ant);

            DBProviderInformix informix = ObterProviderInformix();
            return ExecutarSql(sql.ToString(), informix);
        }

        public bool InsertV2Comunica(string codigo_cp, string codigo_cs, string codigo_ps, string estado, string comando, int numero_cliente, string numero_ordem)
        {
            string sql = string.Format(@"insert into v2_comunica (codigo_cp,codigo_cs,codigo_ps,estado,comando,data_ingresso,numero_cliente,numero_ordem) values ('{0}','{1}','{2}','{3}','{4}',current,'{5}','{6}')", codigo_cp, codigo_cs, codigo_ps, estado, comando, numero_cliente, numero_ordem);

            DBProviderInformix informix = ObterProviderInformix();
            return ExecutarSql(sql.ToString(), informix);
        }

        public double RetornaRacMediaNovos(string classe, string subclasse, string tipo_ligacao, string municipio)
        {
            double retorno = 0;

            string sql = string.Format(@"select consumo 
										 from rac_media_novos
										 where classe = '{0}' 
										 and subclasse = '{1}' 
										 and tipo_ligacao = '{2}'
										 and municipio = '{3}'", classe, subclasse, tipo_ligacao, municipio);

            var dt = ConsultaSql(sql.ToString());
            if (dt.Rows.Count > 0)
            {
                retorno = Convert.ToDouble(dt.Rows[0]["consumo"]);
            }

            return retorno;
        }

        public double RetornaPercentualReducao(string classe)
        {
            double retorno = 0;
            string sql = string.Format(@"select valor 
										   from tabla
										  where nomtabla = 'RACRED' and
												sucursal = '0000' and
												codigo = '{0}';", classe);

            var dt = ConsultaSql(sql.ToString());
            if (dt.Rows.Count > 0)
            {
                retorno = Convert.ToDouble(dt.Rows[0]["valor"]);
            }

            return retorno;
        }

        public bool InsereRacConsumo(string data_referencia, string numero_cliente, string classe, string subclasse, string cod_atividade, string carga_instalada, double consumo_media, string ind_pendencia, string permite_corte, string observ, string usuario, string referencia, double quota_consumo, double meta_mes, string tipo_ligacao, DBProviderInformix informix)
        {
            string sql = string.Format(@"insert into rac_consumos (data_referencia, 
																   numero_cliente, 
																   classe,
																   subclasse, 
																   cod_atividade, 
																   carga_instalada,
																   consumo_media, 
																   ind_pendencia, 
																   permite_corte,
																   observ, 
																   usuario, 
																   referencia, 
																   quota_consumo,
																   meta_mes, 
																   tipo_ligacao)
														   values ('{0}','{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}','{9}','{10}','{11}','{12}','{13}','{14}')"
                                                                    , data_referencia
                                                                    , numero_cliente
                                                                    , classe
                                                                    , subclasse
                                                                    , cod_atividade
                                                                    , carga_instalada
                                                                    , consumo_media
                                                                    , ind_pendencia
                                                                    , permite_corte
                                                                    , observ
                                                                    , usuario
                                                                    , referencia
                                                                    , quota_consumo
                                                                    , meta_mes
                                                                    , tipo_ligacao);
            try
            {
                return ExecutarSql(sql.ToString(), informix);
            }
            catch (Exception ex)
            {
                throw;
            }
        }


        public List<TelefoneDTO> RetornaTelOrdem(string squencial)
        {
            string sql = string.Format(@"SELECT tipo_telefone, prefixo_ddd, numero_telefone, ind_contato, ramal
											FROM telefone_ordem
											WHERE numero_ordem = '{0}';");

            var dt = ConsultaSql(sql.ToString());
            if (dt.Rows.Count > 0)
            {
                var resultado = DataHelper.ConvertDataTableToList<TelefoneDTO>(dt);

                return resultado;
            }
            else
                return null;
        }

        public RetornoAltaContratoDTO RetornaDadosMedidor(string numero_cliente)
        {
            string sql = string.Format(@"   select correlativo_ruta as SEC_LECT,
		   case when m.FECHA_ULT_INSTA is not null then FECHA_ULT_INSTA
		   else FECHA_PRIM_INSTA END as EQ_DATAB, 
		   NVL(t.valor_alf, '') as EQ_MATNR_I,
		   m.numero_fabrica as EQ_SERNR,
		   mm.constante_kd as EQ_ACTIVE_CFACT
		   from medid m,cliente c,modmed mm, tabla t where
		   c.numero_cliente = m.numero_cliente and
		   t.codigo = (m.clave_demanda||m.clave_montri)
		   and t.sucursal = '0000'
		   and t.nomtabla = 'FOCONS'
		   and mm.marca_medidor = m.marca_medidor 
		   and mm.modelo = m.modelo and
		   m.numero_cliente = {0}
		   ", numero_cliente);

            var dt = ConsultaSql(sql.ToString());
            if (dt.Rows.Count > 0)
            {
                var resultado = DataHelper.ConvertDataTableToEntity<RetornoAltaContratoDTO>(dt);

                return resultado;
            }
            else
                return null;
        }

        //        public RetornoAltaContratoDTO RetornaDadosMedidorGA(string numero_cliente)
        //        {
        //            string sql = string.Format(@"   select 
        //		   case when m.FECHA_ULT_INSTA is not null then FECHA_ULT_INSTA
        //		   else FECHA_PRIM_INSTA END as EQ_DATAB, 
        //		   NVL(t.valor_alf, '') as grupo_registrador,
        //		   m.numero_fabrica,
        //		   mm.constante_kd as fator,
        //		   case when h.lect_ter_act_hp <> 0 then h.lect_ter_act_hp 
        //		   else lect_fac_act_hp END as leitura_registrador1,
        //		   case when lectura_terreno <> 0 then lectura_terreno else lectura_facturac END 
        //		   as leitura_registrador2,
        //		   m.enteros as casa_antes_virgula1,
        //		   m.decimales casa_pos_virgula1
        //		   from medid m,modmed mm, hislec h, tabla t where
        //		   t.codigo = (m.clave_demanda||m.clave_montri)
        //		   and t.sucursal = '0000'
        //		   and t.nomtabla = 'FOCONS'
        //           and mm.marca_medidor = m.marca_medidor 
        //		   and mm.modelo = m.modelo 
        //		   and m.numero_cliente = h.numero_cliente and
        //		   m.numero_cliente = {0}
        //		   and h.fecha_evento = (select max(fecha_evento) from hislec where numero_cliente = {0})
        //		   ", numero_cliente);

        //            var dt = ConsultaSql(sql.ToString());
        //            if (dt.Rows.Count > 0)
        //            {
        //                var resultado = DataHelper.ConvertDataTableToEntity<RetornoAltaContratoDTO>(dt);

        //                return resultado;
        //            }
        //            else
        //                return null;
        //        }

        public DataTable RetornaDocumentoOrdem(string sequencial)
        {
            string sql = string.Format(@"SELECT tipo_documento
												,numero_doc
												,dv_documento
												,compl_documento
												,data_emissao
												,nvl(sequencial,'0') as doc_sequencial
												,uf 
												,foto 
												,rowid
											  FROM documento_ordem
											 WHERE numero_ordem = {0}
											 ORDER BY 6, rowid;", sequencial);

            var dt = ConsultaSql(sql.ToString());

            return dt;
        }

        public bool InsertDocumentoCliente(string numero_cliente, string tipo_documento, string numero_doc, string dv_documento, string compl_documento, string data_emissao, string sequencial, string uf, string foto)
        {
            string sql = string.Format(@"INSERT INTO documento_cliente
													(numero_cliente
													,tipo_documento
													,numero_doc
													,dv_documento
													,compl_documento
													,data_emissao
													,sequencial
													,uf 
													,foto) 
													VALUES
													({0},'{1}','{2}','{3}','{4}','{5}','{6}','{7}','{8}')"
                                                    , numero_cliente
                                                    , tipo_documento
                                                    , numero_doc
                                                    , dv_documento
                                                    , compl_documento
                                                    , data_emissao
                                                    , sequencial
                                                    , uf
                                                    , foto);


            DBProviderInformix informix = ObterProviderInformix();
            return ExecutarSql(sql.ToString(), informix);
        }

        public bool ExcluirDocumentoOrdem(string sequencial)
        {
            string sql = string.Format(@"DELETE FROM documento_ordem
										WHERE numero_ordem = '{0}'", sequencial);

            DBProviderInformix informix = ObterProviderInformix();
            return ExecutarSql(sql.ToString(), informix);

        }

        public bool InserirMudancaDeTarifa(ContratoDTO contrato, DBProviderInformix informix)
        {
            string sql = "INSERT INTO mudtar ";
            sql += "(numero_ordem, numero_cliente, data_solicitacao, origem_solicitacao, ";
            sql += "prazo_recurso_cli, prazo_resp_coelce, classe_atual, subclasse_atual, ";
            sql += "tarifa_atual, classe_nova, subclasse_nova, tarifa_nova, data_inc_solic, ";
            sql += "usuario_inc_solic, data_situacao, situacao, usuario_situacao) ";
            sql += "values ( ";
            sql += string.Format(
                "'{0}', '{1}', {2}, '{3}', '{4}', '{5}', '{6}', '{7}', '{8}', '{9}', '{10}', '{11}', {12}, '{13}', {14}, '{15}', '{16}'"
                , contrato.numero_ordem     //criar
                , contrato.numero_cliente   //receber
                , "current"                 //fixo
                , contrato.origem_solicitacao//receber
                , 10                        //fixo
                , 30                        //fixo
                , contrato.classe           //consultar
                , contrato.subclasse        //consultar
                , contrato.tarifa           //consultar
                , contrato.classe_nova      //receber
                , contrato.subclasse_nova   //receber
                , contrato.tarifa_nova      //receber
                , "current"                 //fixo
                , contrato.UsuarioIngresso  //fixo
                , "current"                 //fixo
                , 0                         //fixo
                , contrato.UsuarioIngresso  //fixo
                );
            sql += ")";

            return ExecutarSql(sql, informix);
        }

        public ContratoRetornoDTO retornaLeitura(string numero_cliente)
        {
            string sql = string.Format(@"SELECT FIRST 1 h.lectura_terreno as EQ_ZWSTANDCE_EAI_1,                                                                                
														m.numero_fabrica AS EQ_SERNR,
														NVL(t.valor_alf, '') as EQ_MATNR_I                                              
										FROM hislec h, medid m, tabla t   
										WHERE h.numero_cliente={0}
										AND h.numero_cliente = m.numero_cliente
										AND t.codigo = (m.clave_demanda||m.clave_montri)
										and t.sucursal = '0000'
										and t.nomtabla = 'FOCONS'
										ORDER BY h.fecha_evento DESC", numero_cliente);

            var dt = ConsultaSql(sql.ToString());
            if (dt.Rows.Count > 0)
            {
                ContratoRetornoDTO resultado = DataHelper.ConvertDataTableToEntity<ContratoRetornoDTO>(dt);

                return resultado;
            }
            else
                return null;
        }

        public int ConsultaCliMedProcesso(ContratoDTO contrato)
        {
            int retorno = 0;
            string sql = string.Format(@"SELECT count(*) as total FROM cli_med_processo WHERE numero_cliente={0}", contrato.numero_cliente);

            var dt = ConsultaSql(sql.ToString());

            if (dt.Rows.Count > 0)
            {
                retorno = Convert.ToInt32(dt.Rows[0]["total"]);
            }

            return retorno;
        }


        public bool insertPangeaLeituraVisita(string numero_cliente,string numero_gac,string codigo, DBProviderInformix informix)
        {
            bool resultado = true;
            try
            {
                string sql = string.Format(@"INSERT INTO pangea_leitura_visita (numero_cliente,nro_gac,codigo) VALUES({0},{1},{2})",numero_cliente,numero_gac,codigo);

                ExecutarSql(sql.ToString(), informix);
            }
            catch (Exception ex)
            {
                resultado = false;
                throw;
            }

            return resultado;
        }

    }
}
