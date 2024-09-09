/*
========================================================
Name: CNT_IntegrationDataBR
Type: Class 
Purpose: Data preparation for BRAZIL before send to each integration --> 239=SYNERGIA; 242=SAP

Created by: Alberto Solonyczny
Created on: December 29, 2017

Rev #   Revised on      Revised by          Revision Description 
-----   ----------      --------------      ------------------------
1.0     2017/12/29      Alb. Solonyczny     Initial release
2.0     2018/1/10       Alb. Solonyczny     239 all services/operations
2.1     2018/1/10       Alb. Solonyczny     242 all services/operations
2.2     2018/1/15       Alb. Solonyczny     4 all services/operations
2.3     2018/3/22       Alb. Solonyczny     Added ALTA 239 UAT
2.4     2018/5/2        Felipe Gouvêa       Added Distributed Generation operation
2.5     2018/5/3        Alb. Solonyczny     Replaced CNT_ExternalContract_ID__c by CNT_ExternalContract_ID__c
2.6     2018/5/27       Artur Miranda       Modify Distributed Generation operation
2.6     2018/5/30       Alb. Solonyuczny    Added Troca Titularidade operation 242 SAP
2.7     2018/7/26       D.Guana             Added Validation to posible null values
2.8     2018/8/16       D.Guana             Added condition to case description in "Baixa"
========================================================
*/
public with sharing class CNT_IntegrationDataBR { 
    
    /* SAP */
    
    public static CNT_IntegrationHelper.ALTA_BR_242 flow_ALTA_SAP_BR(Id caseId, boolean isTransfer, boolean isOrder){
        System.debug('::Running flow_ALTA_SAP_BR');
        Case c = [SELECT Id, CaseNumber, AccountId, Account.Name, Account.RecordType.DeveloperName, Contact.Email, Account.IdentityType__c,
                         Account.IdentityNumber__c, Account.MainPhone__c, Account.ExternalId__c, Account.CNT_State_Inscription__c,
                         Account.CNT_State_Inscription_Exemption__c, Account.CNT_Executive__r.CNT_Code_Executive__c, PointofDelivery__r.Name,
                         PointofDelivery__c, Contact.FirstName, Contact.LastName, Address__r.StreetMD__r.Municipality__c, Address__r.Number__c, 
                         Address__r.Postal_Code__c, Address__r.StreetMD__r.Street__c, Address__r.StreetMD__r.Street_Type__c, Address__r.Corner__c,
                         Account.CompanyID__c, CNT_Observation__c, CNT_Economical_Activity__c, CNT_Free_Client__c, CNT_Transit_Potencia_Total__c,
                         Address__r.StreetMD__r.Neighbourhood__c, Address__r.Municipality__c, SubCauseBR__c
                    FROM Case
                   WHERE Id =: caseId];
        String accRecordType = c.Account.RecordType.DeveloperName;

        Id quoteId;
        if(isTransfer){
            quoteId = [Select Id From NE__Order__c Where CNT_Case__c =: caseId And RecordType.DeveloperName = 'Order'].Id;
        } else {
            quoteId = [Select Id From NE__Order__c Where CNT_Case__c =: caseId And RecordType.DeveloperName = 'Quote'].Id;
        }
        
        Contract cont = [Select Id, CNT_ExternalContract_ID__c, CNT_Free_Client__c, StartDate, CNT_Economical_Activity__c From Contract Where CNT_Case__c =: c.Id];
        
        Contract_Line_Item__c cli = [Select Billing_Profile__r.CNT_Due_Date__c, Billing_Profile__r.Type__c, Billing_Profile__r.Delivery_Type__c, Billing_Profile__r.Address__r.StreetMD__r.Name, Billing_Profile__r.Address__r.StreetMD__r.Street__c From Contract_Line_Item__c where Contract__c =: cont.Id and CNT_Product__c like 'Grupo%'];
        
        AttributeBR att = getAttributes(quoteId);
        
        String codSistema;
        if (c.Account.CompanyID__c == '2005'){
            CodSistema = 'AMPSAP';
        }else{
            CodSistema = 'COESAP';
        }
        
        //prepare data
        CNT_IntegrationHelper.ALTA_BR_242 req = new CNT_IntegrationHelper.ALTA_BR_242();
        
        CNT_IntegrationHelper.Header reqHeader = new CNT_IntegrationHelper.Header();
        reqHeader.CodSistema = codSistema;
        reqHeader.SistemaOrigen = 'SFDC';
        reqHeader.FechaHora = date.today();
        reqHeader.Funcionalidad = 'Alta';
        
        //TROCA
        //CLASSE=TRANSFER
        //ATTIVITA=ACCOUNT
        
        CNT_IntegrationHelper.BodyALTA_242 reqBody = new CNT_IntegrationHelper.BodyALTA_242();
        reqBody.CLASSE = isOrder == false ? 'MOVE IN' : 'TRANSFER';
        reqBody.ATTIVITA = isOrder == false ? 'DEFINITIVE' : 'ACCOUNT';
        reqBody.IM_ZZ_NUMUTE = Integer.valueOf(cont.CNT_ExternalContract_ID__c); //contract number
        reqBody.AC_VKONT = cont.CNT_ExternalContract_ID__c;
        reqBody.IM_ANLAGE = c.PointofDelivery__r.Name; //pod number
        reqBody.ID_RICHIESTA = c.CaseNumber;
        reqBody.ID_RICHIESTA_FO = c.CaseNumber;
        reqBody.BP_EXECUTIVE = ( String.isNotBlank (c.Account.CNT_Executive__c) && String.isNotBlank(c.Account.CNT_Executive__r.CNT_Code_Executive__c) &&  c.Account.CNT_Executive__r.CNT_Code_Executive__c.indexOf('-') > 0) ? c.Account.CNT_Executive__r.CNT_Code_Executive__c.split('-').get(1).trim() : '';   
        
        reqBody.BP_BPEXT = c.Account.ExternalId__c == null ? '': c.Account.ExternalId__c;
        if(reqBody.BP_BPEXT.startsWith('2003') || reqBody.BP_BPEXT.startsWith('2005')){ reqBody.BP_BPEXT = reqBody.BP_BPEXT.substring(4);  }
        
        reqBody.IM_CHARGE = Integer.valueOf(att.Carga);
        //info tecnica
        reqBody.IM_TARIFTYP = att.Tarifa;
        
        if (c.CNT_Free_Client__c){ //cliente livre
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
        //
        reqBody.AC_ZAHLKOND = cli.Billing_Profile__r.CNT_Due_Date__c.length() == 1 ? 'CP0' + cli.Billing_Profile__r.CNT_Due_Date__c : 'CP' + cli.Billing_Profile__r.CNT_Due_Date__c; //data vencimento
        reqBody.BP_PARTNER = '';
		reqBody.BP_NAME_FIRST = c.Contact.FirstName == null ? '' : c.Contact.FirstName;
		reqBody.BP_NAME_LAST = c.Contact.LastName == null ? c.Contact.FirstName : c.Contact.LastName;//c.Contact.LastName;
        reqBody.BP_OPBUK = String.valueOf(c.Account.CompanyID__c);
        reqBody.ID_CASE_EXTERNAL = c.Id;
        //reqBody.ID_CASE_EXTERNAL = String.valueOf(c.Id).substring(4);
        if(cont.CNT_Free_Client__c && c.SubCauseBR__c.equalsIgnoreCase('15')){
            reqBody.DATA_VALIDITA = cont.StartDate;
        } else {
            reqBody.DATA_VALIDITA = (isTransfer || isOrder ? date.today().addDays(1) : date.today());
        }
        reqBody.ZZ_CANALE_STAMPA = '1';
        reqBody.MI_VERTRAG = '';
		String resp = getDateLastVisit(caseId);
		if (String.isNotBlank(resp)){
			reqBody.MI_VENDE = Date.valueOf(resp);
		} else {
			//reqBody.VENDE = String.valueOf(System.Today());
			reqBody.MI_VENDE = date.today();
		}
        reqBody.IM_TRASNF_LOSS = 0;
        reqBody.IM_REGION = 'U';
        reqBody.IM_FACTOR_4 = att.TipoTensao;
        reqBody.IM_FACTOR_3 = '14767';
        reqBody.IM_FACTOR_2 = '14767';
        reqBody.IM_FACTOR_1 = String.valueOf(c.CNT_Transit_Potencia_Total__c);
		System.debug(' IM_FACTOR_1 de flow_ALTA_SAP_BR '+ reqBody.IM_FACTOR_1);
        //reqBody.CO_STREET = c.Address__r.StreetMD__r.Street__c;
        String streetType = '';// Avenida, Rua, Travessa, etc.
        for (CNT_Street_Type_Setting__mdt threatMapping : [SELECT Code__c, Value__c FROM CNT_Street_Type_Setting__mdt]) {
            System.debug(threatMapping.Code__c + ':' + threatMapping.Value__c);
            if(c.Address__r.StreetMD__r.Street_Type__c == threatMapping.Code__c){
                streetType = threatMapping.Value__c + ' ';
                break;
            }
        }
        reqBody.CO_REGION = c.Account.CompanyID__c == '2005' ? 'RJ' : 'CE';
        reqBody.CO_POST_CODE1 = c.Address__r.Postal_Code__c;
        //reqBody.CO_HOUSE_NUM1 = c.Address__r.Number__c;
		reqBody.CO_HOUSE_NUM1 = String.isNotBlank(c.Address__r.Number__c) ? c.Address__r.Number__c : '0' ;
        //reqBody.CO_CITY1 = c.Address__r.Municipality__c != null ? c.Address__r.Municipality__c : '';
		reqBody.CO_CITY1 = c.Address__r.Municipality__c != null ? c.Address__r.Municipality__c : 'CE';
        reqBody.BUPLA = '';

        if(accRecordType.equalsIgnoreCase('B2C_BRASIL')){//Persona Fisica
            reqBody.BPKIND = '9001';
        } else if(accRecordType.equalsIgnoreCase('B2B_BRASIL')){// Persona Juridica
            reqBody.BPKIND = '9002';
        } else if(accRecordType.equalsIgnoreCase('B2G_BRASIL')){// Governo (Persona Juridica)
            reqBody.BPKIND = '9003';
        }

        //reqBody.BP_STREET = streetType + c.Address__r.StreetMD__r.Street__c;
        //reqBody.BP_SMTP_ADDR = c.Contact.Email != null ? c.Contact.Email : '';
		reqBody.BP_SMTP_ADDR = c.Contact.Email != null ? c.Contact.Email : 'cttmigradonaopossuiemail@enel.com';
        reqBody.BP_REGION = c.Account.CompanyID__c == '2005' ? 'RJ' : 'CE';
        reqBody.BP_POST_CODE1 = c.Address__r.Postal_Code__c;
        //Juan - Enviando valores para Bairro e numero, se estiverem nulos.
        reqBody.BP_HOUSE_NUM1 = String.isNotBlank(c.Address__r.Number__c) ? c.Address__r.Number__c : '0' ;
        reqBody.BP_CITY2 = String.isNotBlank(c.Address__r.StreetMD__r.Neighbourhood__c) ? c.Address__r.StreetMD__r.Neighbourhood__c : 'SEM BAIRRO';
        //reqBody.BP_CITY1 = String.isNotBlank(c.Address__r.Municipality__c) ? c.Address__r.Municipality__c : '';
		reqBody.BP_CITY1 = String.isNotBlank(c.Address__r.Municipality__c) ? c.Address__r.Municipality__c : 'CE';
        
        // MA => Endereco Postal ou FA => Endereco Fiscal
        String enderecoType = 'MA'; // = FA  / != MA
        if(cli.Billing_Profile__r.Delivery_Type__c == 'N') {
            enderecoType = 'FA';
        }
        
        reqBody.CO_STREET = streetType + c.Address__r.StreetMD__r.Street__c;// Endereço do ponto de fornecimento como endereço de entrega
        reqBody.BP_STREET = streetType + cli.Billing_Profile__r.Address__r.StreetMD__r.Street__c;
        reqBody.BP_ADEXT_ADDR = cont.CNT_ExternalContract_ID__c + '_' + enderecoType;
        reqBody.BP_IDNUMBER_IE = (c.Account.CNT_State_Inscription_Exemption__c)? 'ISENTO': c.Account.CNT_State_Inscription__c;// Isencao ou numero de Inscricao
        
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
        
        reqBody.BP_ZZ_CODFISC = c.Account.IdentityNumber__c;
        //reqBody.BP_TAXTYPE = (('2,002').containsIgnoreCase(c.Account.IdentityType__c) ? 'BR1' : 'BR2');
        if(Integer.valueOf(c.Account.IdentityType__c) != 2){// Pessoa Fisica (B2C_BRASIL)
            reqBody.BP_TYPE = '1';
            reqBody.BP_TAXTYPE = 'BR2';
            reqBody.BP_NAME_ORG1 = '';
            reqBody.BP_NAME_FIRST = c.Account.Name == null ? '' : c.Account.Name.substringBefore(' ');
            reqBody.BP_NAME_LAST = c.Account.Name == null ? '' : c.Account.Name.substringAfter(' ');
        } else { // Pessoa Juridica (B2B_BRASIL)
            reqBody.BP_TYPE = '2';
            reqBody.BP_TAXTYPE = 'BR1';
            reqBody.BP_NAME_ORG1 = c.Account.Name;
            reqBody.BP_NAME_FIRST = '';
            reqBody.BP_NAME_LAST = '';
        }
        
        req.Header = reqHeader;
        req.Body = reqBody;
        
        return req;
    }
    
    public static CNT_IntegrationHelper.BAJA_BR_242 flow_BAJA_SAP_BR(Id caseId, String codeCaseProcess){
        System.debug('::Running flow_BAJA_SAP_BR');
        
        String contractNumber;
        String endDate;
        
        Case c = [Select Id, CaseNumber, Account.Name, Contact.Email, Account.IdentityType__c, Account.IdentityNumber__c,
                         Account.MainPhone__c, CNT_Controller242Success__c, RecordType.DeveloperName, PointofDelivery__r.Name,
                         PointofDelivery__r.SegmentType__c, PointofDeliveryNumber__c, Account.CompanyID__c, CNT_Observation__c,
                         Description, CNT_LastInvoiceOptions__c, Asset.Contract__r.CNT_EndDate__c, CNT_Reservado_Injetado__c,
                         Asset.Contract__r.CNT_ExternalContract_ID__c, CNT_Leitura_1__c, CNT_Leitura_2__c, CNT_Leitura_3__c, CNT_Leitura_4__c, 
						 CNT_Leitura_5__c, CNT_Leitura_6__c, CNT_Leitura_7__c, CNT_Leitura_8__c
                    FROM Case
                   WHERE Id =: caseId];
		  
        //List<Asset> listAsset = [SELECT id, Contract__r.CNT_ExternalContract_ID__c, Contract__r.CNT_EndDate__c FROM Asset WHERE PointofDelivery__c =: c.PointofDelivery__c ORDER BY CreatedDate desc LIMIT 1];
        
        String codSistema;
        if (c.Account.CompanyID__c == '2005'){
            CodSistema = 'AMPSAP';
        }else{
            CodSistema = 'COESAP';
        }
        
        // if(c.RecordTypeId == CNT_CaseUtility.getRecordTypeID('ANY_case_OwnerChange')){
        //     contractNumber = listAsset.get(0).Contract__r.CNT_ExternalContract_ID__c;
        //     endDate = String.valueOf(listAsset.get(0).Contract__r.CNT_EndDate__c);
        // }else{
            contractNumber = c.Asset.Contract__r.CNT_ExternalContract_ID__c;
            endDate = String.valueOf(c.Asset.Contract__r.CNT_EndDate__c);
        // }
        
        //prepare data
        CNT_IntegrationHelper.BAJA_BR_242 req = new CNT_IntegrationHelper.BAJA_BR_242();
        
        CNT_IntegrationHelper.Header reqHeader = new CNT_IntegrationHelper.Header();
        reqHeader.CodSistema = CodSistema;
        reqHeader.SistemaOrigen = 'SFDC';
        reqHeader.FechaHora = date.today();
        reqHeader.Funcionalidad = 'Baixa';
        
        CNT_IntegrationHelper.BodyBAJA_242 reqBody = new CNT_IntegrationHelper.BodyBAJA_242();
        reqBody.IdCaseExternal = c.Id;
        reqBody.CaseNumber = c.CaseNumber;
        reqBody.Cause = 'DEFINITIVE';
        reqBody.SubCause = 'MOVEOUT';
        reqBody.AccountNumber = contractNumber;
        reqBody.AssetId = '';
        reqBody.PoDId = c.PointofDeliveryNumber__c;
        reqBody.CreatedDate = datetime.now(); // Data de execucao MOVE OUT
        reqBody.Request_Date = endDate;
		//Juan Leitura Informada pelo cliente
		reqBody.EQ_ZWSTANDCE_EAI_1 = c.CNT_Leitura_1__c;
        reqBody.EQ_ZWSTANDCE_EAI_2 = c.CNT_Leitura_2__c;
        reqBody.EQ_ZWSTANDCE_EAI_3 = c.CNT_Leitura_3__c;
        reqBody.EQ_ZWSTANDCE_EAI_4 = c.CNT_Leitura_4__c;
		// Abaixo informações de GD para o sap
		reqBody.EQ_ZWSTANDCE_EAI_5 = c.CNT_Leitura_5__c;
		reqBody.EQ_ZWSTANDCE_ERI_1 = c.CNT_Leitura_6__c;
		reqBody.EQ_ZWSTANDCE_ERI_2 = c.CNT_Leitura_7__c;
		reqBody.EQ_ZWSTANDCE_ERI_3 = c.CNT_Leitura_8__c;
		
        reqBody.LecturaCampo = codeCaseProcess;
        reqBody.OPTION_INVOICE = (('1,2'.containsIgnoreCase(codeCaseProcess)) ? 'X' : ''); //empty if is leituraemcampo
        reqBody.OPTION_TARIFF = 'X'; 
        reqBody.ContractNumber = contractNumber;
        System.debug('reqBody --> '+reqBody);

        req.Header = reqHeader;
        req.Body = reqBody;
        
        return req;
    }
    
   //Juan metodo de chamada para o smile

   public static CNT_IntegrationHelper.Lecture_xxx_SmileInnerClass Lecture_xxx_Smile(Id caseId){
        System.debug('::Running Lecture_xxx_Smile');
		String numeroCliente;
		Contract contrato = new Contract();
        Case c = [Select Id, CaseNumber, RecordType.DeveloperName, Account.Company__c,  AccountId, Account.Name, Account.RecordType.DeveloperName, Contact.Email, Account.IdentityType__c, Account.IdentityNumber__c, Account.MainPhone__c, Account.ExternalId__c, Account.CNT_State_Inscription__c, Account.CNT_State_Inscription_Exemption__c,
                  Account.CNT_Executive__r.CNT_Code_Executive__c, PointofDelivery__r.CNT_White_Rate__c, PointofDelivery__r.CNT_Rural_Irrigating__c, PointofDelivery__r.CNT_Free_Client__c, PointofDelivery__r.PointofDeliveryNumber__c, PointofDelivery__r.Name, PointofDelivery__c, Contact.FirstName, Contact.LastName, Address__r.StreetMD__r.Municipality__c, Address__r.Number__c, 
                  Address__r.Postal_Code__c, Address__r.StreetMD__r.Street__c, Address__r.StreetMD__r.Street_Type__c, Address__r.Corner__c, Account.CompanyID__c, CNT_Observation__c, CNT_Economical_Activity__c, 
                  CNT_Free_Client__c, Address__r.StreetMD__r.Neighbourhood__c, Address__r.Municipality__c, CNT_Contract__c, CNT_Leitura_1__c, CNT_Leitura_2__c, CNT_Leitura_3__c, CNT_Leitura_4__c,
				  PointofDelivery__r.MeterNumber__c, PointofDelivery__r.MeterBrand__c, AssetId, Asset.Contract__r.CNT_ExternalContract_ID__c, Asset.PointofDelivery__r.name
                  From Case Where Id =: caseId];
        String accRecordType = c.Account.RecordType.DeveloperName;  //14857360
		String caseRecordType = c.RecordType.DeveloperName;
		System.debug('>>> D ' + caseRecordType);
		//PointofDelivery__c pointCase = [SELECT Id, Name from PointofDelivery__c where name like '14857360'

		        //String contNumber = [Select CNT_ExternalContract_ID__c From Contract Where Id =: c.CNT_Contract__c limit 1].CNT_ExternalContract_ID__c;
        for(Contract loopCntr : [Select id, CNT_Numero_Documento_Beneficio__c, CNT_Document_Type__c, CNT_ExternalContract_ID__c, CNT_Economical_Activity__c, StartDate From Contract Where Id =: c.CNT_Contract__c limit 1]){ contrato = loopCntr;}
        
		List<Device__c> listDvc = [SELECT MeasureType__c, Id, Name, PointofDelivery__r.name, PointofDelivery__r.country__c, MeterBrand__c, MeterModel__c, MeterNumber__c, MeterProperty__c, MeterType__c, Status__c FROM Device__c where PointofDelivery__c =: c.PointofDelivery__c AND Status__c = 'I' LIMIT 1];		

		if(listDvc.size() == 0){
			throw new CNT_CommonException('Nao foi possivel encontrar o Device do Ponto de Fornecimento! O número do medidor deve estar diferente no smile');
		}
		
		
        // Contruindo o REQ
        CNT_IntegrationHelper.Lecture_xxx_SmileInnerClass req = new CNT_IntegrationHelper.Lecture_xxx_SmileInnerClass();

        // Contruindo o HEADER
        CNT_IntegrationHelper.Header reqHeader = new CNT_IntegrationHelper.Header();
        reqHeader.SistemaOrigen = 'SFDC';
        reqHeader.FechaHora = date.today();   // formato correto é 2019-04-11
        reqHeader.Funcionalidad = 'postLectura';
		reqHeader.CodSistema = 'BRASMI';        

        // Contruindo o BODY
        CNT_IntegrationHelper.Body_Lecture_xxx_Smile reqBody = new CNT_IntegrationHelper.Body_Lecture_xxx_Smile();
        // aqui vai entrar as variaveis do metodo Body_Lecture_xxx_Smile, sendo preenchidas com o retorno do select
        //do caso c.       
		
        reqBody.PLAN = 0;  // deve conter
        reqBody.GRUPPO = ''; // deve conter
        reqBody.CENTRO_OPERATIVO = ''; // deve conter
        reqBody.ID_ORDINE_LAVORO = c.PointofDelivery__r.PointofDeliveryNumber__c.leftPad(8,'0');    //  NUMERO DE RASTREIO DA REQUISIÇAO, PODE SER QUALUER UM ex.'86144';    
        reqBody.CodigoEmpresa = (c.Account.CompanyID__c == '2003' ? 'CC01' : c.Account.CompanyID__c);                                  //ID_FO
        System.debug('>>> reqBody.CodigoEmpresa'+reqBody.CodigoEmpresa);
		CNT_IntegrationHelper.ListaLetture reqListaLetture =  new CNT_IntegrationHelper.ListaLetture();
		CNT_IntegrationHelper.Anagrafica  reqAnagrafica = new CNT_IntegrationHelper.Anagrafica();
        reqBody.Anagrafica = reqAnagrafica;
        reqBody.ListaLetture = (new List<CNT_IntegrationHelper.ListaLetture>{reqListaLetture});
		String cntExternalId = c.Asset.Contract__r.CNT_ExternalContract_ID__c;


		/*Campos que são alterados na TROCA DE TITULARIDADE	
		if(caseRecordType == 'CNT_OwnershipChange'){
			reqBody.TIPO_LAVORO = 'NA';  // TIPO DE LAVORO  --> TROCA
			reqListaLetture.SourceType = 'ATTIVAZIONE';
			System.debug('>>> EXTERNAL ID ' + cntExternalId);
			//reqAnagrafica.ENELTEL_PRECEDENTE = cntExternalId;		//-> (NUMERO DO CLIENTE ANTERIOR)
			reqAnagrafica.ENELTEL =  'C'+ cntExternalId.leftPad(8,'0');
			reqAnagrafica.ENELTEL_PRECEDENTE = cntExternalId;
			reqListaLetture.podId = 'BR102E'+c.PointofDelivery__r.Name.leftPad(8,'0');  
		*/


		// CRIANDO O OBJETO listaLetture        
       //COMENTADO ESTE FUNCIONA --- reqListaLetture.serialNumber =  (c.PointofDelivery__r.MeterNumber__c != null ? c.PointofDelivery__r.MeterNumber__c : '4444');  // DEVE SER O MESMO NUMERO DO CAMPO 'MATRICOLA_MIS'
		System.debug('>>> c Antes do metodo '+ listDvc[0].MeterNumber__c);
		reqListaLetture.serialNumber =  (listDvc[0].MeterNumber__c != null ? listDvc[0].MeterNumber__c : '00000000').leftPad(8,'0');
		System.debug('>>> c Depois do metodo '+ reqListaLetture.serialNumber);
		reqListaLetture.PodId = 'BR102E'+c.PointofDelivery__r.Name.leftPad(8,'0');   //'BR102E00012455';
		System.debug('>>> c'+ reqListaLetture.PodId );
        
        reqListaLetture.SourceDetail = 'LAVORO';		///SEMPRE 'LAVORO'
        reqListaLetture.SourceCode = 'S';				///SEMPRE 'S'
		
		//  Gerador
		if(c.PointofDelivery__r.CNT_Rural_Irrigating__c == true && c.PointofDelivery__r.CNT_White_Rate__c == true){
			System.debug('ENCERRAMENTO TIPO BRANCA + IRRIGANTE');
			reqListaLetture.enAttPrelF2  = Integer.valueOf((c.CNT_Leitura_1__c != null ? c.CNT_Leitura_1__c : '0'));  // PONTA 
			reqListaLetture.enAttPrelF1  = Integer.valueOf((c.CNT_Leitura_2__c != null ? c.CNT_Leitura_2__c : '0'));  // RESERVADO
			reqListaLetture.enAttPrelF3  = Integer.valueOf((c.CNT_Leitura_3__c != null ? c.CNT_Leitura_3__c : '0'));  // FORA PONTA
			reqListaLetture.enAttPrelTot = Integer.valueOf((c.CNT_Leitura_4__c != null ? c.CNT_Leitura_4__c : '0'));  // INTERMEDIARIA
		//  Tarifa Branca + Irrigante 
		}else if(c.PointofDelivery__r.CNT_Rural_Irrigating__c == true && c.PointofDelivery__r.CNT_White_Rate__c == true){
			System.debug('ENCERRAMENTO TIPO BRANCA + IRRIGANTE');
			reqListaLetture.enAttPrelF2  = Integer.valueOf((c.CNT_Leitura_1__c != null ? c.CNT_Leitura_1__c : '0'));  // PONTA 
			reqListaLetture.enAttPrelF1  = Integer.valueOf((c.CNT_Leitura_2__c != null ? c.CNT_Leitura_2__c : '0'));  // RESERVADO
			reqListaLetture.enAttPrelF3  = Integer.valueOf((c.CNT_Leitura_3__c != null ? c.CNT_Leitura_3__c : '0'));  // FORA PONTA
			reqListaLetture.enAttPrelTot = Integer.valueOf((c.CNT_Leitura_4__c != null ? c.CNT_Leitura_4__c : '0'));  // INTERMEDIARIA
		}else if(c.PointofDelivery__r.CNT_White_Rate__c == true){
		//  Tarifa Branca 
			System.debug('ENCERRAMENTO TIPO BRANCA');
			reqListaLetture.enAttPrelF2  = Integer.valueOf((c.CNT_Leitura_1__c != null ? c.CNT_Leitura_1__c : '0'));  // PONTA 
			reqListaLetture.enAttPrelTot = Integer.valueOf((c.CNT_Leitura_2__c != null ? c.CNT_Leitura_2__c : '0'));  // INTERMEDIARIA
			reqListaLetture.enAttPrelF3  = Integer.valueOf((c.CNT_Leitura_3__c != null ? c.CNT_Leitura_3__c : '0'));  // FORA PONTA  
			reqListaLetture.enAttPrelF1  = Integer.valueOf((c.CNT_Leitura_4__c != null ? c.CNT_Leitura_4__c : '0'));  // ----------	
		}else if(c.PointofDelivery__r.CNT_Rural_Irrigating__c == true){
		// Grupo B Irrigante
		System.debug('ENCERRAMENTO TIPO IRRIGANTE');
			reqListaLetture.enAttPrelF1  = Integer.valueOf((c.CNT_Leitura_1__c != null ? c.CNT_Leitura_1__c : '0'));  // RESERVADO	
			reqListaLetture.enAttPrelF3  = Integer.valueOf((c.CNT_Leitura_2__c != null ? c.CNT_Leitura_2__c : '0'));  // FORA PONTA
			reqListaLetture.enAttPrelF2  = Integer.valueOf((c.CNT_Leitura_3__c != null ? c.CNT_Leitura_3__c : '0'));  // ---------- 
			reqListaLetture.enAttPrelTot = Integer.valueOf((c.CNT_Leitura_4__c != null ? c.CNT_Leitura_4__c : '0'));  // ----------
		}else{
		// Grupo B Padrão
		System.debug('ENCERRAMENTO TIPO PADRÃO');
			reqListaLetture.enAttPrelF3  = Integer.valueOf((c.CNT_Leitura_1__c != null ? c.CNT_Leitura_1__c : '0'));  // FORA PONTA
			reqListaLetture.enAttPrelF2  = Integer.valueOf((c.CNT_Leitura_2__c != null ? c.CNT_Leitura_2__c : '0'));  // ---------- 
			reqListaLetture.enAttPrelF1  = Integer.valueOf((c.CNT_Leitura_3__c != null ? c.CNT_Leitura_3__c : '0'));  // ----------	
			reqListaLetture.enAttPrelTot = Integer.valueOf((c.CNT_Leitura_4__c != null ? c.CNT_Leitura_4__c : '0'));  // ----------
		}
		
		
				
		// Campos Obrogatórios
		reqBody.TIPO_LAVORO = 'AD';  // TIPO DE LAVORO  --> ENCERRAMENTO
		reqListaLetture.SourceType = 'ADJUSTE';		//APENAS LEITURA
		reqAnagrafica.ENELTEL_PRECEDENTE = '';		
		reqAnagrafica.ENELTEL = 'C'+ c.PointofDelivery__r.PointofDeliveryNumber__c.leftPad(8,'0');		//OBG
		reqListaLetture.podId = 'BR102E'+c.Asset.PointofDelivery__r.name.leftPad(8,'0');
		

        reqListaLetture.potAttPrelF1 = 0;
        reqListaLetture.potAttPrelF2 = 0;    
		reqListaLetture.potAtt_prelF3 = 0.0;
        reqListaLetture.enReatF1 = 0;
        reqListaLetture.enReatF2 = 0;
        reqListaLetture.enReatF3 = 0;  //DEVE SER ZERO;
        reqListaLetture.piccoEnReatF2 = 0;  //DEVE SER ZERO        
        reqListaLetture.piccoEnReatF3 = 0;
		System.debug('>>> c.PointofDelivery__r.MeterBrand__c '+listDvc[0].MeterBrand__c);
        reqListaLetture.manufacturer =  (listDvc[0].MeterBrand__c != null ? listDvc[0].MeterBrand__c : '000'); //'CPN';   //DEVE SER A SIGLA DA MARCA DO MEDIDOR
		//reqListaLetture.podId = c.PointofDelivery__r.Name; // c.PointofDelivery__r.Name;  /// 00012455  --- numero do cliente sem a letr C
           //'BR102E00012455';
		System.debug('>>> c manufacturer '+listDvc[0].MeterBrand__c);
		reqListaLetture.readingdateContatore = String.valueOf(Date.today())+'T05:29:10Z'; // '2019-03-19T05:29:10Z';  //String.valueOf(Date.today());
        reqListaLetture.readingdateSistema = String.valueOf(Date.today())+'T05:29:10Z';  // '2019-03-19T05:29:10Z';  // '20180123000000';
        reqListaLetture.enReatQ2F3  = 0;  
        reqListaLetture.enReatQ2F2  = 0;
        reqListaLetture.enReatQ2F1  = 0;
        reqListaLetture.enReatQ1Tot = 0;
        reqListaLetture.enReatF3    = 0;
        reqListaLetture.enAttImmTot = 0;
        reqListaLetture.enAttImmF3  = 0;
        reqListaLetture.enAttImmF2  = 0;
        reqListaLetture.enAttImmF1  = 0;
        //reqListaLetture.codiceMisuratore = '002CPN308'; // não precisa mais enviar  ///apagar valor depois de testar    
        reqListaLetture.anomalyReasoncode ='';
        reqListaLetture.anomalyCode = '';

        // CRIANDO O OBJETO Anagrafica
        //CNT_IntegrationHelper.Anagrafica reqAnagrafica = new CNT_IntegrationHelper.Anagrafica(); 
        reqAnagrafica.codSocieta = (c.Account.CompanyID__c == '2003' ? 'CC01' : 'AA01');	//OBG COELCE = CC01  : AMPLA = AA01
        //reqAnagrafica.ENELTEL = 'C00012455'; // AQUI SERÁ O NUMERO DO CLIENTE CONCATENADO
		System.debug('>>> c' + reqAnagrafica.ENELTEL);
        reqAnagrafica.DATA_EFFICACIA = String.valueOf(Date.today());
        reqAnagrafica.CODICE_PRESA = 'AD';
        reqAnagrafica.CODICE_ZONA = '';
        reqAnagrafica.COST_E = 1; // 
        reqAnagrafica.COST_P = 1;
        reqAnagrafica.DATA_DECORRENZA_MIS = '';
        reqAnagrafica.DT_CONTRATTO = '';
        reqAnagrafica.FLAG_TG = '';
        reqAnagrafica.ID_TIPO_FORNIT = '';
        //reqAnagrafica.MATRICOLA_MIS = '03233101';  //CORRIGIDO , NUMERO DE SERIE DO 
		reqAnagrafica.MATRICOLA_MIS =  (listDvc[0].MeterNumber__c != null ? listDvc[0].MeterNumber__c : '00000000').leftPad(8,'0');
		reqAnagrafica.TENS_FOR = '0';
		System.debug('>>> c' + reqAnagrafica.MATRICOLA_MIS);
        //reqAnagrafica.pod = '';
		reqAnagrafica.pod = 'BR102E'+c.PointofDelivery__r.Name.leftPad(8,'0');   //'BR102E00012455';
        reqAnagrafica.STATO_PRESA = '';
        reqAnagrafica.tensione = 13200; //deve ter
        reqAnagrafica.TIPO_MISURATORE = 'DIRETA';
        reqAnagrafica.CFT = '116';
        reqAnagrafica.VIA_ESAZ = '';
        reqAnagrafica.VIA = '';
		reqAnagrafica.VALORE_PERC = 0;
		reqAnagrafica.TIPO_TARIFFA_DESC = '';
		reqAnagrafica.TIPO_TARIFFA = '';
		reqAnagrafica.TIPO_RIPROGR = 0;
		reqAnagrafica.TIPO_OPZIONE = '';
		reqAnagrafica.TIPO_CLIENTE = '';
		reqAnagrafica.TARIFFARIO = '';
		reqAnagrafica.STATO_TIPO_MIS = 0;
		reqAnagrafica.STATO_CLIENTE = 0;
        reqAnagrafica.SOTTOSTATO_PRESA = '';
		reqAnagrafica.SOM_POT = 0;
		reqAnagrafica.SITUAZ_FORN = '';
		reqAnagrafica.SCALA = '';
		reqAnagrafica.SALDO = 0;
		reqAnagrafica.RECORRIDO = 0;
		reqAnagrafica.RAPP_2 = '';
		reqAnagrafica.RAPP_1 = '';
		reqAnagrafica.RAGIONE_SOCIALE = '';
		reqAnagrafica.PROVINCIA_ESAZ = '';
        reqAnagrafica.PUNTO_PRODUTTORE = '0';
        reqAnagrafica.PROVINCIA = '';
		reqAnagrafica.PREFISSO = '';
		reqAnagrafica.PRECINTO_T2T3_N4 = '';
		reqAnagrafica.PRECINTO_T2T3_N3 = '';
		reqAnagrafica.PRECINTO_T2T3_N2 = '';
		reqAnagrafica.PRECINTO_T2T3_N1 = '';
		reqAnagrafica.PRECINTO_T1 = '';
		reqAnagrafica.POTENZA_PRODUZIONE = 0;
		reqAnagrafica.POTENZA_FRANCHIGIA = 0;
		reqAnagrafica.POT_IMPEGNATA = 0;
		reqAnagrafica.POT_DISP = 0;
		reqAnagrafica.POT_CONTR_COTTIMO = 0;
		reqAnagrafica.POT_AGG_DETRAZ_F3_2 = 0;
		//reqAnagrafica.POD = 'BR102E00012455';
		reqAnagrafica.POD = 'BR102E'+c.PointofDelivery__r.Name.leftPad(8,'0');   //'BR102E00012455';
		reqAnagrafica.PIANO = '';
		reqAnagrafica.PERC_MAGGIORAZIONE = '';
		reqAnagrafica.PARTITA_IVA = '';
		reqAnagrafica.ORE_UTILIZZO = '';
		reqAnagrafica.NUM_TEL1 = '';
		reqAnagrafica.NUM_CIV = '';
		reqAnagrafica.NOTE_ACCESSO = '';
		reqAnagrafica.NON_DISALIMENTABILE = '';
		reqAnagrafica.NOMINATIVO = '';
		reqAnagrafica.NOME = '';
		reqAnagrafica.NAZIONE_ESAZ = '';
		reqAnagrafica.MATR_MISURATORE_REA = '';
		reqAnagrafica.MATR_4_TA = '';
		reqAnagrafica.MATR_3_TA = '';
		reqAnagrafica.MATR_2_TA = '';
		reqAnagrafica.MATR_1_TA = '';
		reqAnagrafica.LONGITUDINE = 0;
        reqAnagrafica.LOCALITA = '';
        reqAnagrafica.LETTURA_REA_F4 = '';
        reqAnagrafica.LETTURA_REA_F3 = '';
        reqAnagrafica.LETTURA_REA_F2 = '';
        reqAnagrafica.LETTURA_REA_F1 = '';
        reqAnagrafica.LETTURA_POT_F4 = '';
        reqAnagrafica.LETTURA_POT_F3 = '';
        reqAnagrafica.LETTURA_POT_F2 = '';
        reqAnagrafica.LETTURA_POT_F1 = '';
        reqAnagrafica.LETTURA_ATT_F4 = '';
        reqAnagrafica.LETTURA_ATT_F3 = '';
        reqAnagrafica.LETTURA_ATT_F2 = '';
        reqAnagrafica.LETTURA_ATT_F1 = '';
        reqAnagrafica.LATITUDINE = 0;
		reqAnagrafica.INTERSECCION = '';
        reqAnagrafica.INTERNO = '';
        reqAnagrafica.INICIAL = 0;
        reqAnagrafica.ID_TIPO_FORNIT = '00';
        reqAnagrafica.ID_FASE = '';
        reqAnagrafica.ID_DISPACC = '';
        reqAnagrafica.GESTORE_RETE = '';
        reqAnagrafica.FREQUENZA_LETTURA = '';
        reqAnagrafica.FONDO_SCALA = '';
        reqAnagrafica.FLAG_TG = 'N';
        reqAnagrafica.FLAG_SWITCH_CESSATO = '';
        reqAnagrafica.FLAG_MISURATORE_INTERNO = '';
        reqAnagrafica.FLAG_MIS_POT = '';
        reqAnagrafica.FLAG_MANDATO_CONNESSIONE = '';
        reqAnagrafica.ENERGIA_AD_F3_REL = '';
        reqAnagrafica.ENERGIA_AD_F3_ASS = '';
        reqAnagrafica.ENER_ANNUA_COTT = '';
        reqAnagrafica.DT_FIRMA_CONTR_CONTRAE_STR = '';
        reqAnagrafica.DT_FIRMA_CONTR_COMMITT_STR = '';
        reqAnagrafica.DT_CONTRATTO = String.valueOf(Date.today())+'T05:00:00Z';//'2019-03-19T05:29:10Z';
        reqAnagrafica.DT_ALLACCIAMENTO = String.valueOf(Date.today())+'T00:00:00Z'; //'2019-03-19T05:29:10Z';
        reqAnagrafica.DETERMINAZ_CONTI_CONTRATTI = '';
        reqAnagrafica.DATA_EFFICACIA =  String.valueOf(Date.today())+'T00:00:00Z'; //'2019-03-19T05:29:10Z';
        reqAnagrafica.DATA_DECORRENZA_MIS = String.valueOf(Date.today())+'T00:00:00Z'; //'2019-03-19T05:29:10Z';
        reqAnagrafica.DATA_CESSAZIONE =  String.valueOf(Date.today())+'T00:00:00Z';//'2019-03-19T05:29:10Z';
        reqAnagrafica.COST_P = 1;
        reqAnagrafica.COST_E = 1;        
		reqAnagrafica.CONST_ENRG_UFER_REATTIVA_HR_F5 = 0;
        reqAnagrafica.CONST_ENRG_UFER_HP_REATTIVA_F1 = 0;
        reqAnagrafica.CONST_ENRG_UFER_FP_REATTIVA_F3 = 0;
        reqAnagrafica.CONST_ENRG_PTNZ_ATTIVA_HR_F1 = 0;
        reqAnagrafica.CONST_ENRG_PTNZ_ATTIVA_HP_F2 = 0;
        reqAnagrafica.CONST_ENRG_PTNZ_ATTIVA_FP_F3 = 0;
        reqAnagrafica.CONST_ENRG_DNCR_REATTIVA_HR_F6 = 0;
        reqAnagrafica.CONST_ENRG_DNCR_REATTIVA_FP_F4 = 0;
        reqAnagrafica.CONST_ENRG_DNCR_HP_REATTIVA_F2 = 0;
        reqAnagrafica.CONST_ENRG_ATTIVA_HR_F1 = 0;
        reqAnagrafica.CONST_ENRG_ATTIVA_HP_F2 = 0;
        reqAnagrafica.CONST_ENRG_ATTIVA_FR_F3 = 0;
        reqAnagrafica.COMUNE_ESAZ = '';
		reqAnagrafica.COMPONENTE_AUC = '';
		reqAnagrafica.COGNOME = '';
		reqAnagrafica.CODIGO_PARTIDO = '';
		reqAnagrafica.CODIGO_NRO_CONFIG = 0;
		//reqAnagrafica.CODICE_ZONA = 'CA13';  // (C= EMPRESA C DE COELCE)   (A= Grupo A ou Grupo B)   (13 = setor do cliente)
		reqAnagrafica.CODICE_PRESA = '';
		reqAnagrafica.CODICE_GRUPPO = '';
		reqAnagrafica.CODICE_FISCALE = '';
		reqAnagrafica.COD_U = '';
		reqAnagrafica.COD_MISURATORE_REA = '';
		reqAnagrafica.COD_MISURATORE_POT = '';
		reqAnagrafica.COD_INSERZIONE_POTENZA = '';
		reqAnagrafica.COD_ESAZIONE = '';
		reqAnagrafica.COD_CONTRIBUTO = '';
		reqAnagrafica.CLIENTE_SPECIALE = '';
		reqAnagrafica.CLASSE_E_SUB = '';
		reqAnagrafica.CLASSE = '';
		reqAnagrafica.CIFRE_REATTIVA = '';
		reqAnagrafica.CIFRE_POTENZA = '';
		reqAnagrafica.CIFRE_DECIMALI_REATTIVA = '';
		reqAnagrafica.CIFRE_DECIMALI_POTENZA = '';
		reqAnagrafica.CIFRE_DECIMALI_ATTIVA = '';
		reqAnagrafica.CIFRE_ATT = '';
		reqAnagrafica.CFT = '';
		reqAnagrafica.CD_CL_MERCEOLOGICA = '';
		reqAnagrafica.CAP_ESAZ = '';
		reqAnagrafica.CAP = '';
		reqAnagrafica.ADVLECTURISTA = '';   

        reqBody.Anagrafica = reqAnagrafica;
        reqBody.ListaLetture = (new List<CNT_IntegrationHelper.ListaLetture>{reqListaLetture});
        req.Header = reqHeader; 
        req.Body = reqBody;
        
		System.debug(':: req >>> ' +req);

        return req;  
    }
     
    
    //Distributed Generation
    // By artur.miranda
    public static CNT_IntegrationHelper.CCC_BR_242 flow_CCC_GD_SAP_BR(Id caseId){
        System.debug('::Running flow_CCC_GD_SAP_BR');
        System.debug('::caseId: '+ caseId);
        
        Case gdCase = [Select Id, CaseNumber, PointofDelivery__r.Name, PointofDelivery__r.PointofDeliveryNumber__c, PointofDelivery__c, SubCauseBR__c, Account.CompanyID__c, CNT_Observation__c, Description, CNT_Free_Client__c, CNT_Change_Type__c, RecordType.DeveloperName,  CNT_Contract__c From Case Where Id =: caseId];
        
        String codSistema;
        if (gdCase.Account.CompanyID__c == '2005'){
            CodSistema = 'AMPSAP';
        }
        else{
            CodSistema = 'COESAP';
        }
        
        Id orderId = null;
        String changeType = '';
        if(gdCase.RecordType.DeveloperName == 'CNT_BR_Alta_Contrato'){// Executado apos alta com GD
            orderId = [Select Id From NE__Order__c Where CNT_Case__c =: caseId And RecordType.DeveloperName = 'Order'].Id;
        } else {// Executado em Alteração Contratual
            orderId = [Select Id From NE__Order__c Where CNT_Case__c =: caseId And RecordType.DeveloperName = 'Order' And CNT_Change_Type__c != null].Id;
            changeType = gdCase.CNT_Change_Type__c.substring(0,gdCase.CNT_Change_Type__c.indexOf('-')).trim();
        }
        
        Contract cont = [Select Id, CNT_ExternalContract_ID__c, CNT_Generation_Type__c, CNT_Free_Client__c, StartDate From Contract Where Id =: gdCase.CNT_Contract__c];
        
        AttributeBR att = getAttributes(orderId);
        
        System.debug('CHANGE TYPE >>>>>>>' + changeType);
        
        //prepare data
        CNT_IntegrationHelper.CCC_BR_242 req = new CNT_IntegrationHelper.CCC_BR_242();
        
        CNT_IntegrationHelper.Header reqHeader = new CNT_IntegrationHelper.Header();
        reqHeader.CodSistema = codSistema;
        reqHeader.SistemaOrigen = 'SFDC';
        reqHeader.FechaHora = date.today();
        reqHeader.Funcionalidad = 'Troca Contrato';
        
        CNT_IntegrationHelper.BodyCCC_242 reqBody = new CNT_IntegrationHelper.BodyCCC_242();
        
        reqBody.ID_FO = gdCase.CaseNumber;
        reqBody.PARTNER = cont.CNT_ExternalContract_ID__c;
        reqBody.ANLAGE = gdCase.PointofDelivery__r.PointofDeliveryNumber__c;//sumisistro
        reqBody.CAUSALE = 'GENERATION';
        reqBody.TIPOOPERAZIONE = 'CHANGE';
        reqBody.INSTALLED_LOAD = '';
        reqBody.TARIFTYP = '';
        reqBody.TEMP_AREA = '';
        reqBody.IM_DI_CONTRAT = 0;
		String resp = getDateLastVisit(caseId);
		if (String.isNotBlank(resp)){
			reqBody.VENDE = resp;
		}
		else{
			reqBody.VENDE = String.valueOf(System.Today());
		}
        reqBody.ID_RICHIESTA = '';

        // Em teste
        // By artur.miranda 2018-12-19
        //List<CNT_IntegrationHelper.ListDistributedsGeneration> distributedList = CNT_VFC023_Distributed_Generation.getDistributedGenerationListUpdated(cont, gdCase, false);

        /**/
        // Beneficiarios da Geracao distribuida
        List<CNT_IntegrationHelper.ListDistributedsGeneration> distributedList = new List<CNT_IntegrationHelper.ListDistributedsGeneration>();

        List<CNT_Distributed_Generation__c> beneficiadosList = [SELECT Id, CNT_Benefited__c, CNT_Benefited__r.PointofDeliveryNumber__c, CNT_Percentage__c, CNT_Status__c FROM CNT_Distributed_Generation__c WHERE CNT_Donator__c =: cont.Id AND CNT_Benefited__c != null];
        
        Map<String, CNT_IntegrationHelper.ListDistributedsGeneration> beneficiadosMap = new Map<String, CNT_IntegrationHelper.ListDistributedsGeneration>(); 
        
        Boolean isRemoveGD = (gdCase.SubCauseBR__c == '45')? true: false; // 45 => Retirada de Fornecimento de GD

        List<String> suministros = new List<String>();
        if(!isRemoveGD && cont.CNT_Generation_Type__c != null && cont.CNT_Generation_Type__c.equals('Consumo_Local')) {
           
            System.debug('::Generation_Type: Consumo_Local');

            CNT_IntegrationHelper.ListDistributedsGeneration podDistributed = new CNT_IntegrationHelper.ListDistributedsGeneration();
            podDistributed.RATIO = '100';
            podDistributed.ANLAGE_RATIO = gdCase.PointofDelivery__r.PointofDeliveryNumber__c;
            //distributedList.add(podDistributed);
            beneficiadosMap.put(gdCase.PointofDelivery__r.PointofDeliveryNumber__c, podDistributed );
            //suministros.add(gdClient.CNT_Benefited__r.PointofDeliveryNumber__c);
        }else{

            List<CNT_Distributed_Generation__c> removeBeneficiados = new List<CNT_Distributed_Generation__c>();
            Set<String> setTypeGd = new Set<String>{'Auto_Consumo_Remoto','Geracao_Compartilhada'};
            
            if(!isRemoveGD && setTypeGd.contains(cont.CNT_Generation_Type__c)) {
                CNT_IntegrationHelper.ListDistributedsGeneration newDistributed = new CNT_IntegrationHelper.ListDistributedsGeneration();
                newDistributed.RATIO = '100';
                newDistributed.ANLAGE_RATIO = gdCase.PointofDelivery__r.PointofDeliveryNumber__c;
                beneficiadosMap.put(gdCase.PointofDelivery__r.PointofDeliveryNumber__c, newDistributed );
            }

            for(CNT_Distributed_Generation__c beneficiado : beneficiadosList ){
                CNT_IntegrationHelper.ListDistributedsGeneration podDistributed = new CNT_IntegrationHelper.ListDistributedsGeneration();
                    
                if(!isRemoveGD &&  setTypeGd.contains(cont.CNT_Generation_Type__c) && gdCase.PointofDelivery__r.PointofDeliveryNumber__c == beneficiado.CNT_Benefited__r.PointofDeliveryNumber__c) {
                    beneficiado.CNT_Percentage__c += 100;
                }
                podDistributed.RATIO = String.valueOf(beneficiado.CNT_Percentage__c);
                podDistributed.ANLAGE_RATIO = beneficiado.CNT_Benefited__r.PointofDeliveryNumber__c;
                //distributedList.add(podDistributed);
                beneficiadosMap.put(beneficiado.CNT_Benefited__r.PointofDeliveryNumber__c, podDistributed );
                if(beneficiado.CNT_Percentage__c == 0.00){// Remove suministros with 0 percent
                    removeBeneficiados.add(beneficiado);
                } else {// Update PoD status in (CNT_IsGD__c) 
                    suministros.add(beneficiado.CNT_Benefited__r.PointofDeliveryNumber__c);
                }
            }


            System.debug('::beneficiadosMap: '+beneficiadosMap);
            System.debug('::removeBeneficiados: '+removeBeneficiados);
            if(removeBeneficiados.size() > 0){
                System.debug('::Removing... ' + removeBeneficiados);
                //delete removeBeneficiados;
            }
        }

        List<PointofDelivery__c> updatedPoDList = new List<PointofDelivery__c>();
        PointofDelivery__c podFornecedor = new PointofDelivery__c(Id=gdCase.PointofDelivery__c, PointofDeliveryNumber__c=gdCase.PointofDelivery__r.PointofDeliveryNumber__c);
        podFornecedor.CNT_IsGD__c = '1';// Fornecedor
        System.debug('::Fornecedor: '+podFornecedor);
        updatedPoDList.add(podFornecedor);
        if(cont.CNT_Generation_Type__c != null && !cont.CNT_Generation_Type__c.equals('Consumo_Local')) {
            List<PointofDelivery__c> updatePoDStatus = [SELECT CNT_IsGD__c, PointofDeliveryNumber__c FROM PointofDelivery__c WHERE PointofDeliveryNumber__c IN: suministros];
            for(PointofDelivery__c pod : updatePoDStatus){
                if(pod.PointofDeliveryNumber__c != podFornecedor.PointofDeliveryNumber__c){
                    pod.CNT_IsGD__c = '2';// 1 - Fornecedor; 2 - Beneficiado
                    updatedPoDList.add(pod);
                }
            }
        }
        System.debug('::updatedPoDList: '+updatedPoDList);
        //if(updatedPoDList.size() > 0){
            //update updatedPoDList;
        //}
        /**/        
        //System.debug('::distributedList: '+distributedList);
        //reqBody.LISTDISTRIBUTEDSGENERATION = distributedList;
        reqBody.LISTDISTRIBUTEDSGENERATION = beneficiadosMap.values();
        if (gdCase.CNT_Free_Client__c){ //cliente livre
            if(String.isNotBlank(att.ModalidadeTarifaria) && att.ModalidadeTarifaria.contains('Azul')){
                reqBody.IM_DI_CONTPTL = att.DemandaPonta;
                reqBody.IM_DI_CONTFPL = att.DemandaForaPonta;
                reqBody.IM_DI_CONTRPT = 0;
                reqBody.IM_DI_CONTRFP = 0;
                reqBody.IM_DI_CONTUL = 0;
            } else if(String.isNotBlank(att.ModalidadeTarifaria) && att.ModalidadeTarifaria.contains('Verde')){
                reqBody.IM_DI_CONTPTL = att.DemandaPonta;
                reqBody.IM_DI_CONTFPL = att.DemandaForaPonta;
                reqBody.IM_DI_CONTRPT = 0;
                reqBody.IM_DI_CONTRFP = 0;
                reqBody.IM_DI_CONTUL = att.Demanda;
            }

        }
        else{
            if(String.isNotBlank(att.ModalidadeTarifaria) && att.ModalidadeTarifaria.contains('Azul')){
                reqBody.IM_DI_CONTRPT = att.DemandaPonta;
                reqBody.IM_DI_CONTRFP = att.DemandaForaPonta;
            } else if(String.isNotBlank(att.ModalidadeTarifaria) && att.ModalidadeTarifaria.contains('Verde')){
                reqBody.IM_DI_CONTRPT = 0;
                reqBody.IM_DI_CONTRFP = 0;
            } else {
                reqBody.IM_DI_CONTRPT = 0;
                reqBody.IM_DI_CONTRFP = 0;
            }
            reqBody.IM_DI_CONTUL = 0;
            reqBody.IM_DI_CONTFPL = 0;
            reqBody.IM_DI_CONTPTL = 0;
            reqBody.IM_DI_CONTRAT = 0;
            
        }
        
        req.Header = reqHeader;
        req.Body = reqBody;
        
        return req;
    }
    
    // Cambio de Condiçoes Contratuais
    public static CNT_IntegrationHelper.CCC_BR_242 flow_CCC_SAP_BR(Id caseId){
        System.debug('::Running flow_CCC_SAP_BR');

        Case c = [Select Id, CaseNumber, PointofDelivery__r.Name, PointofDelivery__c, RecordType.DeveloperName,
                  Account.CompanyID__c, CNT_Observation__c, Description, CNT_Free_Client__c, CNT_Change_Type__c,
                  CNT_Transit_Date_To__c,
                  CNT_Contract__c, CNT_Contract__r.ContractNumber, CNT_Contract__r.CNT_ExternalContract_ID_2__c,
                  CNT_Public_Ilumination__c, CNT_Contract__r.CNT_Free_Client__c, CNT_Contract__r.StartDate
                  From Case Where Id =: caseId];
                  
        Id orderId = [Select Id From NE__Order__c Where CNT_Case__c =: caseId And RecordType.DeveloperName = 'Order' And CNT_Change_Type__c != null].Id;
        
        String clienteNumber = (!String.isBlank(c.CNT_Contract__r.CNT_ExternalContract_ID_2__c))? c.CNT_Contract__r.CNT_ExternalContract_ID_2__c: c.CNT_Contract__r.ContractNumber;
        
        AttributeBR att = getAttributes(orderId);
        
        String codSistema;
        if (c.Account.CompanyID__c == '2005'){
            CodSistema = 'AMPSAP';
        }else{
            CodSistema = 'COESAP';
        }
        
        String changeType = c.CNT_Change_Type__c.substring(0,c.CNT_Change_Type__c.indexOf('-')).trim();
        
        System.debug('CHANGE TYPE >>>>>>>' + changeType);
        
        //prepare data
        CNT_IntegrationHelper.CCC_BR_242 req = new CNT_IntegrationHelper.CCC_BR_242();
        
        CNT_IntegrationHelper.Header reqHeader = new CNT_IntegrationHelper.Header();
        reqHeader.CodSistema = codSistema;
        reqHeader.SistemaOrigen = 'SFDC';
        reqHeader.FechaHora = date.today();
        reqHeader.Funcionalidad = 'Troca Contrato';
        
        CNT_IntegrationHelper.BodyCCC_242 reqBody = new CNT_IntegrationHelper.BodyCCC_242();
        
        reqBody.INSTALLED_LOAD = '';
        
        reqBody.TIPOOPERAZIONE = 'CHANGE';
        reqBody.ID_RICHIESTA = '';
        reqBody.ID_FO = c.CaseNumber;
        reqBody.PARTNER = clienteNumber;
        reqBody.ANLAGE = c.PointofDelivery__r.Name;
        reqBody.TARIFTYP = att.Tarifa;
		reqBody.AC_KOFIZ_SD = att.Classe;
		


        List<CNT_IntegrationHelper.ListDistributedsGeneration> distributedList = new List<CNT_IntegrationHelper.ListDistributedsGeneration>();
        CNT_IntegrationHelper.ListDistributedsGeneration podDistributed = new CNT_IntegrationHelper.ListDistributedsGeneration();
        podDistributed.RATIO = '';
        podDistributed.ANLAGE_RATIO = '';
        distributedList.add(podDistributed);
        reqBody.LISTDISTRIBUTEDSGENERATION = distributedList;

        // Iluminação pública - Wanderson Dantas
        List<CNT_IntegrationHelper.ListIluminacaoPublica> iluminacaoList = new List<CNT_IntegrationHelper.ListIluminacaoPublica>();
        CNT_IntegrationHelper.ListIluminacaoPublica caseIlumPublica = new CNT_IntegrationHelper.ListIluminacaoPublica();
        if (c.CNT_Public_Ilumination__c) {
            System.debug(':: CNT_Public_Ilumination__c >>> '+c.CNT_Public_Ilumination__c);
            List<CNT_Lamp__c> lamps = [SELECT id, CNT_Quantity__c, CNT_Lamp_Code__c, CNT_Charge__c, CNT_Totalizer__c, CNT_Loss__c FROM CNT_Lamp__c WHERE CNT_Contract__c =: c.CNT_Contract__c];
            Map<Decimal, CNT_Lamp__c> mapLamps = new Map<Decimal, CNT_Lamp__c>();
            for(CNT_Lamp__c lamp: lamps) {
                if(lamp.CNT_Quantity__c > 0 && !mapLamps.containsKey(lamp.CNT_Lamp_Code__c)){
                    mapLamps.put(lamp.CNT_Lamp_Code__c, lamp);
                }
            }
            System.debug('mapLamps --> '+mapLamps);

            for(CNT_Lamp__c lamp: mapLamps.values()) {
                caseIlumPublica = new CNT_IntegrationHelper.ListIluminacaoPublica();
                caseIlumPublica.CODE_TYPE = String.valueOf(lamp.CNT_Lamp_Code__c);
                Decimal podency = 0.00;
                if(lamp.CNT_Charge__c > 0 ){
                    podency =  lamp.CNT_Charge__c.divide(1000, 8);
                }

                caseIlumPublica.POTENCY = String.valueOf(podency);
                caseIlumPublica.FACTOR = String.valueOf(lamp.CNT_Loss__c);
                caseIlumPublica.QUANTITY = lamp.CNT_Totalizer__c;
                iluminacaoList.add(caseIlumPublica);
            }
            reqBody.ILUMI_PUB = iluminacaoList;
        } else {
            caseIlumPublica.CODE_TYPE = '0';
            caseIlumPublica.POTENCY = '0';
            caseIlumPublica.FACTOR = '0';
            caseIlumPublica.QUANTITY = 0;
            iluminacaoList.add(caseIlumPublica);
            reqBody.ILUMI_PUB = iluminacaoList;
        }
        System.debug(':: LISTILUMINACAOPUBLICA>>> '+reqBody.ILUMI_PUB);

        // Novos campos 09-05-2018
        //reqBody.RATIO = '';
        //reqBody.ANLAGE_RATIO = '';
        reqBody.TEMP_AREA = att.SubClasse;
        if(c.CNT_Contract__r.CNT_Free_Client__c){
            reqBody.VENDE = String.valueOf(c.CNT_Contract__r.StartDate);
        }else{
			String resp = getDateLastVisit(caseId);
			if (String.isNotBlank(resp)){
				reqBody.VENDE = resp;
			}
			else{
				reqBody.VENDE = String.valueOf(System.Today());
			}
		}

        if (c.CNT_Free_Client__c){ //cliente livre
            // changed artur.miranda 12-06-2018 (Verified with SAP e Mulesoft) 
            if(String.isNotBlank(att.ModalidadeTarifaria) && att.ModalidadeTarifaria.contains('Azul')){
                reqBody.DI_CONTPTL = String.valueOf(att.DemandaPonta);
                reqBody.DI_CONTFPL = String.valueOf(att.DemandaForaPonta);
                reqBody.DI_CONTUL = '';
                reqBody.DI_CONTRPT = 0;
                reqBody.DI_CONTRFP = 0;
                reqBody.DI_CONTRAT = 0;
            }else if(String.isNotBlank(att.ModalidadeTarifaria) && att.ModalidadeTarifaria.contains('Verde')){
                reqBody.DI_CONTUL = String.valueOf(att.Demanda);
                reqBody.DI_CONTPTL = '';
                reqBody.DI_CONTFPL = '';
                reqBody.DI_CONTRPT = 0;
                reqBody.DI_CONTRFP = 0;
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
            } else if(String.isNotBlank(att.ModalidadeTarifaria) && att.ModalidadeTarifaria.contains('Verde')){
                reqBody.IM_DI_CONTRAT = att.Demanda;
                reqBody.IM_DI_CONTRPT = 0;
                reqBody.IM_DI_CONTRFP = 0;
                reqBody.IM_DI_CONTUL = 0;
                reqBody.IM_DI_CONTFPL = 0;
                reqBody.IM_DI_CONTPTL = 0;
            } else if((String.isNotBlank(att.ModalidadeTarifaria) && att.ModalidadeTarifaria.contains('OPTANTE')) || att.Grupo == 'B'){
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
            
            reqBody.DI_CONTPTL = '';
            reqBody.DI_CONTFPL = '';
            reqBody.DI_CONTUL = '';
            reqBody.DI_CONTRPT = 0;
            reqBody.DI_CONTRFP = 0;
            reqBody.DI_CONTRAT = 0;
        }
        
        
        if(changeType == '1') { reqBody.CAUSALE = 'TARIFF'; }
        if(changeType == '2'){
            reqBody.CAUSALE = 'LOAD'; 
            reqBody.INSTALLED_LOAD= String.valueOf(att.Carga);
            reqBody.IM_DI_CONTPTL = 0;
            reqBody.IM_DI_CONTFPL = 0;
            reqBody.IM_DI_CONTRPT = 0;
            reqBody.IM_DI_CONTRFP = 0;
            reqBody.IM_DI_CONTRAT = 0;
            reqBody.IM_DI_CONTUL = 0;
            reqBody.INSTALLED_PHASE = att.TipoTensao;       //juan passando tipo de tensao pro sap
        }
        if(changeType == '3'){ reqBody.CAUSALE = 'DEMAND';}
        if(changeType == '6'){  //Acresc Decresc de Potencia
            if(att.ChangeLoad == 'Change'){ // se houver mudança de carga 
                reqBody.INSTALLED_LOAD= String.valueOf(att.Carga);
            }
            
            if(att.ActionTarifa == 'Change'){ // houve troca de tarifa
                reqBody.CAUSALE = 'TARIFF';
                
            }else{
                reqBody.CAUSALE = 'LOAD';
            }
        }// Aumento de Potencia
        if(changeType == '5'){ reqBody.CAUSALE = 'ILLUMINATION';}// Iluminação Pública - Wanderson Dantas 17-09-2018
        if(changeType == '7'){ reqBody.CAUSALE = 'INTERIM';}//Ligação provisoria - Madson Felipe 18-12-2018

        req.Header = reqHeader;
        req.Body = reqBody;
        
        return req;
    }
    
    /* SAP - ISU */
    public static ATCL_VFC042_wsdlUtils.Put_PerfilFacturacionRequest_element flow_AF_SAP_BR(Id caseId){
        System.debug('::Run flow_ALTA_Tecnica_BR');
        
        Map<String,String> mapFieldPickLabelSegment = new Map<String,String>();
        Map<String,String> mapFieldPickLabelArea = new Map<String,String>();
        Billing_Profile__c billingProfile = new Billing_Profile__c();
        Contract contrato = new Contract();
        Case caso = new Case();
        String enderecoType = 'MA';
        String streetType = '';
        
        mapFieldPickLabelSegment = CNT_Utility.getPicklistLabel('Contract','CNT_GroupSegment__c');
        mapFieldPickLabelArea = CNT_Utility.getPicklistLabel('Contract','CNT_GroupArea__c');
        
        caso = [SELECT Id, CaseNumber, CNT_Contract__c, Account.Name, Account.Parent.CondominiumRUT__c,
                       Account.CompanyID__c, Account.IdentityType__c, Account.CNT_State_Inscription__c,
                       Account.CNT_State_Inscription_Exemption__c, CNT_Observation__c,
                       Account.CNT_GroupAssociate__r.CondominiumRUT__c, Account.IdentityNumber__c,
                       Account.ExternalId__c
                  FROM Case
                 WHERE Id =: caseId];
        
        for(Contract loopCntr : [SELECT id, ContractNumber, CNT_ExternalContract_ID__c, CNT_GroupNumerCntr__c,
                                        CNT_GroupSegment__c, CNT_GroupArea__c, CNT_GroupPayType__c, CNT_GroupClass__c
                                   FROM Contract
                                  WHERE Id =: caso.CNT_Contract__c limit 1]){ contrato = loopCntr;}
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
        if (caso.Account.CompanyID__c == '2005'){
            CodSistema = 'AMPSAP';
        }else{
            CodSistema = 'COESAP';
        }
        
        ATCL_VFC042_wsdlUtils.BodyPutPerfilFacturacionRequest_element reqBody = new ATCL_VFC042_wsdlUtils.BodyPutPerfilFacturacionRequest_element();
        ATCL_VFC042_wsdlUtils.Put_PerfilFacturacionResponse_element response = new ATCL_VFC042_wsdlUtils.Put_PerfilFacturacionResponse_element();
        ATCL_VFC042_wsdlUtils.Put_PerfilFacturacionRequest_element req = new ATCL_VFC042_wsdlUtils.Put_PerfilFacturacionRequest_element();
        ATCL_VFC042_wsdlUtils.ListaCuentaContrato_element listaCuentaContrato = new ATCL_VFC042_wsdlUtils.ListaCuentaContrato_element();
        ATCL_VFC042_wsdlUtils.HeaderRequestType reqHeader = new ATCL_VFC042_wsdlUtils.HeaderRequestType();
        
        //prepare data
        reqHeader.CodSistema = codSistema;
        reqHeader.SistemaOrigen = 'SFDC';
        reqHeader.FechaHora = String.valueOf(date.today());
        reqHeader.Funcionalidad = 'Alta';
        
        reqBody.CodigoEmpresa = caso.Account.CompanyID__c;                                    //ID_FO
        reqBody.NumeroCaso = caso.CaseNumber;                                                 //ID_FO
        reqBody.Motivo = 'GROUP';                                                             //CAUSALE
        reqBody.SubMotivo = 'DELIVERYINVOICE';                                                //TIPO_OPERAZIONE
        reqBody.CuentaContrato = contrato.CNT_GroupNumerCntr__c;                              //VKONTO
        reqBody.Cliente = caso.Account.ExternalId__c;                                         //BP
        if(reqBody.Cliente.startsWith('2003') || reqBody.Cliente.startsWith('2005')){ reqBody.Cliente = reqBody.Cliente.substring(4);  }
        reqBody.CondicionPagoCliente = (billingProfile.CNT_Due_Date__c.length() == 1 ?
                                            'CP0' + billingProfile.CNT_Due_Date__c :
                                            'CP' + billingProfile.CNT_Due_Date__c);           //ZAHLKOND
        reqBody.Calle = streetType + billingProfile.Address__r.StreetMD__r.Street__c;         //STREET
        reqBody.Numero = String.isNotBlank(billingProfile.Address__r.Number__c) ? billingProfile.Address__r.Number__c : '0' ;//HOUSENUMBER
        reqBody.CodigoPostal = billingProfile.Address__r.Postal_Code__c;                      //POST_CODE
        //reqBody.Ciudad = (billingProfile.Address__r.Municipality__c != null ?
        //                      billingProfile.Address__r.Municipality__c :
        //                      '');                                                          //CITY
		reqBody.Ciudad = (billingProfile.Address__r.Municipality__c != null ?
                              billingProfile.Address__r.Municipality__c : 'CE');              //CITY
		reqBody.Barrio = (billingProfile.Address__r.StreetMD__r.Neighbourhood__c != null ?
                              billingProfile.Address__r.StreetMD__r.Neighbourhood__c :
                              'SEM BAIRRO');                                                  //CITY2
        reqBody.Estado = (caso.Account.CompanyID__c == '2005' ? 'RJ' : 'CE');                 //REGION
        reqBody.ComplementoDireccion = '';                                                    //SUPLEMENT
        reqBody.NombreCuentaContratoInd = caso.Account.Name;                                  //VKBEZ
        reqBody.CuentaContratoColect = caso.Account.CNT_GroupAssociate__r.CondominiumRUT__c;  //CONTROLLER
        reqBody.NombreCuentaContColect = caso.Account.CNT_GroupAssociate__r.CondominiumRUT__c;//CONTROLLER
        //reqBody.TipoEnvioFactura = billingProfile.CNT_FormPayment__c;                       //OPTION_INVOICE
        reqBody.TipoEnvioFactura = (String.isNotBlank(billingProfile.CNT_GroupPayType__c)) ? billingProfile.CNT_GroupPayType__c: '';//OPTION_INVOICE
        reqBody.DireccionExterna = contrato.CNT_ExternalContract_ID__c + '_' + enderecoType;
                                                                                              //ADEXT
        if(Integer.valueOf(caso.Account.IdentityType__c) != 2){// Pessoa Fisica (B2C_BRASIL)
            reqBody.TipoCliente = '1';                                                        //BU_TYPE
            reqBody.TipoDocFiscal = 'BR2';                                                    //BP_TAXTYPE
            reqBody.NomeOrganizacao = '';                                                     //BP_NAME_ORG1
            reqBody.PrimeiroNome = (caso.Account.Name == null ?
                                        '' :
                                        caso.Account.Name.substringBefore(' '));              //BP_NAME_FIRST
            reqBody.UltimoNome = (caso.Account.Name == null ?
                                      '' :
                                      caso.Account.Name.substringAfter(' '));                 //BP_NAME_LAST
        } else { // Pessoa Juridica (B2B_BRASIL)
            reqBody.TipoCliente = '2';                                                        //BU_TYPE
            reqBody.TipoDocFiscal = 'BR1';                                                    //BP_TAXTYPE
            reqBody.NomeOrganizacao = caso.Account.Name;                                      //BP_NAME_ORG1
            reqBody.PrimeiroNome = '';                                                        //BP_NAME_FIRST
            reqBody.UltimoNome = '';                                                          //BP_NAME_LAST
        }
        reqBody.NumDocumentoFiscal = caso.Account.IdentityNumber__c;                          //BP_FISCALNUMBER
        reqBody.CaractDeterminacion = (String.isNotBlank(billingProfile.CNT_GroupClass__c)) ? billingProfile.CNT_GroupClass__c: '';                             //KOFIZ_SD
        reqBody.InscricaoEstadual = (caso.Account.CNT_State_Inscription_Exemption__c ?
                                         'ISENTO':
                                         caso.Account.CNT_State_Inscription__c);              //BP_IDNUMBER_IE
        reqBody.InscricaoMunicipal = '';                                                      //BP_IDNUMBER_IM
        reqBody.FormaPagamento = 'C';                                                         //AC_EZAWE ??????? De onde pegar essa informação
        reqBody.Segmento = contrato.CNT_GroupSegment__c;                                      //AC_SEGMENT
        reqBody.TextoSegmento = mapFieldPickLabelSegment.get(contrato.CNT_GroupSegment__c);   //AC_SEGMENT_T
        reqBody.Area = contrato.CNT_GroupArea__c;                                             //AC_AREA
        reqBody.TextoArea = mapFieldPickLabelArea.get(contrato.CNT_GroupArea__c);             //AC_AREA_T
        reqBody.Lote = (String.isNotBlank(billingProfile.CNT_Lot__c)) ? billingProfile.CNT_Lot__c : '';                                                             //AC_PORTION
        reqBody.Tipo = '';                                                                    //AC_PORTION


        reqBody.ListaCuentaContrato = new ATCL_VFC042_wsdlUtils.ListaCuentaContrato_element();
        reqBody.ListaCuentaContrato.CuentasContratos = new List<ATCL_VFC042_wsdlUtils.CuentasContratos_element>();
        for(Map<Id,ATCL_VFC042_wsdlUtils.CuentasContratos_element> loopMapCuentasContratos : ((Map<Id,Map<Id,ATCL_VFC042_wsdlUtils.CuentasContratos_element>>) JSON.deserialize(caso.CNT_Observation__c,Map<Id,Map<Id,ATCL_VFC042_wsdlUtils.CuentasContratos_element>>.class)).values()){
            for(ATCL_VFC042_wsdlUtils.CuentasContratos_element loopCuentasContratos : loopMapCuentasContratos.values()){
                reqBody.ListaCuentaContrato.CuentasContratos.add(loopCuentasContratos);
            }
        }
        
        req.HeaderRequest = reqHeader;
        req.BodyPutPerfilFacturacionRequest = reqBody;
        
        return req;
    }
    
    //cargos sd
    public static HttpResponse flow_CargoSD_SAP_BR(Id caseId){
        System.debug('::Running flow_CargoSD_SAP_BR');
        
        Case c = [Select Id, CaseNumber, Account.Name, AccountId, Contact.Email, Account.IdentityType__c, Account.IdentityNumber__c, Account.MainPhone__c, Account.ExternalId__c,
                  Account.CNT_Executive__r.CNT_Code_Executive__c, PointofDelivery__r.Name, PointofDelivery__c, Contact.FirstName, Contact.LastName, Address__r.StreetMD__r.Municipality__c, Address__r.Number__c, 
                  Address__r.Postal_Code__c, Address__r.StreetMD__r.Street__c, Address__r.StreetMD__r.Street_Type__c, Address__r.Corner__c, Account.CompanyID__c, CNT_Observation__c, Description, CNT_Economical_Activity__c, 
                  CNT_Free_Client__c, Address__r.StreetMD__r.Neighbourhood__c, Address__r.StreetMD__r.Literal_Municipality__c, Account.RecordType.DeveloperName
                  From Case Where Id =: caseId];
                  
        Id quoteId = [Select Id From NE__Order__c Where CNT_Case__c =: caseId And RecordType.DeveloperName = 'Quote'].Id;
        
        Contract cont = [Select Id, CNT_ExternalContract_ID__c From Contract Where CNT_Case__c =: c.Id];
        
        Billing_Profile__c bp = [Select CNT_Due_Date__c, Type__c From Billing_Profile__c Where Account__c =: c.AccountId Limit 1];
        
        List<CNT_Budget__c> listBudgt = [SELECT id, CNT_TotalPayable__c, CNT_BillingType__c, CNT_Slice_Number__c FROM CNT_Budget__c WHERE CNT_isClosed__c = false and CNT_Contract__c =: cont.id];
        
        String idricInvoice = '';
        if(listBudgt.get(0).CNT_BillingType__c.equalsIgnoreCase('Antecipado') && c.Account.CompanyID__c == '2003'){
            idricInvoice = 'ZB11';
        }else if(listBudgt.get(0).CNT_BillingType__c.equalsIgnoreCase('Antecipado') && c.Account.CompanyID__c == '2005'){
            idricInvoice = 'ZB51';
        }else if(listBudgt.get(0).CNT_BillingType__c.equalsIgnoreCase('Posterior') && c.Account.CompanyID__c == '2003'){
            idricInvoice = 'ZB1A';
        }else if(listBudgt.get(0).CNT_BillingType__c.equalsIgnoreCase('Posterior') && c.Account.CompanyID__c == '2003'){
            idricInvoice = 'ZB5A';
        }
        
        String formaPagamento;
        switch on listBudgt.get(0).CNT_Slice_Number__c {
            when '1' {
                formaPagamento = '0018';
            }
            when '2' {
                formaPagamento = '0023';
            }
            when '3' {
                formaPagamento = '0024';
            }
            when '4' {
                formaPagamento = '0025';
            }
            when '5' {
                formaPagamento = '0026';
            }
            when '6' {
                formaPagamento = '0027';
            }
        }
        
        AttributeBR att = getAttributes(quoteId);

        //prepare data
        CNT_IntegrationHelper.CARGO_BR_4 req = new CNT_IntegrationHelper.CARGO_BR_4();
        
        CNT_IntegrationHelper.Header reqHeader = new CNT_IntegrationHelper.Header();
        reqHeader.CodSistema = 'COESAP';
        reqHeader.SistemaOrigen = 'SFDC';
        reqHeader.FechaHora = date.today();
        reqHeader.Funcionalidad = 'Create_CargoSD';
        
        CNT_IntegrationHelper.BodyCARGO_BR_4 reqBody = new CNT_IntegrationHelper.BodyCARGO_BR_4();
        reqBody.ID_CASE_EXTERNAL = c.Id;
        reqBody.ID_RICHIESTA_FO = c.CaseNumber;
        reqBody.ID_RICHIESTA = '';
        reqBody.IDRIC_INVOICE = idricInvoice;
        reqBody.CLASSE = 'MOVE IN';
        reqBody.ATTIVITA = 'DEFINITIVE';
        reqBody.IM_TARIFTYP = att.Tarifa;
        reqBody.DATA_VALIDITA = String.valueOf(Date.today());
        reqBody.BP_OPBUK = String.valueOf(c.Account.CompanyID__c);
        reqBody.BP_PARTNER = '';
        reqBody.BP_ZZ_CODFISC = c.Account.IdentityNumber__c;
        String streetType = '';
        for (CNT_Street_Type_Setting__mdt threatMapping : [SELECT Code__c, Value__c FROM CNT_Street_Type_Setting__mdt]) {
            System.debug(threatMapping.Code__c + ':' + threatMapping.Value__c);
            if(c.Address__r.StreetMD__r.Street_Type__c == threatMapping.Code__c){
                streetType = threatMapping.Value__c + ' ';
                break;
            }
        }
        reqBody.BP_STREET =  streetType + c.Address__r.StreetMD__r.Street__c;// Street Name (Char 45)
        //reqBody.BP_STREET = c.Address__r.StreetMD__r.Street__c;
        //reqBody.BP_HOUSE_NUM1 = c.Address__r.Number__c;
		reqBody.BP_HOUSE_NUM1 = String.isNotBlank(c.Address__r.Number__c) ? c.Address__r.Number__c : '0' ;
        reqBody.BP_POST_CODE1 = c.Address__r.Postal_Code__c;
        //reqBody.BP_CITY1 = c.Address__r.StreetMD__r.Literal_Municipality__c != null ? c.Address__r.StreetMD__r.Literal_Municipality__c : '';
		reqBody.BP_CITY1 = c.Address__r.StreetMD__r.Literal_Municipality__c != null ? c.Address__r.StreetMD__r.Literal_Municipality__c : 'CE';
        reqBody.BP_REGION = c.Account.CompanyID__c == '2005' ? 'RJ' : 'CE';
        reqBody.AC_ZAHLKOND = formaPagamento;
        reqBody.AC_GSBER = '0300';
        reqBody.AC_KOFIZ_SD = att.Classe;
        reqBody.AC_VKTYP = '1';
        reqBody.AC_ABWRH = '';
        //reqBody.AC_EZAWE = 'B';

        if(  String.isNotBlank(bp.Type__c) ){
            
            if(bp.Type__c.equalsIgnoreCase('Statement')){
                 reqBody.AC_EZAWE =  'B';  // B = Boleto; C = Código de Barras; D = Débito Automático 

            }else if(bp.Type__c.equalsIgnoreCase('Automatic debit')){
                reqBody.AC_EZAWE =  'D'; 

            }else if(bp.Type__c.equalsIgnoreCase('BarCode')){
                 reqBody.AC_EZAWE =  'C';

            }
        }else{
             reqBody.AC_EZAWE =  'B';  // B = Boleto; C = Código de Barras
        }   


        reqBody.AC_VKONT = cont.CNT_ExternalContract_ID__c;
        reqBody.MI_VERTRAG = '';
        reqBody.MI_VENDE = '';
        reqBody.CO_STREET = '';
        reqBody.CO_HOUSE_NUM1 = '';
        reqBody.CO_POST_CODE1 = '';
        reqBody.CO_CITY1 = '';
        reqBody.CO_REGION = '';
        reqBody.IM_BRANCHE = '';
        reqBody.IM_SPEBENE = '';
        
        String accRecordType = c.Account.RecordType.DeveloperName;
        if(accRecordType.equalsIgnoreCase('B2C_BRASIL')){//Persona Fisica
            reqBody.BPKIND = '9001';
        } else if(accRecordType.equalsIgnoreCase('B2B_BRASIL')){// Persona Juridica
            reqBody.BPKIND = '9002';
        } else if(accRecordType.equalsIgnoreCase('B2G_BRASIL')){// Governo (Persona Juridica)
            reqBody.BPKIND = '9003';
        }
        
        reqBody.BUPLA = ' ';
        reqBody.IM_ANLAGE = '';
        reqBody.IM_FACTOR_2 = '14767';
        reqBody.IM_FACTOR_3 = '14767';
        reqBody.IM_FACTOR_4 = att.TipoTensao;
        reqBody.IM_TRASNF_LOSS = 0;
        reqBody.IM_GROUP_TENSION = '';
        reqBody.IM_REGION = att.Grupo;
        reqBody.IM_CHARGE = Integer.valueOf(att.Carga);
		//reqBody.IM_TEMP_AREA = att.SubClasse;
        reqBody.IM_TEMP_AREA = att.SubClasse == null ? 'COMER' : att.SubClasse;
        reqBody.ZZFLG_ONLINE = 'X';
        
        reqBody.IM_ZZ_NUMUTE = Integer.valueOf(cont.CNT_ExternalContract_ID__c);
        //reqBody.IM_ZZ_NUMUTE = 9898910;
        //reqBody.BP_BPEXT = c.Account.ExternalId__c;
        //reqBody.BP_BPEXT = c.Account.ExternalId__c == null ? '': c.Account.ExternalId__c;
        reqBody.BP_BPEXT = c.Account.ExternalId__c;
        reqBody.BP_OLD_ZPARTNER = '';
        if(c.Account.RecordType.DeveloperName.equalsIgnoreCase('B2C_BRASIL')){
            reqBody.BP_NAME_ORG1 = '';
            reqBody.BP_NAME_FIRST = c.Contact.FirstName == null ? '' : c.Contact.FirstName;
            reqBody.BP_NAME_LAST = c.Contact.LastName == null ? c.Contact.FirstName : c.Contact.LastName;//c.Contact.LastName;
            reqBody.BP_TYPE = '1';
            reqBody.BP_TAXTYPE = 'BR2';
        }else{
            reqBody.BP_NAME_ORG1 = c.Account.Name;
            reqBody.BP_NAME_FIRST = '';
            reqBody.BP_NAME_LAST = '';
            reqBody.BP_TYPE = '2';
            reqBody.BP_TAXTYPE = 'BR1';
        }
        
        //reqBody.BP_ADEXT_ADDR = c.Address__r.StreetMD__r.Municipality__c != null ? c.Address__r.StreetMD__r.Municipality__c : '';
        reqBody.BP_ADEXT_ADDR = c.Address__r.StreetMD__r.Municipality__c != null ? c.Address__r.StreetMD__r.Municipality__c : 'CE';
		reqBody.BP_XDFADR = '';
        reqBody.BP_CITY2 = c.Address__r.StreetMD__r.Neighbourhood__c != null ? c.Address__r.StreetMD__r.Neighbourhood__c : '';
        reqBody.BP_SMTP_ADDR = '';
        reqBody.BP_BANKS = '';
        reqBody.BP_BANKL = '';
        reqBody.BP_BANKN = '';
        reqBody.BP_IDNUMBER_IE = '';
        reqBody.BP_IDNUMBER_IM = '';
        reqBody.BP_IDNUMBER_NIS = '';
        reqBody.BP_IDNUMBER_NB = '';
        reqBody.BP_IDNUMBER_RANI = '';
        reqBody.BP_BKEXT = '';
        reqBody.BP_MUSTER_KUN = '';
        reqBody.BP_HOUSE_NUM2_MA = '';
        reqBody.AC_VKTYP = '';
        reqBody.AC_ABWMA = '';
        reqBody.AC_EBVTY = '';
        reqBody.COPRC = '';
        reqBody.BP_COUNTRY = 'BR';
        reqBody.IM_ZZPASSO_FATT = '';
        reqBody.AC_ZZ_STATO_DOM = '';
        reqBody.LEGAL_ENTY = '';
        reqBody.ACCNAME = '';
        reqBody.CCINS = '';
        reqBody.CCNUM = '';
        reqBody.ICOM_TEL_NUMBER = '';
        reqBody.VKONA = '';
        reqBody.MANSP = '';
        reqBody.KTOKL = '';
        reqBody.FORMKEY = '';
        reqBody.FITYP = '';
        reqBody.PROVINCE = '';
        reqBody.COUNTY = '';
        reqBody.LANDL = '';
        reqBody.ZZ_CANALE_STAMPA = '';
        reqBody.MWSKZ = '';
        reqBody.KSCHL = '';
        reqBody.EXDFR = '';
        reqBody.EXDTO = '';
        reqBody.EXNUM = '';
        reqBody.EXRAT = '';
        reqBody.VREFER = '';
        reqBody.STAGRUVER = '';
        reqBody.BEGRU = '';
        reqBody.VKBEZ = '';
        reqBody.IM_TAP = '';
        reqBody.IM_VIP = '';
        reqBody.IM_EBP = '';
        reqBody.IM_TIS = '';
        reqBody.IM_FACTOR_1 = '';
																			
        reqBody.IM_DI_CONTRFP = 0;
        reqBody.IM_DI_CONTRPT = 0;
        reqBody.IM_DI_CONTRAT = 0;
        reqBody.IM_DI_CONTFPL = 0;
        reqBody.IM_DI_CONTPTL = 0;
        reqBody.IM_DI_CONTUL = 0;
        reqBody.PM_VSTELLE = '';
        reqBody.ZZ_FATTURA_EMAIL = '';
        reqBody.BP_EXECUTIVE = ( String.isNotBlank (c.Account.CNT_Executive__c) && String.isNotBlank(c.Account.CNT_Executive__r.CNT_Code_Executive__c) &&  c.Account.CNT_Executive__r.CNT_Code_Executive__c.indexOf('-') > 0) ? c.Account.CNT_Executive__r.CNT_Code_Executive__c.split('-').get(1).trim() :  '';   
        reqBody.AcQsskzE = '';
        reqBody.FL_DESCRUR = '';
        reqBody.FL_SAZONAL = '';
        reqBody.PISO = '';
        reqBody.DEPARTAMENTO = '';
        reqBody.IM_DI_CONTRGE = 0;
        
        reqBody.AC_VKONT_NEW = '';
        reqBody.BP_PARTNER_NEW = '';
        reqBody.BUKRS = '2003';
        reqBody.CAUSRICH = 'ANCO';
        reqBody.COD_OPERAZ = 'FATT';
        reqBody.RECORD_TYPE = 'HEAD';
        reqBody.ZZINT_FATT = 'BP';
        reqBody.CO_COUNTRY = '';
        reqBody.CO_HAUS = '';
        
        
        list<CNT_IntegrationHelper.Items> items = new list<CNT_IntegrationHelper.Items>();
        CNT_IntegrationHelper.Items item = new CNT_IntegrationHelper.Items();
        item.RECORD_TYP = 'POSI';
        //item.ID_RICHIESTA = '';
        item.ID_RICHIESTA = '';
        item.ID_RICHIESTA_FO = c.CaseNumber;
        item.MATERIALE = '000000000000902430'; //Codigo Default referente a Obra
        item.POSNR = 0; //Posicao do item da lista, como so tem 1 item esse valor será sempre da posição 0
        item.QUANTITA = 1; // Valor Default Relação a apenas 1 Obra
        item.IMPORTO_IMP = String.valueOf(listBudgt.get(0).CNT_TotalPayable__c).replace(',', '.'); //Total Cliente Vai Pagar
        //item.IMPORTO_IMP = '300.25';
        item.CAUSALE = '10'; //Valor Default
        item.SUB_CAUSALE = '10'; //Valor Default
        item.IMPORTO_UNI = String.valueOf(listBudgt.get(0).CNT_TotalPayable__c); //Total Cliente Vai Pagar
        //item.IMPORTO_UNI = '300.25';
        item.PERCENT = 00; //Percentual de Imposto
        
        items.add(item);
        reqBody.Items = items;
        
        req.Header = reqHeader;
        req.Body = reqBody;
        
        return CNT_IntegrationHelper.CargoSD_4_BR(req, caseId);
    }
    
	/**
	 * @description: CARGO FICA, interface to create taxes in SAP FICA
	 * @param: Id caseId
	 * @return: CNT_IntegrationHelper.CARGO_FICA_BR_4
	 * @by: artur.miranda in 06/06/2019
	 **/ 
    public static CNT_IntegrationHelper.CARGO_FICA_BR_4 flow_CARGO_FICA_4_BR(Id caseId){
        System.debug(':: Running flow_CARGO_FICA_4_BR >>>');

		Case myCase = [Select Id, CaseNumber, SubCauseBR__c, AccountId, Account.Name, Account.RecordType.DeveloperName, Contact.Email, Account.IdentityType__c, Account.IdentityNumber__c, Account.MainPhone__c, Account.ExternalId__c, Account.CNT_State_Inscription__c, Account.CNT_State_Inscription_Exemption__c,
                  Account.CNT_Executive__r.CNT_Code_Executive__c, Account.AccountNumber, PointofDelivery__r.Name, PointofDelivery__r.PointofDeliveryNumber__c, PointofDelivery__c, Contact.FirstName, Contact.LastName, Address__r.StreetMD__r.Municipality__c, Address__r.Number__c, 
                  Address__r.Postal_Code__c, Address__r.StreetMD__r.Street__c, Address__r.StreetMD__r.Street_Type__c, Address__r.Corner__c, Account.CompanyID__c, CNT_Observation__c, CNT_Economical_Activity__c, 
                  CNT_Free_Client__c, Address__r.StreetMD__r.Neighbourhood__c, Address__r.Municipality__c
                  From Case Where Id =: caseId];
        String accRecordType = myCase.Account.RecordType.DeveloperName;
        
        Id quoteId = [Select Id From NE__Order__c Where CNT_Case__c =: caseId And RecordType.DeveloperName = 'Quote'].Id;

        Contract cont = [Select Id, ContractNumber, CNT_ExternalContract_ID__c, CNT_Free_Client__c, StartDate, CNT_Economical_Activity__c From Contract Where CNT_Case__c =: myCase.Id];
        
        Contract_Line_Item__c cli = [Select Billing_Profile__r.CNT_Due_Date__c, Billing_Profile__r.Type__c, Billing_Profile__r.Delivery_Type__c, Billing_Profile__r.Address__r.StreetMD__r.Name, Billing_Profile__r.Address__r.StreetMD__r.Street__c From Contract_Line_Item__c where Contract__c =: cont.Id and CNT_Product__c like 'Grupo%'];
        
        AttributeBR att = getAttributes(quoteId);
        
        //prepare data
        CNT_IntegrationHelper.CARGO_FICA_BR_4 req = new CNT_IntegrationHelper.CARGO_FICA_BR_4();
        
        CNT_IntegrationHelper.Header reqHeader = new CNT_IntegrationHelper.Header();
        reqHeader.CodSistema = 'COEFIC';
        reqHeader.SistemaOrigen = 'SFDC';
        reqHeader.FechaHora = date.today();
        reqHeader.Funcionalidad = 'Cargo FICA';
        
        CNT_IntegrationHelper.BodyCARGO_FICA_4 reqBody = new CNT_IntegrationHelper.BodyCARGO_FICA_4();
        
		reqBody.IM_ZZ_NUMUTE = '';//String.valueOf(Integer.valueOf(cont.CNT_ExternalContract_ID__c)); //contract number
        reqBody.AC_ZAHLKOND = 'CP01';//cli.Billing_Profile__r.CNT_Due_Date__c.length() == 1 ? 'CP0' + cli.Billing_Profile__r.CNT_Due_Date__c : 'CP' + cli.Billing_Profile__r.CNT_Due_Date__c; //data vencimento
        reqBody.BP_NAME_FIRST = '';//myCase.Contact.FirstName == null ? '' : myCase.Contact.FirstName;
        reqBody.BP_NAME_LAST = '';//myCase.Contact.LastName;
        reqBody.BP_OPBUK = String.valueOf(myCase.Account.CompanyID__c);

        String streetType = '';// Avenida, Rua, Travessa, etc.
        for (CNT_Street_Type_Setting__mdt threatMapping : [SELECT Code__c, Value__c FROM CNT_Street_Type_Setting__mdt]) {
            System.debug(threatMapping.Code__c + ':' + threatMapping.Value__c);
            if(myCase.Address__r.StreetMD__r.Street_Type__c == threatMapping.Code__c){
                streetType = threatMapping.Value__c + ' ';
                break;
            }
        }
        reqBody.CO_STREET = streetType + myCase.Address__r.StreetMD__r.Street__c;// End. do PoD como endereço de entrega
        //reqBody.CO_HOUSE_NUM1 = myCase.Address__r.Number__c;
		reqBody.CO_HOUSE_NUM1 = String.isNotBlank(myCase.Address__r.Number__c) ? myCase.Address__r.Number__c : '0' ;
        reqBody.CO_POST_CODE1 = myCase.Address__r.Postal_Code__c;
        //reqBody.CO_CITY1 = myCase.Address__r.Municipality__c != null ? myCase.Address__r.Municipality__c : '';
		reqBody.CO_CITY1 = myCase.Address__r.Municipality__c != null ? myCase.Address__r.Municipality__c : 'CE';
        reqBody.CO_REGION = myCase.Account.CompanyID__c == '2005' ? 'RJ' : 'CE';
		reqBody.BPKIND = '0001';
        //reqBody.BP_SMTP_ADDR = myCase.Contact.Email != null ? myCase.Contact.Email : '';
        reqBody.BP_REGION = myCase.Account.CompanyID__c == '2005' ? 'RJ' : 'CE';
        reqBody.BP_POST_CODE1 = myCase.Address__r.Postal_Code__c;
        //enviando valores para Bairro e numero, se estiverem nulos.  //juan
        reqBody.BP_HOUSE_NUM1 = String.isNotBlank(myCase.Address__r.Number__c) ? myCase.Address__r.Number__c : '0' ;
        //reqBody.BP_CITY2 = String.isNotBlank(myCase.Address__r.StreetMD__r.Neighbourhood__c) ? myCase.Address__r.StreetMD__r.Neighbourhood__c : 'SEM BAIRRO';
        //reqBody.BP_CITY1 = String.isNotBlank(myCase.Address__r.Municipality__c) ? myCase.Address__r.Municipality__c : '';
		reqBody.BP_CITY1 = String.isNotBlank(myCase.Address__r.Municipality__c) ? myCase.Address__r.Municipality__c : 'CE';
        
        reqBody.BP_STREET = 'NITEROI';//streetType + cli.Billing_Profile__r.Address__r.StreetMD__r.Street__c;
        //reqBody.BP_ADEXT_ADDR = cont.CNT_ExternalContract_ID__c + '_' + enderecoType;
        //reqBody.BP_IDNUMBER_IE = (myCase.Account.CNT_State_Inscription_Exemption__c)? 'ISENTO': myCase.Account.CNT_State_Inscription__c;// Isencao ou numero de Inscricao
		reqBody.BP_COUNTRY = 'BR';
        
        reqBody.AC_VKTYP = '01';
        reqBody.AC_KOFIZ_SD = att.Classe; // Classe
        reqBody.AC_GSBER = '0300';

		if( ( String.isNotBlank (cli.Billing_Profile__c) && String.isNotBlank(cli.Billing_Profile__r.Type__c) )){
			if(cli.Billing_Profile__r.Type__c.equalsIgnoreCase('Automatic debit')){
				reqBody.AC_EZAWE =  'D'; // Forma de Pagamento (B = Boleto; C = Código de Barras; D = Débito Automático)
			}else if(cli.Billing_Profile__r.Type__c.equalsIgnoreCase('BarCode')){
				 reqBody.AC_EZAWE =  'C';
			} else {
				reqBody.AC_EZAWE =  'B';  // B = Boleto; C = Código de Barras
			}
		}else{
			 reqBody.AC_EZAWE =  'B';  // B = Boleto; C = Código de Barras
		}	

        if(Integer.valueOf(myCase.Account.IdentityType__c) == 5){// Pessoa Fisica (B2C_BRASIL) [005;5] 
            reqBody.BP_TYPE = '1';
            reqBody.BP_TAXTYPE = 'CPF';
        } else { // Pessoa Juridica (B2B_BRASIL)
            reqBody.BP_TYPE = '2';
            reqBody.BP_TAXTYPE = 'PJ';
        }

		reqBody.ATTIVITA = '0001'; // 0001 = Criacao Conta Contrato; 0002 => Criacao Encargo
		reqBody.BP_PARTNER = '';//'10275631';//myCase.Account.AccountNumber;// Verificar o uso desse campo

		//reqBody.ATTIVITA = '0002'; // 0001 = Criacao Conta Contrato; 0002 => Criacao Encargo
		//reqBody.BP_PARTNER = '10275631';//'10275631';//myCase.Account.AccountNumber;// Verificar o uso desse campo

        reqBody.BP_ZZ_CODFISC = cont.ContractNumber;
		reqBody.BP_BPEXT = myCase.Account.IdentityNumber__c + reqBody.BP_TAXTYPE; //'00827955847CPF';
		reqBody.ACCNAME = myCase.Account.Name;// 'TEST';//
        reqBody.ICOM_TEL_NUMBER = myCase.Account.MainPhone__c;//'1199998888';//
		reqBody.BP_NAME_ORG1 = '';
        reqBody.FORMKEY = 'IS_U_BILL';
		reqBody.ZZFLG_ONLINE = '';
		reqBody.IM_TARIFTYP = 'DDD';
		reqBody.COUNTRY = 'BR';
        reqBody.AC_ABWRH = '32';
        reqBody.AC_ABWMA = '';
		reqBody.BUKRS = myCase.Account.CompanyID__c;// Codigo da Empresa
        reqBody.VKONT = myCase.PointofDelivery__r.PointofDeliveryNumber__c;
        reqBody.ENCARGO = 'E009';//'E009';//'SER0';	// 'Z010';// Operação do encargo
        reqBody.BETRW = '1.0';	// Valor do Encargo tabelado em FICA 
        
		reqBody.BP_OLD_ZPARTNER = '';
		reqBody.BP_XDFADR = '';
		reqBody.AC_EBVTY = '';
		reqBody.COPRC = '';
		reqBody.IM_ZZPASSO_FATT = '';
		reqBody.AC_ZZ_STATO_DOM = '';
		reqBody.LEGAL_ENTY = '';
		reqBody.CCINS = '';
		reqBody.CCNUM = '';
		reqBody.VKONA = '';
		reqBody.KTOKL = '';
		reqBody.FITYPE = '';
		reqBody.PROVINCE = 'CE';
		reqBody.ZZ_FATTURA_EMAIL = '';
		reqBody.BP_EXECUTIVE = '';
		reqBody.ACQSSKZE = '';
		
        req.Header = reqHeader;
        req.Body = reqBody;
        
		return req;
    }
    
    /* isu */
    
    //boleto rds
    public static CNT_IntegrationHelper.LiberacionObraRequest flow_LiberacionObra_SYN_BR(Id caseId){
        System.debug('::Running flow_LiberacionObra_SYN_BR');
        
        Case c = [Select Id, CaseNumber, Account.CompanyID__c, CNT_Contract__c
                    From Case
                   Where Id =: caseId];
                  
        CNT_Budget__c bud = [SELECT Id, CNT_NumberSalesOrder__c, CNT_DateGenLetter__c, CNT_DateGenContract__c
                               FROM CNT_Budget__c
                              WHERE CNT_Contract__r.CNT_Case__c =: caseId
                           Order By CreatedDate desc
                              LIMIT 1];
        
        String contNumber = [Select CNT_ExternalContract_ID__c From Contract Where Id =: c.CNT_Contract__c limit 1].CNT_ExternalContract_ID__c;
        
        //prepare data
        CNT_IntegrationHelper.LiberacionObraRequest req = new CNT_IntegrationHelper.LiberacionObraRequest();
        
        CNT_IntegrationHelper.Header reqHeader = new CNT_IntegrationHelper.Header();
        reqHeader.CodSistema = 'BRASYN';
        reqHeader.SistemaOrigen = 'SFDC';
        reqHeader.FechaHora = date.today();
        reqHeader.Funcionalidad = 'LiberacionObra';
        
        CNT_IntegrationHelper.Body_LiberacionObra_Request reqBody = new CNT_IntegrationHelper.Body_LiberacionObra_Request();
        reqBody.CodigoEmpresa = c.Account.CompanyID__c;
        reqBody.NumeroCaso = c.CaseNumber;
        reqBody.NumeroCliente = Integer.valueOf(contNumber);
        
        reqBody.DataEnvCarta = String.valueOf(bud.CNT_DateGenLetter__c) + 'T00:00:00Z';
        reqBody.DataRecCarta = String.valueOf(bud.CNT_DateGenLetter__c) + 'T00:00:00Z';
        reqBody.DataRegCarta = String.valueOf(bud.CNT_DateGenLetter__c) + 'T00:00:00Z';
        reqBody.DataImpContrac = String.valueOf(bud.CNT_DateGenContract__c) + 'T00:00:00Z';
        reqBody.DataAssinContrac = String.valueOf(bud.CNT_DateGenContract__c) + 'T00:00:00Z';
        reqBody.DataRegContrac = String.valueOf(bud.CNT_DateGenContract__c) + 'T00:00:00Z';
        reqBody.DataAutorizacao = String.valueOf(bud.CNT_DateGenContract__c) + 'T00:00:00Z';
        
        req.Header = reqHeader;
        req.Body = reqBody;
        
		// Jonas C. Jr. - BUGFIX-41 - 6/28/2019
        //return CNT_IntegrationHelper.LiberacionObra_4_BR(req, caseId);
		return req;
    }
    
    /* isu */
    
    //boleto rds
    public static HttpResponse flow_BoletoRDS_SAP_BR(Id caseId){
        System.debug('::Running flow_BoletoRDS_SAP_BR');
        
        Case c = [Select Id, Account.CompanyID__c
                    From Case
                   Where Id =: caseId];
                  
        CNT_Budget__c bud = [SELECT Id, CNT_NumberSalesOrder__c
                               FROM CNT_Budget__c
                              WHERE CNT_Contract__r.CNT_Case__c =: caseId
                           Order By CreatedDate desc
                              LIMIT 1];
        
        //prepare data
        CNT_IntegrationHelper.BoletoRDSRequest req = new CNT_IntegrationHelper.BoletoRDSRequest();
        
        CNT_IntegrationHelper.Header reqHeader = new CNT_IntegrationHelper.Header();
        reqHeader.CodSistema = 'COEFIC';
        reqHeader.SistemaOrigen = 'SFDC';
        reqHeader.FechaHora = date.today();
        reqHeader.Funcionalidad = 'CreateBoletoRDS';
        
        CNT_IntegrationHelper.Body_BoletoRDS_Request reqBody = new CNT_IntegrationHelper.Body_BoletoRDS_Request();
        reqBody.CodigoEmpresa = c.Account.CompanyID__c;
        reqBody.OrdenVenta = bud.CNT_NumberSalesOrder__c;
        
        req.Header = reqHeader;
        req.Body = reqBody;
        
        return CNT_IntegrationHelper.BoletoRDS_4_BR(req, caseId);
    }
    
    
    /* SYNERGIA */ 
    public static CNT_IntegrationHelper.ALTA_BR_239 flow_ALTA_Tecnica_BR(Id caseId){
        System.debug('::Run flow_ALTA_Tecnica_BR');
        
        Contract contrato = new Contract();
        
        Case c = [SELECT Id, CNT_White_Rate__c, CNT_Contract__c, CaseNumber, Account.Name, Contact.Email,
                         Account.IdentityType__c, Account.IdentityNumber__c, Account.MainPhone__c,
                         Account.CNT_Executive__r.CNT_Code_Executive__c, PointofDelivery__r.Name, 
                         Address__r.Postal_Code__c, CNT_Economical_Activity__c, Address__r.StreetMD__r.Street__c,
                         Address__r.StreetMD__r.Street_Type__c, Address__r.Corner__c, Description, SubCauseBR__c,
                         Address__r.StreetMD__r.Neighbourhood__c, Account.CompanyID__c, CNT_Observation__c,
                         Address__r.Number__c, Address__r.Lot__c, Account.RecordType.DeveloperName, Address__r.Municipality__c,
                         CNT_Conexion_Transitoria__c
                  From Case Where Id =: caseId];
        
        Id quoteId = [Select Id From NE__Order__c Where CNT_Case__c =: caseId Order By CreatedDate DESC][0].Id;
        
        //String contNumber = [Select CNT_ExternalContract_ID__c From Contract Where Id =: c.CNT_Contract__c limit 1].CNT_ExternalContract_ID__c;
        for(Contract loopCntr : [Select id, CNT_Numero_Documento_Beneficio__c, CNT_Document_Type__c, CNT_ExternalContract_ID__c, CNT_Economical_Activity__c, StartDate From Contract Where Id =: c.CNT_Contract__c limit 1]){ contrato = loopCntr;}
        
        
        System.debug('Passou das queries iniciais em ALTA TECNICA');
        
        AttributeBR att = getAttributes(quoteId);
        System.debug(':: getAttributes: '+ JSON.serialize(att));
        
        String codSistema;
        if (c.Account.CompanyID__c == '2005'){
            CodSistema = 'BRASYN';
        }else{
            CodSistema = 'COESYN';
        }
        
        //prepare data
        CNT_IntegrationHelper.ALTA_BR_239 req = new CNT_IntegrationHelper.ALTA_BR_239();
        
        CNT_IntegrationHelper.Header reqHeader = new CNT_IntegrationHelper.Header();
        reqHeader.CodSistema = codSistema;
        reqHeader.SistemaOrigen = 'SFDC';
        reqHeader.FechaHora = date.today();
        reqHeader.Funcionalidad = 'Alta';
        
        CNT_IntegrationHelper.BodyALTA_BR_239 reqBody = new CNT_IntegrationHelper.BodyALTA_BR_239();
        reqBody.codigo_empresa = c.Account.CompanyID__c;
        reqBody.numero_cliente = Integer.valueOf(contrato.CNT_ExternalContract_ID__c); //Integer.valueOf(c.PointofDelivery__r.Name);
        reqBody.nome = c.Account.Name;
        reqBody.Submotivo = c.SubCauseBR__c;
        reqBody.interface_contrato = 1;
        reqBody.sucursal = '999';
        reqBody.id_unico = '2';
        reqBody.user_number = 123;
        if(c.CNT_Conexion_Transitoria__c){
            reqBody.data_contrato = contrato.StartDate;
        }else{
            reqBody.data_contrato = date.today();
        }
        reqBody.execution_mode = '1';
        reqBody.UsuarioIngresso = userinfo.getUserName().substring(0,19);
        reqBody.numero_caso = c.CaseNumber;
        if(contrato.CNT_Document_Type__c == 'NIS'){
             reqBody.numero_nis = contrato.CNT_Numero_Documento_Beneficio__c;
             reqBody.numero_nb = '0';
        }else if(contrato.CNT_Document_Type__c == 'NB'){
            reqBody.numero_nis = '0';
            reqBody.numero_nb = contrato.CNT_Numero_Documento_Beneficio__c;
        }
        reqBody.Activity = '72';
        reqBody.tarifa = att.Tarifa;
        reqBody.potencia = Integer.valueOf(att.Potencia);
        reqBody.carga = Integer.valueOf(att.Carga);
        
        System.debug('::att >>> '+ att);
        if(String.isNotBlank(att.ModalidadeTarifaria) && att.ModalidadeTarifaria.contains('Azul')){
            reqBody.demanda = Integer.valueOf(att.DemandaPonta);
            reqBody.demanda_FP = att.DemandaForaPonta != null ? Integer.valueOf(att.DemandaForaPonta) : 0;
        }
        else if(String.isNotBlank(att.ModalidadeTarifaria) && att.ModalidadeTarifaria.contains('Verde')){
            reqBody.demanda_FP = Integer.valueOf(att.Demanda);
            reqBody.demanda = 0;
        } else {
            reqBody.demanda_FP = 0;
            reqBody.demanda = 0;
        }
        reqBody.demanda_Reservada = 0;
        
        reqBody.classe = att.Classe;
        reqBody.subclasse = att.SubClasse; //att.SubClasse;
        reqBody.grupo = att.Grupo;
         //reqBody.observacao = c.CNT_Observation__c != null ? c.CNT_Observation__c : '-';
        reqBody.Observacao = c.Description != null ? c.Description : '-';
        reqBody.ReferenciaEndereco = '';
        reqBody.Tipo_liga_nova = 'U';
        
        //Juan Vieitez -- Traduzindo o valor para synergia
        System.debug('TipoTensao: '+att.TipoTensao);        
        switch on att.TipoTensao {
            when '1' {  //Tipo de ligação Monofasica
                reqBody.tipo_ligacao = 'M';
            }
            when '2' {  //Tipo de ligação Bifasica
                reqBody.tipo_ligacao = 'B';
            }
            when '3' {  //Tipo de ligação Trifasica
                reqBody.tipo_ligacao = 'T';
            }
            when else {
                throw new CNT_CommonException('Tipo de Ligação não definido ou inválido!');
            }
        }
        //reqBody.tipo_ligacao = 'T';
        reqBody.valor_tensao = att.ValorTensaoSAP;
        System.debug('::valor_tensao: '+att.ValorTensaoSAP );
        reqBody.bairro = c.Address__r.StreetMD__r.Neighbourhood__c != null ? c.Address__r.StreetMD__r.Neighbourhood__c : '';
        reqBody.bairro_postal = c.Address__r.StreetMD__r.Neighbourhood__c != null ? c.Address__r.StreetMD__r.Neighbourhood__c : '';
        //reqBody.bairro = 'COP';
        //reqBody.bairro_postal = 'COP';
                                    
        reqBody.cadastro_br = 'N';
        reqBody.cobro_postal = 'N';
        reqBody.cod_logra = '5';
        reqBody.num_imovel = (c.Address__r.Number__c != null) ? Integer.valueOf(c.Address__r.Number__c) : 0;// House number
        //reqBody.nome_logra = c.Address__r.StreetMD__r.Name;// Street Name (Char 45)
        String streetType = '';
        for (CNT_Street_Type_Setting__mdt threatMapping : [SELECT Code__c, Value__c FROM CNT_Street_Type_Setting__mdt]) {
            System.debug(threatMapping.Code__c + ':' + threatMapping.Value__c);
            if(c.Address__r.StreetMD__r.Street_Type__c == threatMapping.Code__c){
                streetType = threatMapping.Value__c + ' ';
                break;
            }
        }
        reqBody.nome_logra =  streetType + c.Address__r.StreetMD__r.Street__c;// Street Name (Char 45)
        //reqBody.nome_logra = c.Address__r.StreetMD__r.Street_Type__c +' '+ c.Address__r.StreetMD__r.Street__c;// Street Name (Char 45)
        reqBody.complemento = c.Address__r.Corner__c != null ? c.Address__r.Corner__c : '';
        reqBody.ejecutivo = ( String.isNotBlank(c.Account.CNT_Executive__c) && String.isNotBlank(c.Account.CNT_Executive__r.CNT_Code_Executive__c) && c.Account.CNT_Executive__r.CNT_Code_Executive__c.indexOf('-') > 0) ? c.Account.CNT_Executive__r.CNT_Code_Executive__c.split('-').get(1).trim() :  '';   
        reqBody.cep_postal = c.Address__r.Postal_Code__c;
        reqBody.estado = c.Account.CompanyID__c == '2005' ? 'RJ' : 'CE';
        reqBody.data_ingresso = date.today();
        reqBody.data_nasc_br = '';
        //reqBody.direccion_postal = 'COP';
		reqBody.direccion_postal = c.Address__r.Lot__c;

        reqBody.dv_rut2_br = '12';
        reqBody.dv_rut_br = '12';
        reqBody.rut2_br = '12';
        reqBody.rut_br = '12';
        reqBody.estado_civil = 1;
        // Manter isso por um tempo ate que os dados do CNAE sejam migrados para contrato
        if(String.isNotBlank(contrato.CNT_Economical_Activity__c)){
            reqBody.giro = contrato.CNT_Economical_Activity__c.substring(0,8);
        } else {
            reqBody.giro = 'Z0101000';// Z0101000-Residencial Pleno
        }
        reqBody.grau_parent_br = '-';
        reqBody.nome_benef_br = '-';
        reqBody.tipo_br = 'AB';
        reqBody.tipo_ident2_br = '05';
        reqBody.uf_nasc_br = '';
        reqBody.ind_cliente_vital = 'N';
         String personType = 'F';
        if (c.Account.RecordType.DeveloperName == 'B2B_BRASIL' || c.Account.RecordType.DeveloperName == 'B2G_BRASIL'){
            personType = 'J';
        }
        reqBody.tipo_pessoa = personType;
        reqBody.insc_estadual = 'Comercial';
        //reqBody.latitudeEndVizinho = 1.23111;
        //reqBody.longitudeEndVizinho = 2.23111;
        //reqBody.mail = c.Contact.Email != null ? c.Contact.Email : '';
		reqBody.mail = c.Contact.Email != null ? c.Contact.Email : 'cttmigradonaopossuiemail@enel.com';
        //reqBody.municipio = c.Address__r.Municipality__c != null ? c.Address__r.Municipality__c : '';
		reqBody.municipio = c.Address__r.Municipality__c != null ? c.Address__r.Municipality__c : 'CE';
        //reqBody.municipio_postal = c.Address__r.Municipality__c != null ? c.Address__r.Municipality__c : '';
		reqBody.municipio_postal = c.Address__r.Municipality__c != null ? c.Address__r.Municipality__c : 'CE';
        reqBody.nivel_tensao = att.NivelTensao;
        reqBody.tipo_ident2 = '05';
        reqBody.tipo_ident_br = '05';
        reqBody.Tarifa_branca = c.CNT_White_Rate__c ? 'S' : 'N';
        
        CNT_IntegrationHelper.Document doc = new CNT_IntegrationHelper.Document();
        doc.tipo_documento = c.Account.IdentityType__c;
        doc.numero_doc = c.Account.IdentityNumber__c;
        doc.dv_documento = '12';
        list<CNT_IntegrationHelper.Document> docs = new list<CNT_IntegrationHelper.Document>();
        docs.add(doc);
        
        reqBody.documentos = docs;
        
        CNT_IntegrationHelper.Telephone tel = new CNT_IntegrationHelper.Telephone();
        list<CNT_IntegrationHelper.Telephone> tels = new list<CNT_IntegrationHelper.Telephone>();
        tels.add(getTelefone(c.Account.MainPhone__c));
        reqBody.telefones = tels;
        
        reqBody.telefones = tels;
        
        CNT_IntegrationHelper.Question q = new CNT_IntegrationHelper.Question();
        q.pergunta = 'Pergunta1';
        q.resposta = 'Resposta1';
        list<CNT_IntegrationHelper.Question> questions = new list<CNT_IntegrationHelper.Question>();
        questions.add(q);
        
        reqBody.questoes = questions;
        
        req.Header = reqHeader;
        req.Body = reqBody;
        SYSTEM.DEBUG('Chegou ao fim de ALTA TECNICA');
        //CNT_IntegrationHelper.ALTA_239_BR(req, caseId);
        
        return req;
    }
    
    public static CNT_IntegrationHelper.BAJA_BR_239 flow_BAJA_Tecnica_BR(Id caseId){
        System.debug('::Running flow_BAJA_Tecnica_BR');
        
        List<Contract> listOldCntr = new List<Contract>();
        List<Contract> listNewCntr = new List<Contract>();
        List<Asset> listNewAsset = new List<Asset>();
        List<Case> listOldCase = new List<Case>();
        Contract NewContract = new Contract();
        Case c = new Case();
        String oldContractNumber;

        
        listOldCase = [SELECT Id, CNT_Contract__c, CaseNumber, CNT_LastInvoiceOptions__c, CNT_TakeDevice__c,
                              Account.CompanyID__c, AssetId, Asset.Contract__c, Asset.PointofDelivery__c,
                              Account.Name, CNT_Observation__c, Description, Account.RecordType.DeveloperName,
                              SubCauseBR__c, Asset.Contract__r.CNT_ExternalContract_ID__c, CNT_Conexion_Transitoria__c
                         FROM Case
                        WHERE Id =: caseId];
        
        if(listOldCase.size() == 0){
            throw new CNT_CommonException('Caso não encontrado!');
        }else{
            c = listOldCase.get(0);
        }
        
        if(String.isNotBlank(c.AssetId)){
            if(String.isNotBlank(c.Asset.Contract__c)){
                if(String.isNotBlank(c.Asset.Contract__r.CNT_ExternalContract_ID__c)){
                    oldContractNumber = c.Asset.Contract__r.CNT_ExternalContract_ID__c;
                }else{
                    throw new CNT_CommonException('Não foi encontrado a chave externa do Contrato associado ao Caso!');
                }
            }else{
                throw new CNT_CommonException('Não foi encontrado o Contrato associado ao Caso!');
            }
        }else{
            throw new CNT_CommonException('Não foi encontrado o Ativo associado ao Caso!');
        }
        /*
        if(!c.CNT_Conexion_Transitoria__c){
            listNewAsset = [SELECT Contract__c
                             FROM Asset
                            WHERE PointofDelivery__c =: c.Asset.PointofDelivery__c
                              and Contract__r.Status = 'Activated'
                         ORDER BY CreatedDate desc
                            LIMIT 1];
            
            if(listNewAsset.size() == 0){
                throw new CNT_CommonException('Não foi encontrado o novo Ativo da conta despersonalizada!');
            }
            
            NewContract = [Select CNT_ExternalContract_ID__c, Account.Name From Contract Where Id =: listNewAsset.get(0).Contract__c limit 1];
            
            System.debug('::OldContractNumber: '+oldContractNumber+' NewContractNumber: '+NewContract.CNT_ExternalContract_ID__c);
        }
        */

        String codSistema;
        if (c.Account.CompanyID__c == '2005'){
            CodSistema = 'BRASYN';
        }else{
            CodSistema = 'COESYN';
        }
        
        
        //prepare data
        CNT_IntegrationHelper.BAJA_BR_239 req = new CNT_IntegrationHelper.BAJA_BR_239();
        
        CNT_IntegrationHelper.Header reqHeader = new CNT_IntegrationHelper.Header();
        reqHeader.CodSistema = CodSistema;
        reqHeader.SistemaOrigen = 'SFDC';
        reqHeader.FechaHora = date.today();
        reqHeader.Funcionalidad = 'Baixa';
        
        CNT_IntegrationHelper.BodyBAJA_BR_239 reqBody = new CNT_IntegrationHelper.BodyBAJA_BR_239();
        reqBody.codigo_empresa = c.Account.CompanyID__c;
        reqBody.numero_cliente = Integer.valueOf(oldContractNumber);
        reqBody.interface_contrato = 2;
        reqBody.Submotivo = c.SubCauseBR__c;
        reqBody.Motivo = '4';
        //reqBody.numero_cliente_novo = (c.CNT_Conexion_Transitoria__c ? 0 : Integer.valueOf(NewContract.CNT_ExternalContract_ID__c));
        //reqBody.nome_novo = (c.CNT_Conexion_Transitoria__c ? '' : NewContract.Account.Name);
        String personType = 'F';
        if (c.Account.RecordType.DeveloperName == 'B2B_BRASIL' || c.Account.RecordType.DeveloperName == 'B2G_BRASIL'){
            personType = 'J';
        }
         //DGUANA 16-Aug-18: validates if description (old cases) has more than 245 characters, if yes, truncate them.
        String descr = '-';
        if(c.Description!=null){
            descr = c.Description;
            if(descr.length()>245){
                descr = descr.subString(0,245);
            }                   
        }        
        //DGUANA 16-Aug-18: if CNT_takeDevice__c is checked, concat "ret ramal".
        if(c.CNT_takeDevice__c){
            descr = descr +' ret ramal';
        }
        
        reqBody.Observacao = descr; //DGUANA 16-Aug-18 c.Description != null ? c.Description : '-';
        
        reqBody.tipo_pessoa = personType;
        reqBody.sucursal = '0000';
        reqBody.id_unico = '2';
        reqBody.user_number = 123;
        reqBody.data_contrato = date.today();
        reqBody.execution_mode = '1';
        reqBody.UsuarioIngresso = userinfo.getUserName().substring(0,19);
        reqBody.numero_caso = c.CaseNumber;
        reqBody.Activity = '72';
        reqBody.ultima_leitura_visita = c.CNT_LastInvoiceOptions__c != null ? c.CNT_LastInvoiceOptions__c : '3';
        reqBody.retirada_medidor = c.CNT_TakeDevice__c ? '1' : '0';
        
        req.Header = reqHeader;
        req.Body = reqBody;
        
        //CNT_IntegrationHelper.BAJA_239_BR(req, caseId);
        return  req;
    }
    // cambio de titularidade - troca de titularidade
    public static CNT_IntegrationHelper.CT_BR_239 flow_CT_Tecnica_BR(Id caseId, String oldContractNumber){
        System.debug('::Running flow_CT_Tecnica_BR');

        System.debug('::caseId: '+caseId);
        System.debug('::oldContractNumber: '+oldContractNumber);
        
        
        
        Case c = [SELECT Id, CaseNumber, CNT_Contract__c, CNT_Observation__c, Description, Account.RecordType.DeveloperName, 
                         Account.CompanyID__c, Account.MainPhone__c, CNT_Change_Type__c, SubCauseBR__c,
                         Account.Name,Account.CNT_Mothers_Full_Name__c, Account.IdentityType__c, Account.IdentityNumber__c,
                         Address__r.Municipality__c, Address__r.StreetMD__r.Neighbourhood__c, Address__r.Number__c,
                         Address__r.StreetMD__r.Name, Address__r.Corner__c, Contact.Email
                    FROM Case
                   WHERE Id =: caseId];
        
        //Id orderId = [Select Id From NE__Order__c Where CNT_Case__c =: caseId And RecordType.DeveloperName = 'Order'].Id;
        Id orderId = [Select Id From NE__Order__c Where CNT_Case__c =: caseId LIMIT 1].Id;
                  
        String newContractNumber = [Select CNT_ExternalContract_ID__c From Contract Where Id =: c.CNT_Contract__c limit 1].CNT_ExternalContract_ID__c;
		
	    //Juan - adicionando campos para nis e nb -- CNT_Document_Type__c, CNT_Numero_Documento_Beneficio__c
		//enviar o campo 
		//String newContractNumber = [Select CNT_ExternalContract_ID__c, CNT_Document_Type__c, CNT_Numero_Documento_Beneficio__c From Contract Where Id =: c.CNT_Contract__c limit 1].CNT_ExternalContract_ID__c;
       
        // Verify olds client (CNT_ExternalContract_ID_2__c)
        List<Contract> listContract = [Select Account.Name, CNT_Document_Type__c, CNT_Numero_Documento_Beneficio__c From Contract Where CNT_ExternalContract_ID_2__c =: oldContractNumber limit 1];
        Contract oldContract = new Contract();
        if(listContract.isEmpty()){
             listContract =  [Select Account.Name, CNT_Document_Type__c, CNT_Numero_Documento_Beneficio__c From Contract Where ContractNumber =:oldContractNumber limit 1];  
        }
        if(!listContract.isEmpty()){
            oldContract = listContract.get(0);
        }

        System.debug('Passou das queries iniciais em CT TECNICA');
        
        String codSistema;
        if (c.Account.CompanyID__c == '2005'){
            CodSistema = 'BRASYN';
        }else{
            CodSistema = 'COESYN';
        }
        
        //prepare data
        CNT_IntegrationHelper.CT_BR_239 req = new CNT_IntegrationHelper.CT_BR_239();
        
        CNT_IntegrationHelper.Header reqHeader = new CNT_IntegrationHelper.Header();
        reqHeader.CodSistema = CodSistema;
        reqHeader.SistemaOrigen = 'SFDC';
        reqHeader.FechaHora = date.today();
        reqHeader.Funcionalidad = 'Troca Titularidade';
        
        CNT_IntegrationHelper.BodyCT_BR_239 reqBody = new CNT_IntegrationHelper.BodyCT_BR_239();
        reqBody.codigo_empresa = c.Account.CompanyID__c;
        reqBody.nome = oldContract.Account.Name;
        reqBody.nome_novo = c.Account.Name;
        reqBody.nome_mae_novo = c.Account.CNT_Mothers_Full_Name__c;
        reqBody.numero_cliente = Integer.valueOf(oldContractNumber);
        reqBody.numero_cliente_novo = Integer.valueOf(newContractNumber);
        reqBody.interface_contrato = 4;
        reqBody.Submotivo = c.SubCauseBR__c;
        String personType = 'F';
        if (c.Account.RecordType.DeveloperName == 'B2B_BRASIL' || c.Account.RecordType.DeveloperName == 'B2G_BRASIL'){
            personType = 'J';
        }
        reqBody.tipo_pessoa = personType;
        reqBody.sucursal = '0000';
        reqBody.id_unico = '2';
        reqBody.user_number = 123;
        reqBody.data_contrato = date.today();
        reqBody.execution_mode = '1';
        reqBody.UsuarioIngresso = userinfo.getUserName().substring(0,19);
        reqBody.numero_caso = c.CaseNumber;
        reqBody.Activity = '72';
        reqBody.Observacao = c.Description != null ? c.Description : '-'; //DGUANA 16-Aug-18: added to change
        
        // Telephone
        CNT_IntegrationHelper.Telephone tel = new CNT_IntegrationHelper.Telephone();
        list<CNT_IntegrationHelper.Telephone> tels = new list<CNT_IntegrationHelper.Telephone>();
        tels.add(getTelefone(c.Account.MainPhone__c));
        reqBody.telefones = tels;
		
        // Documents
        CNT_IntegrationHelper.Document doc = new CNT_IntegrationHelper.Document();
        doc.tipo_documento = c.Account.IdentityType__c;
        doc.numero_doc = c.Account.IdentityNumber__c;
        doc.dv_documento = '12';
        list<CNT_IntegrationHelper.Document> docs = new list<CNT_IntegrationHelper.Document>();
        docs.add(doc);
        reqBody.documentos = docs;

        //reqBody.municipio = c.Address__r.Municipality__c != null ? c.Address__r.Municipality__c : '';
		reqBody.municipio = c.Address__r.Municipality__c != null ? c.Address__r.Municipality__c : 'CE';
        reqBody.bairro = c.Address__r.StreetMD__r.Neighbourhood__c != null ? c.Address__r.StreetMD__r.Neighbourhood__c : '';
        reqBody.cod_logra = '5';
        reqBody.num_imovel = (c.Address__r.Number__c != null) ? Integer.valueOf(c.Address__r.Number__c) : 0;// House number
        reqBody.nome_logra = c.Address__r.StreetMD__r.Name;// Street Name (Char 45)
        reqBody.complemento = c.Address__r.Corner__c != null ? c.Address__r.Corner__c : '';

        //Juan - baixa renda
        /*
		if(c.CNT_Document_Type__c == 'NIS'){
             reqBody.numero_nis = c.CNT_Numero_Documento_Beneficio__c;
             reqBody.numero_nb = '0';
        }else if(c.CNT_Document_Type__c == 'NB'){
            reqBody.numero_nis = '0';
            reqBody.numero_nb = c.CNT_Numero_Documento_Beneficio__c;
        }
		*/
        
        AttributeBR att = getAttributes(orderId);
        reqBody.classe = att.Classe;
        reqBody.subclasse = att.SubClasse; //att.SubClasse;
        reqBody.tarifa = att.Tarifa;
        reqBody.data_ingresso = date.today();

        if(String.isNotBlank(att.ModalidadeTarifaria) && att.ModalidadeTarifaria.contains('Azul')){
            reqBody.demanda = Integer.valueOf(att.DemandaPonta);
            reqBody.demanda_FP = att.DemandaForaPonta != null ? Integer.valueOf(att.DemandaForaPonta) : 0;
        }
        else if(String.isNotBlank(att.ModalidadeTarifaria) && att.ModalidadeTarifaria.contains('Verde')){
            reqBody.demanda_FP = Integer.valueOf(att.Demanda);
            reqBody.demanda = 0;
        } else {
            reqBody.demanda_FP = 0;
            reqBody.demanda = 0;
        }
        reqBody.demanda_Reservada = 0;

        //reqBody.mail_novo = c.Contact.Email != null ? c.Contact.Email : '';
		reqBody.mail_novo = c.Contact.Email != null ? c.Contact.Email : 'cttmigradonaopossuiemail@enel.com';

        req.Header = reqHeader;
        req.Body = reqBody;
        
        //CNT_IntegrationHelper.CT_239_BR(req, caseId);
        
        return req;
    }
    
    //Juan 10
    //Cambio de Tarifa Synergia
    //Alteração de contrato 
    public static CNT_IntegrationHelper.CCC_BR_239 flow_CCC_Tecnica_BR(Id caseId){
        System.debug('::Running flow_CCC_Tecnica_BR');

        Contract contrato = new Contract();

        Case c = [Select Id, CaseNumber, CNT_Contract__c, CNT_Observation__c, Description, Account.CompanyID__c, Account.MainPhone__c, CNT_Change_Type__c, SubCauseBR__c From Case Where Id =: caseId];
        
        Id orderId = [Select Id From NE__Order__c Where CNT_Case__c =: caseId And RecordType.DeveloperName = 'Order' And CNT_Change_Type__c != null].Id;
                
        for(Contract loopCntr : [Select id, CNT_Numero_Documento_Beneficio__c, CNT_Document_Type__c, CNT_ExternalContract_ID__c, CNT_Economical_Activity__c From Contract Where Id =: c.CNT_Contract__c limit 1]){ contrato = loopCntr;}
       

        AttributeBR att = getAttributes(orderId);
        
        String codSistema;
        if (c.Account.CompanyID__c == '2005'){
            CodSistema = 'BRASYN';
        }else{
            CodSistema = 'COESYN';
        }
        String codMot = c.CNT_Change_Type__c.left(1);
        String motivo = ('1,2,3,4,6'.containsIgnoreCase(codMot) ? codMot : '');
        
        System.debug('Checkpoint 4');
        String changeType = c.CNT_Change_Type__c.substring(0,c.CNT_Change_Type__c.indexOf('-')).trim();
  
        //prepare data
        CNT_IntegrationHelper.CCC_BR_239 req = new CNT_IntegrationHelper.CCC_BR_239();
        
        CNT_IntegrationHelper.Header reqHeader = new CNT_IntegrationHelper.Header();
        reqHeader.CodSistema = codSistema;
        reqHeader.SistemaOrigen = 'SFDC';
        reqHeader.FechaHora = date.today();
        reqHeader.Funcionalidad = 'Cambio Tarifa';
        
        CNT_IntegrationHelper.BodyCCC_BR_239 reqBody = new CNT_IntegrationHelper.BodyCCC_BR_239();
        reqBody.id_unico = '';
        reqBody.Activity = '';
        reqBody.user_number = 0;
        reqBody.data_contrato = date.today();
        reqBody.execution_mode = '';
        reqBody.numero_caso = c.CaseNumber;
        reqBody.Motivo = motivo;
        reqBody.Submotivo = c.SubCauseBR__c;
        reqBody.canal_atendimento = 'DEFAULT';
        reqBody.interface_contrato = 3;
        reqBody.codigo_empresa = c.Account.CompanyID__c;
        reqBody.tarifa_nova = att.Tarifa;
        reqBody.classe_nova = att.Classe;
        reqBody.subclasse_nova = att.SubClasse;
        reqBody.sucursal = '0000';
        if(contrato.CNT_Document_Type__c == 'NIS'){
             reqBody.numero_nis = contrato.CNT_Numero_Documento_Beneficio__c;
             reqBody.numero_nb = '0';
        }else if(contrato.CNT_Document_Type__c == 'NB'){
            reqBody.numero_nis = '0';
            reqBody.numero_nb = contrato.CNT_Numero_Documento_Beneficio__c;
        }
        reqBody.UsuarioIngresso = userinfo.getUserName().substring(0,19);
        reqBody.numero_cliente = Integer.valueOf(contrato.CNT_ExternalContract_ID__c);
        //reqBody.Observacao = c.CNT_Observation__c != null ? c.CNT_Observation__c : '-';
        reqBody.Observacao = c.Description != null ? c.Description : '-';

        reqBody.carga = Integer.valueOf(att.Carga);
        reqBody.potencia = Integer.valueOf(att.Potencia);
        
        if(String.isNotBlank(att.ModalidadeTarifaria) && att.ModalidadeTarifaria.contains('Azul')){
            reqBody.demanda = Integer.valueOf(att.DemandaPonta);
            reqBody.demanda_FP = att.DemandaForaPonta != null ? Integer.valueOf(att.DemandaForaPonta) : 0;
        }
        else if(String.isNotBlank(att.ModalidadeTarifaria) && att.ModalidadeTarifaria.contains('Verde')){
            reqBody.demanda_FP = Integer.valueOf(att.Demanda);
            reqBody.demanda = 0;
        } else {
            reqBody.demanda_FP = 0;
            reqBody.demanda = 0;
        }
        reqBody.demanda_Reservada = 0;
        
        //reqBody.Tarifa_branca = c.CNT_White_Rate__c ? 'S' : 'N';
        System.debug('CARGA >>>>>>>>>>' + att.Carga);
        
        CNT_IntegrationHelper.Telephone tel = new CNT_IntegrationHelper.Telephone();
        list<CNT_IntegrationHelper.Telephone> tels = new list<CNT_IntegrationHelper.Telephone>();
        tels.add(getTelefone(c.Account.MainPhone__c));
        reqBody.telefones = tels;
        
        CNT_IntegrationHelper.Question q = new CNT_IntegrationHelper.Question();
        q.pergunta = 'Quer mudar tarifa?';
        q.resposta = 'Sim';
        list<CNT_IntegrationHelper.Question> questions = new list<CNT_IntegrationHelper.Question>();
        questions.add(q);
        
        reqBody.questoes = questions;
        
        
        req.Header = reqHeader;
        req.Body = reqBody;
        
        System.debug('REQUEST BODY >>>>>>>> ' + reqBody);
        
        //CNT_IntegrationHelper.CCC_239_BR(req, caseId);
        
        return req;
    }

    public static String getDateLastVisit(Id caseId){
		String newDate='';
		try{
			List<WorkOrder> listWorkOrder = new List<WorkOrder>();
			listWorkOrder = [SELECT Id, Account.Name, Case.CaseNumber,Case.RecordType.DeveloperName,Order_Number_BE__c, CNT_OSType__c, CNT_DescriptionStatus__c, CNT_EstadoOrdem__c, CNT_FlowStatus__c, CNT_DateExecutionVisit__c FROM WorkOrder WHERE CaseId =: caseId AND CNT_FlowStatus__c = 'CNT003'];
			System.debug(':: listWorkOrder '+listWorkOrder);
			if (!listWorkOrder.isEmpty()){
				for (WorkOrder tempWO : listWorkOrder){
					if (tempWO.Case.RecordType.DeveloperName=='CNT_BR_Alta_Contrato'||tempWO.Case.RecordType.DeveloperName=='CNT_ChangeProduct'||tempWO.Case.RecordType.DeveloperName=='CNT_OwnershipChange'){
						newDate = String.valueOf(tempWO.CNT_DateExecutionVisit__c);
					}
				}
			}
		}
		catch ( Exception e){

		}
		return newDate;
	}

    public static AttributeBR getAttributes(Id quoteId){
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
        
        AttributeBR result = new AttributeBR();

		// Inicializando atributos
		result.Potencia = '1';
		result.ChangeLoad = '1';
		result.Demanda = 0;
		result.DemandaPonta = 0;
		result.DemandaForaPonta = 0;

        System.debug('::quoteId: '+quoteId);
        NE__OrderItem__c item = [Select NE__ProdName__c From NE__OrderItem__c Where NE__OrderId__c =: quoteId limit 1];
        
        if (item.NE__ProdName__c.contains('A')){
            result.Grupo = 'A';
        }else{
            result.Grupo = 'B';
        }
            String regexValorTensao = '[a-zA-Z]{1,}';
        for (NE__Order_Item_Attribute__c att : [Select Id, Name, NE__Value__c, NE__Action__c From NE__Order_Item_Attribute__c Where NE__Order_Item__r.NE__OrderId__c =: quoteId and  NE__Order_Item__r.NE__ProdName__c like 'Grupo%']){
            
            system.debug(att.Name + '==>' + att.NE__Value__c);
            
            if (att.Name.contains('Carga')){
                //DGUANA 26-07-18: reparado por que explotaba cuando el valor era nulo (¿Está bien 0?)
                result.ChangeLoad = att.NE__Action__c;
                result.Carga = att.NE__Value__c!=null ? Integer.valueOf(att.NE__Value__c) : 0;
            }else if (att.Name.contains('Modalidade')){
                //DGUANA 26-09-18: reparado por que explotaba cuando el valor era nulo
                result.ModalidadeTarifaria = att.NE__Value__c!=null ? att.NE__Value__c : '';
            }else if (att.Name == 'Classe BR'){
                //DGUANA 26-07-18: reparado por que explotaba cuando el valor era nulo
                result.Classe = att.NE__Value__c!=null ? att.NE__Value__c.substring(0,2) : '';
            }else if (att.Name == 'SubClasse BR'){
                //DGUANA 26-07-18: reparado por que explotaba cuando el valor era nulo
                result.SubClasse = att.NE__Value__c!=null ? att.NE__Value__c.substring(0,att.NE__Value__c.indexOf('-')).trim() : '';
            }else if (att.Name == 'Demanda KV BR'){
                result.Demanda = String.isNotBlank(att.NE__Value__c) ? Integer.valueOf(att.NE__Value__c) : 1;
            }else if (att.Name.contains('Valor')){
                //DGUANA 26-07-18: reparado por que explotaba cuando el valor era nulo
                //result.ValorTensao = att.NE__Value__c!=null ? att.NE__Value__c.replace(' KV','') : '';

                // artur.miranda 2018-10-29
                if(att.NE__Value__c != null){
                    result.ValorTensao = att.NE__Value__c.replaceAll(regexValorTensao, '').trim();
                } else {
                    result.ValorTensao = '';
                }
                
            }else if (att.Name.contains('Capacidade')){
                result.CapacidadeDisjuntor = att.NE__Value__c;
            }else if (att.Name.contains('Instala')){
                result.InstalacaoPadrao = att.NE__Value__c;
            }else if (att.Name.contains('Categor')){
                result.CategoriaTarifa = att.NE__Value__c;
                result.ActionTarifa = att.NE__Action__c;
            }else if (att.Name.contains('Potencia')){
                result.Potencia = att.NE__Value__c != null ? att.NE__Value__c : '1';
            }else if (att.Name.contains('Tipo')){
                result.TipoTensao = att.NE__Value__c;
            }else if (att.Name.contains('Demanda Ponta BR')){
                result.DemandaPonta = att.NE__Value__c != null ? Integer.valueOf(att.NE__Value__c) : 0;
            }else if (att.Name.contains('Demanda Fora de Ponta BR')){
                result.DemandaForaPonta = att.NE__Value__c != null ? Integer.valueOf(att.NE__Value__c) : 0;
            }else if (att.Name.contains('Nivel')){
                //DGUANA 26-07-18: reparado por que explotaba cuando el valor era nulo
                result.NivelTensao = att.NE__Value__c!=null ? (att.NE__Value__c.contains('Baixa') ? '1' : '0') : '';
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
        return result;
        
    }
    
    public class AttributeBR{
        public String Tarifa;
        public String ActionTarifa;
        public String ChangeLoad;
        public String Grupo;
        public Integer Carga;
        public String ModalidadeTarifaria;
        public String Classe;
        public String SubClasse;
        public Integer Demanda;
        public Integer DemandaPonta;
        public Integer DemandaForaPonta;
        public String ValorTensao;
        public String CapacidadeDisjuntor;
        public String InstalacaoPadrao;
        public String CategoriaTarifa;
        public String Potencia;
        public String TipoTensao;
        public String NivelTensao;
        public String ValorTensaoSAP;
    }
    
    @TestVisible
    private static void setAttributes(){
        
        AttributeBR att = new AttributeBR();
        
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
        
        att.Tarifa = 'T';
        att.Grupo = 'A';
        att.Carga = 90;
        att.ModalidadeTarifaria = '64';
        att.Classe = 'COMER - 70';
        att.SubClasse = 'COM';
        att.Demanda = 0;
        att.DemandaPonta = 0;
        att.DemandaForaPonta = 0;
        att.ValorTensao = 'B';
        att.CapacidadeDisjuntor = '-';
        att.InstalacaoPadrao = 'Nao';
        att.CategoriaTarifa = '3';
        att.Potencia  = '0';
        att.TipoTensao = '3';
        att.NivelTensao = 'B';
        att.ValorTensaoSAP = '4';
         
    }

    /**
    * @description: Format a telephone according CNT_IntegrationHelper.Telephone
    * @param String mainPhone
    * @return CNT_IntegrationHelper.Telephone
    **/
    public static CNT_IntegrationHelper.Telephone getTelefone(String mainPhone){
        if(mainPhone.length() < 10){
            throw new CNT_CommonException('Número de telefone da conta inválido!');
        }
        CNT_IntegrationHelper.Telephone tel = new CNT_IntegrationHelper.Telephone();
        tel.prefixo_ddd = (String.isNotBlank(mainPhone))? mainPhone.substring(0,2) : '';
        tel.numero_telefone = (String.isNotBlank(mainPhone))? mainPhone.substring(2,10): '';
        tel.ind_contato = 'S';
        tel.tipo_telefone = '01';
        
        return tel;
    }

    public class CNT_CommonException extends Exception {}
}