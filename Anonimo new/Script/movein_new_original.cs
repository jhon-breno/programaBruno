// Enviar ALTERAÇÃO CONTRATUAL SAP massivamente ate 50 registros
List<String> caseNumberList = new List<String>{'36285670'};

CNT_IntegrationHelper.ALTA_BR_242 reqAlta = new CNT_IntegrationHelper.ALTA_BR_242();

CNT_Contracting_Status_BO__c backOffice = new CNT_Contracting_Status_BO__c();
Map<String,Object> resultBackOffice = new Map<String,Object>();

List<id> lst_id = new List<id>();
	
List<Case> listCaso = new List<Case>();
listCaso = [select Id,CaseNumber,AccountId,Account.Name,Account.RecordType.DeveloperName,Contact.Email,Account.IdentityType__c,
Account.IdentityNumber__c,Account.MainPhone__c,Account.ExternalId__c,Account.CNT_State_Inscription__c,
Account.CNT_State_Inscription_Exemption__c,Account.CNT_Executive__r.CNT_Code_Executive__c,PointofDelivery__r.Name,
PointofDelivery__c,Contact.FirstName,Contact.LastName,Address__r.StreetMD__r.Municipality__c,Address__r.Number__c,
Address__r.Postal_Code__c,Address__r.StreetMD__r.Street__c,Address__r.StreetMD__r.Street_Type__c,Address__r.Corner__c,
Account.CompanyID__c,CNT_Economical_Activity__c,CNT_Free_Client__c,CNT_Transit_Potencia_Total__c,
Address__r.StreetMD__r.Neighbourhood__c,Address__r.Municipality__c,SubCauseBR__c,AssetId,CNT_Potencia__c,
CNT_Conexion_Transitoria__c,CNT_Transit_Date_From__c,CNT_Transit_Date_To__c,CNT_Con_Transit_PotenciaMax__c,
CNT_Con_Transit_PotenciaSimult__c,CNT_Con_Transit_CosenoPhi__c,Account.Country__c,RecordType.DeveloperName,
CNT_Contract__c,CNT_Hora_Diaria__c,CNT_Total_Dias_Utilizacao__c,CNT_Ramo__c,CNT_CIIU__c,CNT_Public_Ilumination__c,
CNT_Change_Type__c,RecordTypeId,Asset.NE__Order_Config__c,CNT_Mandate_Code__c,CNT_Mandate_Amount__c,Contact.Name,
Account.Id,PointofDeliveryAddress__c,PointofDeliveryNumber__c,CNT_Ticket_Expiration_Date__c,Status,
CNT_LastInvoiceOptions__c,CreatedDate,CNT_Quote__c
	from Case where CNT_Contract__r.contractnumber IN: caseNumberList and SubCauseBR__c in ('07','08','44','46','48') ];

List<CNT_Contracting_Status_BO__c> backOfficeList = new List<CNT_Contracting_Status_BO__c>();
List<Case> updateListCase = new List<Case>();

