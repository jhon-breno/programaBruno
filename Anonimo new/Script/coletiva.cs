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
      
        for(Contract loopCntr : [SELECT id, ContractNumber, CNT_ExternalContract_ID__c, CNT_GroupNumerCntr__c,
                                        CNT_GroupSegment__c, CNT_GroupArea__c, CNT_GroupPayType__c, CNT_GroupClass__c, Account.Name, Account.Parent.CondominiumRUT__c,
                       Account.CompanyID__c, Account.IdentityType__c, Account.CNT_State_Inscription__c,
                       Account.CNT_State_Inscription_Exemption__c,
                       Account.CNT_GroupAssociate__r.CondominiumRUT__c, Account.IdentityNumber__c,
                       Account.ExternalId__c
                                   FROM Contract
                                  WHERE Account.CompanyID__c='2003' and  Account.CNT_GroupAssociate__c  != '' and CNT_GroupNumerCntr__c = :linha.Split('>')[0] and startdate = 2019-08-22 limit 1]){ contrato = loopCntr;}
        System.debug('><>ContractNumber: '+contrato.ContractNumber);
        
        for(Billing_Profile__c loopBP : [SELECT id, CNT_Due_Date__c, Address__r.StreetMD__r.Street_Type__c, Delivery_Type__c,
                                                Address__r.StreetMD__r.Street__c, Address__r.Number__c, Address__r.Postal_Code__c,
                                                Address__r.Municipality__c, Address__r.StreetMD__r.Neighbourhood__c, CNT_GroupPayType__c, CNT_GroupClass__c, CNT_Lot__c
                                           FROM Billing_Profile__c
                                          WHERE CNT_Contract__c =: contrato.ContractNumber limit 1]){ billingProfile = loopBP;}
        
        for (CNT_Street_Type_Setting__mdt threatMapping : [SELECT Code__c, Value__c FROM CNT_Street_Type_Setting__mdt]) {
            System.debug(threatMapping.Code__c + ':' + threatMapping.Value__c);
            if(billingProfile.Address__r.StreetMD__r.Street_Type__c == threatMapping.Code__c){
                streetType = threatMapping.Value__c + ' ';
                break;
            }
        }
        
        if(billingProfile.Delivery_Type__c == 'N') {
            enderecoType = 'FA';
        }
        
        String codSistema;
        if (contrato.Account.CompanyID__c == '2005'){
            CodSistema = 'AMPSAP';
        }else{
            CodSistema = 'COESAP';
        }
        
        ATCL_VFC042_wsdlUtils.BodyPutPerfilFacturacionRequest_element reqBody = new ATCL_VFC042_wsdlUtils.BodyPutPerfilFacturacionRequest_element();
        ATCL_VFC042_wsdlUtils.ListaCuentaContrato_element listaCuentaContrato = new ATCL_VFC042_wsdlUtils.ListaCuentaContrato_element();
		ATCL_VFC042_wsdlUtils.HeaderRequestType reqHeader = new ATCL_VFC042_wsdlUtils.HeaderRequestType();
        
        //prepare data
        reqHeader.CodSistema = codSistema;
        reqHeader.SistemaOrigen = 'SFDC';
        reqHeader.FechaHora = String.valueOf(date.today());
        reqHeader.Funcionalidad = 'Alta';
        
        reqBody.CodigoEmpresa = contrato.Account.CompanyID__c;                                    //ID_FO
        reqBody.NumeroCaso = '1';                                                             //ID_FO
        reqBody.Motivo = 'GROUP';                                                             //CAUSALE
        reqBody.SubMotivo = 'DELIVERYINVOICE';                                                //TIPO_OPERAZIONE
        reqBody.CuentaContrato = contrato.CNT_GroupNumerCntr__c;                              //VKONTO
        reqBody.Cliente = contrato.Account.ExternalId__c;                                         //BP
        if(reqBody.Cliente.startsWith('2003') || reqBody.Cliente.startsWith('2005')){ reqBody.Cliente = reqBody.Cliente.substring(4);  }
        reqBody.CondicionPagoCliente = (billingProfile.CNT_Due_Date__c.length() == 1 ?
                                            'CP0' + billingProfile.CNT_Due_Date__c :
                                            'CP' + billingProfile.CNT_Due_Date__c);           //ZAHLKOND
        reqBody.Calle = streetType + billingProfile.Address__r.StreetMD__r.Street__c;         //STREET
        reqBody.Numero = billingProfile.Address__r.Number__c;                                 //HOUSENUMBER
        reqBody.CodigoPostal = billingProfile.Address__r.Postal_Code__c;                      //POST_CODE
        reqBody.Ciudad = (billingProfile.Address__r.Municipality__c != null ?
                              billingProfile.Address__r.Municipality__c :
                              '');                                                            //CITY
        reqBody.Barrio = (billingProfile.Address__r.StreetMD__r.Neighbourhood__c != null ?
                              billingProfile.Address__r.StreetMD__r.Neighbourhood__c :
                              'SEM BAIRRO');                                                  //CITY2
        reqBody.Estado = (contrato.Account.CompanyID__c == '2005' ? 'RJ' : 'CE');                 //REGION
        reqBody.ComplementoDireccion = '';                                                    //SUPLEMENT
        reqBody.NombreCuentaContratoInd = contrato.Account.Name.length() > 35 ? contrato.Account.Name.substring(0,35) : contrato.Account.Name;                                  //VKBEZ
        reqBody.CuentaContratoColect = contrato.Account.CNT_GroupAssociate__r.CondominiumRUT__c;  //CONTROLLER
        reqBody.NombreCuentaContColect = contrato.Account.CNT_GroupAssociate__r.CondominiumRUT__c;//CONTROLLER
        //reqBody.TipoEnvioFactura = billingProfile.CNT_FormPayment__c;                       //OPTION_INVOICE
        reqBody.TipoEnvioFactura = (String.isNotBlank(billingProfile.CNT_GroupPayType__c)) ? billingProfile.CNT_GroupPayType__c: '';//OPTION_INVOICE
        reqBody.DireccionExterna = contrato.CNT_ExternalContract_ID__c + '_' + enderecoType;
                                                                                              //ADEXT
        if(Integer.valueOf(contrato.Account.IdentityType__c) != 2){// Pessoa Fisica (B2C_BRASIL)
            reqBody.TipoCliente = '1';                                                        //BU_TYPE
            reqBody.TipoDocFiscal = 'BR2';                                                    //BP_TAXTYPE
            reqBody.NomeOrganizacao = '';                                                     //BP_NAME_ORG1
            reqBody.PrimeiroNome = (contrato.Account.Name == null ?
                                        '' :
                                        contrato.Account.Name.substringBefore(' '));              //BP_NAME_FIRST
            reqBody.UltimoNome = (contrato.Account.Name == null ?
                                      '' :
                                      contrato.Account.Name.substringAfter(' '));                 //BP_NAME_LAST
        } else { // Pessoa Juridica (B2B_BRASIL)
            reqBody.TipoCliente = '2';                                                        //BU_TYPE
            reqBody.TipoDocFiscal = 'BR1';                                                    //BP_TAXTYPE
            reqBody.NomeOrganizacao = contrato.Account.Name;                                      //BP_NAME_ORG1
            reqBody.PrimeiroNome = '';                                                        //BP_NAME_FIRST
            reqBody.UltimoNome = '';                                                          //BP_NAME_LAST
        }
        reqBody.NumDocumentoFiscal = contrato.Account.IdentityNumber__c;                          //BP_FISCALNUMBER
        reqBody.CaractDeterminacion = (String.isNotBlank(billingProfile.CNT_GroupClass__c)) ? billingProfile.CNT_GroupClass__c: '';                             //KOFIZ_SD
        reqBody.InscricaoEstadual = (contrato.Account.CNT_State_Inscription_Exemption__c ?
                                         'ISENTO':
                                         contrato.Account.CNT_State_Inscription__c);              //BP_IDNUMBER_IE
        reqBody.InscricaoMunicipal = '';                                                      //BP_IDNUMBER_IM
        reqBody.FormaPagamento = 'C';                                                         //AC_EZAWE ??????? De onde pegar essa informação
        reqBody.Segmento = contrato.CNT_GroupSegment__c;                                      //AC_SEGMENT
        reqBody.TextoSegmento = mapFieldPickLabelSegment.get(contrato.CNT_GroupSegment__c);   //AC_SEGMENT_T
        reqBody.Area = contrato.CNT_GroupArea__c;                                             //AC_AREA
        reqBody.TextoArea = mapFieldPickLabelArea.get(contrato.CNT_GroupArea__c);             //AC_AREA_T
        reqBody.Lote = linha.Split('>')[1];                                                             //AC_PORTION
        reqBody.Tipo = '';                                                                    //AC_PORTION


		String filha =linha.Split('>')[2];
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


		System.debug('O QUE VAI:' + JSON.serialize(reqBody));
res = service.Put_PerfilFacturacion(req.HeaderRequest, req.BodyPutPerfilFacturacionRequest);

System.debug('RESUTADO: ' + res.BodyPutPerfilFacturacionResponse.DescripcionResultado);