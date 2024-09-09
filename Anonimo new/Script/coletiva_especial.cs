String linha = '{0}';

ATCL_VFC042_wsdlUtils.Put_PerfilFacturacionRequest_element req = new ATCL_VFC042_wsdlUtils.Put_PerfilFacturacionRequest_element();
ATCL_VFC042_wsdlUtils.Put_PerfilFacturacionResponse_element res = new ATCL_VFC042_wsdlUtils.Put_PerfilFacturacionResponse_element();
ATCL_VFC042_wsdlUtils.EXP_SOAP_SALESFORCE_Soap service = new ATCL_VFC042_wsdlUtils.EXP_SOAP_SALESFORCE_Soap();


		

        Map<String,String> mapFieldPickLabelSegment = new Map<String,String>();
        Map<String,String> mapFieldPickLabelArea = new Map<String,String>();
        Billing_Profile__c billingProfile = new Billing_Profile__c();
        Contract contrato = new Contract();
        Case caso = new Case();
        String enderecoType = 'MA';
        String streetType = '';
        
        mapFieldPickLabelSegment = CNT_Utility.getPicklistLabel('Contract','CNT_GroupSegment__c');
        mapFieldPickLabelArea = CNT_Utility.getPicklistLabel('Contract','CNT_GroupArea__c');
      
        
        String codSistema;
        CodSistema = 'COESAP';
        
        
        ATCL_VFC042_wsdlUtils.BodyPutPerfilFacturacionRequest_element reqBody = new ATCL_VFC042_wsdlUtils.BodyPutPerfilFacturacionRequest_element();
        ATCL_VFC042_wsdlUtils.ListaCuentaContrato_element listaCuentaContrato = new ATCL_VFC042_wsdlUtils.ListaCuentaContrato_element();
		ATCL_VFC042_wsdlUtils.HeaderRequestType reqHeader = new ATCL_VFC042_wsdlUtils.HeaderRequestType();
        
        //prepare data
        reqHeader.CodSistema = codSistema;
        reqHeader.SistemaOrigen = 'SFDC';
        reqHeader.FechaHora = String.valueOf(date.today());
        reqHeader.Funcionalidad = 'Alta';
        
        reqBody.CodigoEmpresa = '2003';                                    //ID_FO
        reqBody.NumeroCaso = '1';                                                             //ID_FO
        reqBody.Motivo = 'GROUP';                                                             //CAUSALE
        reqBody.SubMotivo = 'DELIVERYINVOICE';                                                //TIPO_OPERAZIONE
        reqBody.CuentaContrato = linha.Split('>')[1];                              //VKONTO
        reqBody.Cliente = linha.Split('>')[3];//contrato.Account.ExternalId__c;                                         //BP
        //reqBody.Cliente = reqBody.Cliente.substring(4);  }
        reqBody.CondicionPagoCliente = linha.Split('>')[6];
		
    
        reqBody.NombreCuentaContratoInd = linha.Split('>')[4].length() > 35 ? linha.Split('>')[4].substring(0,35) : linha.Split('>')[4];                                  //VKBEZ
        reqBody.CuentaContratoColect = '99990740003';
        reqBody.NombreCuentaContColect = '99990740003';
        
        reqBody.TipoCliente = '2';                                                        //BU_TYPE
        reqBody.TipoDocFiscal = 'BR1';                                                    //BP_TAXTYPE
        reqBody.NomeOrganizacao = linha.Split('>')[4];                                      //BP_NAME_ORG1
        reqBody.PrimeiroNome = '';                                                        //BP_NAME_FIRST
        reqBody.UltimoNome = '';                                                          //BP_NAME_LAST
        reqBody.NumDocumentoFiscal = linha.Split('>')[7];                          //BP_FISCALNUMBER
        reqBody.Lote = linha.Split('>')[2];                                                             //AC_PORTION
        reqBody.Tipo = '';                                                                    //AC_PORTION


		String filha =linha.Split('>')[0];//linha.Split('>')[2];
        reqBody.ListaCuentaContrato = new ATCL_VFC042_wsdlUtils.ListaCuentaContrato_element();
        reqBody.ListaCuentaContrato.CuentasContratos = new List<ATCL_VFC042_wsdlUtils.CuentasContratos_element>();
        
		for(Map<Id,ATCL_VFC042_wsdlUtils.CuentasContratos_element> loopMapCuentasContratos : ((Map<Id,Map<Id,ATCL_VFC042_wsdlUtils.CuentasContratos_element>>) JSON.deserialize('{"a1v1o000006dSw5AAE":{"02i1o000004nnGYAAY":{"TextoSegmentoFila_type_info":["TextoSegmentoFila","http://www.example.org/EXP_SOAP_SALESFORCE/",null,"0","1","false"],"TextoSegmentoFila":null,"TextoAreaFila_type_info":["TextoAreaFila","http://www.example.org/EXP_SOAP_SALESFORCE/",null,"0","1","false"],"TextoAreaFila":null,"SegmentoFila_type_info":["SegmentoFila","http://www.example.org/EXP_SOAP_SALESFORCE/",null,"0","1","false"],"SegmentoFila":null,"RemoveCuentaContrato_type_info":["RemoveCuentaContrato","http://www.example.org/EXP_SOAP_SALESFORCE/",null,"0","1","false"],"RemoveCuentaContrato":"","NombreCuentaContrato_type_info":["NombreCuentaContrato","http://www.example.org/EXP_SOAP_SALESFORCE/",null,"0","1","false"],"NombreCuentaContrato":"'+filha+'","field_order_type_info":["CuentaContrato","NombreCuentaContrato","SegmentoFila","TextoSegmentoFila","AreaFila","TextoAreaFila","RemoveCuentaContrato"],"CuentaContrato_type_info":["CuentaContrato","http://www.example.org/EXP_SOAP_SALESFORCE/",null,"0","1","false"],"CuentaContrato":"'+filha+'","AreaFila_type_info":["AreaFila","http://www.example.org/EXP_SOAP_SALESFORCE/",null,"0","1","false"],"AreaFila":null,"apex_schema_type_info":["http://www.example.org/EXP_SOAP_SALESFORCE/","false","false"]}}}',Map<Id,Map<Id,ATCL_VFC042_wsdlUtils.CuentasContratos_element>>.class)).values())
		{
            for(ATCL_VFC042_wsdlUtils.CuentasContratos_element loopCuentasContratos : loopMapCuentasContratos.values())
			{
                reqBody.ListaCuentaContrato.CuentasContratos.add(loopCuentasContratos);
            }
        }
        
        req.HeaderRequest = reqHeader;
        req.BodyPutPerfilFacturacionRequest = reqBody;

		System.debug('Array: '  + linha.Split('>'));
		System.debug('O QUE VAI:' + JSON.serialize(reqBody));
res = service.Put_PerfilFacturacion(req.HeaderRequest, req.BodyPutPerfilFacturacionRequest);

System.debug('RESUTADO: ' + res.BodyPutPerfilFacturacionResponse.DescripcionResultado);