for(Case caso : listCaso)
{

		try
		{
			//SAP
			String accRecordType = caso.Account.RecordType.DeveloperName;
					
			Id quoteId;
			
			try
			{
				quoteId = [Select Id From NE__Order__c Where CNT_Case__c =: caso.id And RecordType.DeveloperName = 'Order' limit 1].Id;
			} 
			catch(Exception ex)
			{
				quoteId = [Select Id From NE__Order__c Where CNT_Case__c =: caso.id And RecordType.DeveloperName = 'Quote' limit 1].Id;
			}
			
			System.debug('Aqui');
			
			Contract cont = new Contract();
			try
			{
				cont = [Select Id, contractnumber, CNT_ExternalContract_ID__c, CNT_Free_Client__c, StartDate, CNT_Economical_Activity__c From Contract Where CNT_Case__c =: caso.Id limit 1];
			}
			catch(Exception ex) 
			{
				cont = [Select Id, contractnumber, CNT_ExternalContract_ID__c, CNT_Free_Client__c, StartDate, CNT_Economical_Activity__c From Contract Where id =: caso.CNT_Contract__c limit 1];
			}
			
			System.debug('Aqui 2');
			//acont.CNT_ExternalContract_ID__c = cont.contractnumber;
			
			Contract_Line_Item__c cli = new Contract_Line_Item__c();
			try
			{
				cli = [Select Billing_Profile__r.CNT_Due_Date__c, Billing_Profile__r.Type__c, Billing_Profile__r.Delivery_Type__c, Billing_Profile__r.Address__r.StreetMD__r.Name, Billing_Profile__r.Address__r.StreetMD__r.Street__c From Contract_Line_Item__c where Contract__c =: cont.Id and CNT_Product__c like 'Grupo%' limit 1];
			}
			catch(Exception ex)
			{
				System.debug('lala');
				cli =  new Contract_Line_Item__c();
				//Billing_Profile__c bill = new Billing_Profile__c();
				//cli.Billing_Profile__c = new Billing_Profile__c();
				cli.Billing_Profile__r.Type__c = 'B';
				cli.Billing_Profile__r.Delivery_Type__c = 'N';
				cli.Billing_Profile__r.CNT_Due_Date__c = '10';
			}

			System.debug('lala 2');
			if (cli == null)
			{
				cli =  new Contract_Line_Item__c();
				cli.Billing_Profile__r.Type__c = 'B';
				cli.Billing_Profile__r.Delivery_Type__c = 'N';
				cli.Billing_Profile__r.CNT_Due_Date__c = '10';				
			}
			
			System.debug('lala 3');
			if (cli.Billing_Profile__c == null)
			{
				//cli.Billing_Profile__c = new Billing_Profile__c();
				cli.Billing_Profile__r.Type__c = 'B';
				cli.Billing_Profile__r.Delivery_Type__c = 'N';
				cli.Billing_Profile__r.CNT_Due_Date__c = '10';				
			}
			
			System.debug('Aqui 3');
			
			if (cli.Billing_Profile__r.CNT_Due_Date__c == null)
			{
				cli.Billing_Profile__r.Type__c = 'B';
				cli.Billing_Profile__r.Delivery_Type__c = 'N';
				cli.Billing_Profile__r.CNT_Due_Date__c = '10';
			}
			
			CNT_IntegrationDataBR.AttributeBR att = new CNT_IntegrationDataBR.AttributeBR();
			
			System.debug('::Running getAttributes');
        
        map<String,String> mapTensaoSAP = new map<String,String>();
        mapTensaoSAP.put('11.4','27');
        mapTensaoSAP.put('13.8','31');
        mapTensaoSAP.put('34.5','44');
        mapTensaoSAP.put('69','53');
        mapTensaoSAP.put('127','04');
        mapTensaoSAP.put('138','58');
        mapTensaoSAP.put('230','60');
        mapTensaoSAP.put('220','06');
        mapTensaoSAP.put('380','10');
        
        CNT_IntegrationDataBR.AttributeBR result = new CNT_IntegrationDataBR.AttributeBR();

		// Inicializando atributos
		result.Potencia = '1';
		result.ChangeLoad = '1';
		result.Demanda = 0;
		result.DemandaPonta = 0;
		result.DemandaForaPonta = 0;

        System.debug('::quoteId: '+quoteId);
        NE__OrderItem__c item2 = [Select NE__ProdName__c From NE__OrderItem__c Where NE__OrderId__c =: quoteId limit 1];
        
        if (item2.NE__ProdName__c.contains('A')){
            result.Grupo = 'A';
        }else{
            result.Grupo = 'B';
        }
            String regexValorTensao = '[a-zA-Z]{1,}';
        
		for (NE__Order_Item_Attribute__c att2 : [Select Id, Name, NE__Value__c, NE__Action__c From NE__Order_Item_Attribute__c Where NE__Order_Item__r.NE__OrderId__c =: quoteId and  NE__Order_Item__r.NE__ProdName__c like 'Grupo%']){
            
            system.debug(att2.Name + '==>' + att2.NE__Value__c);
            
            if (att2.Name.contains('Carga')){
                //DGUANA 26-07-18: reparado por que explotaba cuando el valor era nulo (¿Está bien 0?)
                result.ChangeLoad = att2.NE__Action__c;
                result.Carga = att2.NE__Value__c!=null ? Integer.valueOf(att2.NE__Value__c) : 0;
            }else if (att2.Name.contains('Modalidade')){
                //DGUANA 26-09-18: reparado por que explotaba cuando el valor era nulo
                result.ModalidadeTarifaria = att2.NE__Value__c!=null ? att2.NE__Value__c : '';
            }else if (att2.Name == 'Classe BR'){
                //DGUANA 26-07-18: reparado por que explotaba cuando el valor era nulo
                result.Classe = att2.NE__Value__c!=null ? att2.NE__Value__c.substring(0,2) : '';
            }else if (att2.Name == 'SubClasse BR'){
                //DGUANA 26-07-18: reparado por que explotaba cuando el valor era nulo
                result.SubClasse = att2.NE__Value__c!=null ? att2.NE__Value__c.substring(0,att2.NE__Value__c.indexOf('-')).trim() : '';
            }else if (att2.Name == 'Demanda KV BR'){
                result.Demanda = String.isNotBlank(att2.NE__Value__c) ? Integer.valueOf(att2.NE__Value__c) : 1;
            }else if (att2.Name.contains('Valor')){
                //DGUANA 26-07-18: reparado por que explotaba cuando el valor era nulo
                //result.ValorTensao = att2.NE__Value__c!=null ? att2.NE__Value__c.replace(' KV','') : '';

                // artur.miranda 2018-10-29
                if(att2.NE__Value__c != null){
                    result.ValorTensao = att2.NE__Value__c.replaceAll(regexValorTensao, '').trim();
                } else {
                    result.ValorTensao = '';
                }
                
            }else if (att2.Name.contains('Capacidade')){
                result.CapacidadeDisjuntor = att2.NE__Value__c;
            }else if (att2.Name.contains('Instala')){
                result.InstalacaoPadrao = att2.NE__Value__c;
            }else if (att2.Name.contains('Categor')){
                result.CategoriaTarifa = att2.NE__Value__c;
                result.ActionTarifa = att2.NE__Action__c;
            }else if (att2.Name.contains('Potencia')){
                result.Potencia = att2.NE__Value__c != null ? att2.NE__Value__c : '1';
            }else if (att2.Name.contains('Tipo')){
                result.TipoTensao = att2.NE__Value__c;
            }else if (att2.Name.contains('Demanda Ponta BR')){
                result.DemandaPonta = att2.NE__Value__c != null ? Integer.valueOf(att2.NE__Value__c) : 0;
            }else if (att2.Name.contains('Demanda Fora de Ponta BR')){
                result.DemandaForaPonta = att2.NE__Value__c != null ? Integer.valueOf(att2.NE__Value__c) : 0;
            }else if (att2.Name.contains('Nivel')){
                //DGUANA 26-07-18: reparado por que explotaba cuando el valor era nulo
                result.NivelTensao = att2.NE__Value__c!=null ? (att2.NE__Value__c.contains('Baixa') ? '1' : '0') : '';
            }
        }
        System.debug('Checkpoint 1');
        
        if (result.CategoriaTarifa != null){
            result.Tarifa = result.CategoriaTarifa.substring(0,result.CategoriaTarifa.indexOf('-')).trim();
        }
        result.ValorTensaoSAP = mapTensaoSAP.get(result.ValorTensao);
        if(result.TipoTensao != null){
            if (result.TipoTensao.contains('Mo')){
                result.TipoTensao = '1';
            }else if (result.TipoTensao.contains('Bi')){
                result.TipoTensao = '2';
            }else if (result.TipoTensao.contains('Tri')){
                result.TipoTensao = '3';
            }
        }
        else { 
            result.TipoTensao = '3';
        }
        System.debug('Checkpoint 2');
			
			
			att = result;
			
		
		String codSistema;
        if (caso.Account.CompanyID__c == '2005'){
            CodSistema = 'AMPSAP';
        }else{
            CodSistema = 'COESAP';
        }
        
		 System.debug('Checkpoint 3');
		 
        //prepare data
        CNT_IntegrationHelper.ALTA_BR_242 req = new CNT_IntegrationHelper.ALTA_BR_242();
        
        CNT_IntegrationHelper.Header reqHeader = new CNT_IntegrationHelper.Header();
        reqHeader.CodSistema = codSistema;
        reqHeader.SistemaOrigen = 'SFDC';
        reqHeader.FechaHora = date.today();
        reqHeader.Funcionalidad = 'Alta';
         System.debug('Checkpoint 4');
        //TROCA
        //CLASSE=TRANSFER
        //ATTIVITA=ACCOUNT
        
        CNT_IntegrationHelper.BodyALTA_242 reqBody = new CNT_IntegrationHelper.BodyALTA_242();
        //reqBody.CLASSE = isOrder == false ? 'MOVE IN' : 'TRANSFER';
        //reqBody.ATTIVITA = isOrder == false ? 'DEFINITIVE' : 'ACCOUNT';
        reqBody.IM_ZZ_NUMUTE = Integer.valueOf(cont.contractnumber); //contract number
        reqBody.AC_VKONT = cont.contractnumber;
        reqBody.IM_ANLAGE = caso.PointofDelivery__r.Name; //pod number
        reqBody.ID_RICHIESTA = caso.CaseNumber;
        reqBody.ID_RICHIESTA_FO = caso.CaseNumber;
        reqBody.BP_EXECUTIVE = ( String.isNotBlank (caso.Account.CNT_Executive__c) && String.isNotBlank(caso.Account.CNT_Executive__r.CNT_Code_Executive__c) &&  caso.Account.CNT_Executive__r.CNT_Code_Executive__c.indexOf('-') > 0) ? caso.Account.CNT_Executive__r.CNT_Code_Executive__c.split('-').get(1).trim() : '';   
         System.debug('Checkpoint 5');
        reqBody.BP_BPEXT = caso.Account.ExternalId__c == null ? '': caso.Account.ExternalId__c;
        if(reqBody.BP_BPEXT.startsWith('2003') || reqBody.BP_BPEXT.startsWith('2005')){ reqBody.BP_BPEXT = reqBody.BP_BPEXT.substring(4);  }
        
        reqBody.IM_CHARGE = Integer.valueOf(att.Carga);
        //info tecnica
        reqBody.IM_TARIFTYP = att.Tarifa;
		
		  System.debug('Checkpoint 6');
        
        if (caso.CNT_Free_Client__c){ //cliente livre
            if(String.isNotBlank(att.ModalidadeTarifaria) && att.ModalidadeTarifaria.contains('Azul')){
                reqBody.DI_CONTPTL = att.DemandaPonta;
                reqBody.DI_CONTFPL = att.DemandaForaPonta;
                reqBody.DI_CONTRPT = 0;
                reqBody.DI_CONTRFP = 0;
                reqBody.DI_CONTRAT = 0;
                reqBody.DI_CONTUL = 0;
            } else if(String.isNotBlank(att.ModalidadeTarifaria) && att.ModalidadeTarifaria.contains('Verde')){
                reqBody.DI_CONTPTL = att.DemandaPonta;
                reqBody.DI_CONTFPL = att.DemandaForaPonta;
                reqBody.DI_CONTRPT = 0;
                reqBody.DI_CONTRFP = 0;
                reqBody.DI_CONTUL = att.Demanda;
                reqBody.DI_CONTRAT = 0;
            }
            reqBody.IM_DI_CONTRAT = 0;
            reqBody.IM_DI_CONTRPT = 0;
            reqBody.IM_DI_CONTRFP = 0;
            reqBody.IM_DI_CONTUL = 0;
            reqBody.IM_DI_CONTFPL = 0;
            reqBody.IM_DI_CONTPTL = 0;
        }else{
			
            if(String.isNotBlank(att.ModalidadeTarifaria) && att.ModalidadeTarifaria.contains('Azul')){
                reqBody.IM_DI_CONTRPT = att.DemandaPonta;
                reqBody.IM_DI_CONTRFP = att.DemandaForaPonta;
                reqBody.IM_DI_CONTRAT = 0;
                reqBody.IM_DI_CONTUL = 0;
                reqBody.IM_DI_CONTFPL = 0;
                reqBody.IM_DI_CONTPTL = 0;
            } else if (String.isNotBlank(att.ModalidadeTarifaria) && att.ModalidadeTarifaria.contains('Verde')){
                reqBody.IM_DI_CONTRAT = att.Demanda;
                reqBody.IM_DI_CONTRPT = 0;
                reqBody.IM_DI_CONTRFP = 0;
                reqBody.IM_DI_CONTUL = 0;
                reqBody.IM_DI_CONTFPL = 0;
                reqBody.IM_DI_CONTPTL = 0;
            } else if(String.isNotBlank(att.ModalidadeTarifaria) && att.ModalidadeTarifaria.contains('OPTANTE')){
                reqBody.IM_DI_CONTRAT = 0;
                reqBody.IM_DI_CONTRPT = 0;
                reqBody.IM_DI_CONTRFP = 0;
                reqBody.IM_DI_CONTUL = 0;
                reqBody.IM_DI_CONTFPL = 0;
                reqBody.IM_DI_CONTPTL = 0;
            } else {
                reqBody.IM_DI_CONTRAT = 0;
                reqBody.IM_DI_CONTRPT = 0;
                reqBody.IM_DI_CONTRFP = 0;
                reqBody.IM_DI_CONTUL = 0;
                reqBody.IM_DI_CONTFPL = 0;
                reqBody.IM_DI_CONTPTL = 0;
            }
            reqBody.DI_CONTPTL = 0;
            reqBody.DI_CONTFPL = 0;
            reqBody.DI_CONTRPT = 0;
            reqBody.DI_CONTRFP = 0;
            reqBody.DI_CONTUL = 0;
            reqBody.DI_CONTRAT = 0;
        }
        
		  System.debug('Checkpoint 7');
        reqBody.EQ_MATNR_I = 'GAPDR08';
        reqBody.EQ_ZWGRUPPE_I = 'GAPDR08';
        reqBody.IM_CHARGE = att.Carga;
        reqBody.IM_SPEBENE = att.ValorTensaoSAP;
        //reqBody.IM_TEMP_AREA = att.SubClasse;
		reqBody.IM_TEMP_AREA = att.SubClasse == null ? 'COMER' : att.SubClasse;
        //reqBody.IM_BRANCHE = c.CNT_Economical_Activity__c.substring(0,8); //CNAE
		// Manter isso por um tempo ate que os dados do CNAE sejam migrados para contrato
        
		if(String.isNotBlank(cont.CNT_Economical_Activity__c)){
            reqBody.IM_BRANCHE = cont.CNT_Economical_Activity__c.substring(0,8);
        } else {
            reqBody.IM_BRANCHE = 'Z0101000';// Z0101000-Residencial Pleno
        }
        reqBody.IM_GROUP_TENSION = att.Grupo;
        
		System.debug('Checkpoint 8');
		//
        reqBody.AC_ZAHLKOND = cli.Billing_Profile__r.CNT_Due_Date__c.length() == 1 ? 'CP0' + cli.Billing_Profile__r.CNT_Due_Date__c : 'CP' + cli.Billing_Profile__r.CNT_Due_Date__c; //data vencimento
        reqBody.BP_PARTNER = '';
		reqBody.BP_OPBUK = String.valueOf(caso.Account.CompanyID__c);
        reqBody.ID_CASE_EXTERNAL = caso.Id;
        //reqBody.ID_CASE_EXTERNAL = String.valueOf(c.Id).substring(4);
        
		if(cont.CNT_Free_Client__c && caso.SubCauseBR__c.equalsIgnoreCase('15')){
            reqBody.DATA_VALIDITA = cont.StartDate;
        } else {
            reqBody.DATA_VALIDITA = date.today();
        }
        reqBody.ZZ_CANALE_STAMPA = '1';
        reqBody.MI_VERTRAG = '';
		reqBody.MI_VENDE = date.today();
		
		System.debug('Checkpoint 9');
        reqBody.IM_TRASNF_LOSS = 0;
        reqBody.IM_REGION = 'U';
        reqBody.IM_FACTOR_4 = att.TipoTensao;
        reqBody.IM_FACTOR_3 = '14767';
        reqBody.IM_FACTOR_2 = '14767';
        reqBody.IM_FACTOR_1 = String.valueOf(caso.CNT_Transit_Potencia_Total__c);
		System.debug(' IM_FACTOR_1 de flow_ALTA_SAP_BR '+ reqBody.IM_FACTOR_1);
        //reqBody.CO_STREET = c.Address__r.StreetMD__r.Street__c;
        String streetType = '';// Avenida, Rua, Travessa, etc.
        for (CNT_Street_Type_Setting__mdt threatMapping : [SELECT Code__c, Value__c FROM CNT_Street_Type_Setting__mdt]) {
            System.debug(threatMapping.Code__c + ':' + threatMapping.Value__c);
            if(caso.Address__r.StreetMD__r.Street_Type__c == threatMapping.Code__c){
                streetType = threatMapping.Value__c + ' ';
                break;
            }
        }
        reqBody.CO_REGION = caso.Account.CompanyID__c == '2005' ? 'RJ' : 'CE';
        reqBody.CO_POST_CODE1 = caso.Address__r.Postal_Code__c;
        //reqBody.CO_HOUSE_NUM1 = c.Address__r.Number__c;
		reqBody.CO_HOUSE_NUM1 = String.isNotBlank(caso.Address__r.Number__c) ? caso.Address__r.Number__c : '0' ;
        //reqBody.CO_CITY1 = c.Address__r.Municipality__c != null ? c.Address__r.Municipality__c : '';
		reqBody.CO_CITY1 = caso.Address__r.Municipality__c.Replace('�','') != null ? caso.Address__r.Municipality__c.Replace('�','') : 'CE';
        reqBody.BUPLA = '';

		System.debug('Checkpoint 10');
        if(accRecordType.equalsIgnoreCase('B2C_BRASIL')){//Persona Fisica
            reqBody.BPKIND = '9001';
        } else if(accRecordType.equalsIgnoreCase('B2B_BRASIL')){// Persona Juridica
            reqBody.BPKIND = '9002';
        } else if(accRecordType.equalsIgnoreCase('B2G_BRASIL')){// Governo (Persona Juridica)
            reqBody.BPKIND = '9003';
        }

        //reqBody.BP_STREET = streetType + c.Address__r.StreetMD__r.Street__c;
        //reqBody.BP_SMTP_ADDR = c.Contact.Email != null ? c.Contact.Email : '';
		reqBody.BP_SMTP_ADDR = caso.Contact.Email != null ? caso.Contact.Email : 'cttmigradonaopossuiemail@enel.com';
        reqBody.BP_REGION = caso.Account.CompanyID__c == '2005' ? 'RJ' : 'CE';
        reqBody.BP_POST_CODE1 = caso.Address__r.Postal_Code__c;
        //Juan - Enviando valores para Bairro e numero, se estiverem nulos.
        reqBody.BP_HOUSE_NUM1 = String.isNotBlank(caso.Address__r.Number__c) ? caso.Address__r.Number__c : '0' ;
        reqBody.BP_CITY2 = String.isNotBlank(caso.Address__r.StreetMD__r.Neighbourhood__c.Replace('�','')) ? caso.Address__r.StreetMD__r.Neighbourhood__c.Replace('�','') : 'SEM BAIRRO';
        reqBody.BP_CITY1 = String.isNotBlank(caso.Address__r.Municipality__c.Replace('�','')) ? caso.Address__r.Municipality__c.Replace('�','') : 'CE';
        
        // MA => Endereco Postal ou FA => Endereco Fiscal
        String enderecoType = 'MA'; // = FA  / != MA
        if(cli.Billing_Profile__r.Delivery_Type__c == 'N') {
            enderecoType = 'FA';
        }
        
        reqBody.CO_STREET = streetType + caso.Address__r.StreetMD__r.Street__c.Replace('�','');// Endereço do ponto de fornecimento como endereço de entrega
        reqBody.BP_STREET = streetType + caso.Address__r.StreetMD__r.Street__c.Replace('�','');
        reqBody.BP_ADEXT_ADDR = cont.contractnumber + '_' + enderecoType;
        reqBody.BP_IDNUMBER_IE = (caso.Account.CNT_State_Inscription_Exemption__c)? 'ISENTO': caso.Account.CNT_State_Inscription__c;// Isencao ou numero de Inscricao
        
        System.debug('::CO_STREET: '+reqBody.CO_STREET);
        System.debug('::BP_STREET: '+reqBody.BP_STREET);
        System.debug('::CO_STREET: '+reqBody.CO_STREET);
        
        reqBody.AC_VKTYP = '1';
        //reqBody.AC_KOFIZ_SD = '10';
        reqBody.AC_KOFIZ_SD = att.Classe;
        reqBody.AC_GSBER = '1';

		if( ( String.isNotBlank (cli.Billing_Profile__c) && String.isNotBlank(cli.Billing_Profile__r.Type__c) )){
			
			if(cli.Billing_Profile__r.Type__c.equalsIgnoreCase('Statement')){
				 reqBody.AC_EZAWE =  'B';  // B = Boleto; C = Código de Barras; D = Débito Automático 

			}else if(cli.Billing_Profile__r.Type__c.equalsIgnoreCase('Automatic debit')){
				reqBody.AC_EZAWE =  'D'; 

			}else if(cli.Billing_Profile__r.Type__c.equalsIgnoreCase('BarCode')){
				 reqBody.AC_EZAWE =  'C';

			}
		}else{
			 reqBody.AC_EZAWE =  'B';  // B = Boleto; C = Código de Barras
		}	

        reqBody.AC_ABWRH = '';
        
        reqBody.BP_ZZ_CODFISC = caso.Account.IdentityNumber__c;
        //reqBody.BP_TAXTYPE = (('2,002').containsIgnoreCase(c.Account.IdentityType__c) ? 'BR1' : 'BR2');
                
        req.Header = reqHeader;
        req.Body = reqBody;
			
			
		reqBody.DATA_VALIDITA = date.today();	
		reqAlta = req;
		
					//reqAlta.Body.DATA_VALIDITA = date.ValueOf(caso.CreatedDate).addDays(1);
					reqAlta.Body.CLASSE = 'MOVE IN';
					reqAlta.Body.ATTIVITA = 'DEFINITIVE';
					
					if(Integer.valueOf(caso.Account.IdentityType__c) != 2)
					{// Pessoa Fisica (B2C_BRASIL)
						reqAlta.Body.BP_TYPE = '1';
						reqAlta.Body.BP_TAXTYPE = 'BR2';
						reqAlta.Body.BP_NAME_ORG1 = '';
						reqAlta.Body.BP_NAME_FIRST = caso.Account.Name == null ? '' : caso.Account.Name.substringBefore(' ');
						reqAlta.Body.BP_NAME_LAST = caso.Account.Name == null ? '' : caso.Account.Name.substringAfter(' ');
						
						if (reqAlta.Body.BP_NAME_LAST == '')
						{
							reqAlta.Body.BP_NAME_LAST = '.';
						}
					} 
					else 
					{ // Pessoa Juridica (B2B_BRASIL)
						reqAlta.Body.BP_TYPE = '2';
						reqAlta.Body.BP_TAXTYPE = 'BR1';
						reqAlta.Body.BP_NAME_ORG1 = caso.Account.Name;
						reqAlta.Body.BP_NAME_FIRST = '';
						reqAlta.Body.BP_NAME_LAST = '';
					}
					
					if (reqAlta.Body.AC_KOFIZ_SD == '')
					{
						reqAlta.Body.AC_KOFIZ_SD = '10';
						reqAlta.Body.IM_TEMP_AREA = 'REPLN';
						reqAlta.Body.IM_TARIFTYP = 'B1_RESID';
					}
					else if (reqAlta.Body.AC_KOFIZ_SD == '10' && (reqAlta.Body.IM_TEMP_AREA == '' || reqAlta.Body.IM_TEMP_AREA == null))
					{
						reqAlta.Body.AC_KOFIZ_SD = '10';
						reqAlta.Body.IM_TEMP_AREA = 'REPLN';
						reqAlta.Body.IM_TARIFTYP = 'B1_RESID';					
					}
					else if (reqAlta.Body.AC_KOFIZ_SD == '10' && reqAlta.Body.IM_TEMP_AREA == 'REPLN' && (reqAlta.Body.IM_TARIFTYP == '' || reqAlta.Body.IM_TARIFTYP == null))
					{
						reqAlta.Body.AC_KOFIZ_SD = '10';
						reqAlta.Body.IM_TEMP_AREA = 'REPLN';
						reqAlta.Body.IM_TARIFTYP = 'B1_RESID';										
					}
					
			System.debug('JSON SAP: '+JSON.serialize(reqAlta));
			HttpResponse responseSAP = CNT_IntegrationHelper.ALTA_242_BRSync(reqAlta,caso.Id);
			System.debug('responseSAP: '+responseSAP);
			resultBackOffice = CNT_Utility.resultRequest(responseSAP, JSON.serialize(reqAlta), caso, 'Facturador');
			backOffice = (CNT_Contracting_Status_BO__c) resultBackOffice.get('backoffice');
			backOfficeList.add(backOffice);
			Boolean responseStatusSAP = ((Boolean) resultBackOffice.get('status'));
			
			if(responseStatusSAP)
			{
				caso.Status = 'CNT0008';
				caso.cnt_processStatus__c = '';
	        	caso.CNT_ByPass__c = true;
				updateListCase.add(caso);
				lst_id.add(caso.id);		
			}
		}
		catch(Exception ex)
		{
			System.debug('responseSAP: '+ex);
			continue;
		}
}

Insert backOfficeList;
if (updateListCase.size() > 0){ update updateListCase; }

if (lst_id.size() > 0){CNT_ConfigurationUtility.completeOrderFlow(lst_id);}

if (updateListCase.size() > 0){ update updateListCase; }
