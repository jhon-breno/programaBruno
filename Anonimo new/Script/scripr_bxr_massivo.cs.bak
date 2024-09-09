List<String> caseNumberList = new List<String>{{0}};

List<Case> ListaCasos = new List<Case>();

List<Case> ListaCasosCompletos = new List<Case>();

List<CNT_Contracting_Status_BO__c> backOfficeList = new List <CNT_Contracting_Status_BO__c> ();

List<Contract> contratos = new List<Contract>();

Map<String,Object> resultBackOffice = new Map<String,Object>();

public List<Id> lstCaseCompleteOrder  = new List<Id>(); 

List<Case> listCaso = new List<Case>();

listCaso = [SELECT Id, RecordTypeId, RecordType.DeveloperName, CaseNumber, CNT_Contract__c, CNT_Contract__r.CNT_EndDate__c,CaseRemarks__c,
		CNT_Contract__r.StartDate, CNT_Transit_Potencia_Total__c, AssetId, CNT_Quote__c, CNT_Conexion_Transitoria__c, 
		CNT_Payment_Method__c, CNT_Change_Type__c, PointofDelivery__r.PointofDeliveryNumber__c, CNT_Transit_Date_To__c, 
		SubCauseBR__c, CNT_ProcessStatus__c, CNT_LastInvoiceOptions__c, PointofDelivery__c, CNT_Documento_Pagamento__c, Status, 
		CNT_Controller242Success__c, CNT_Transit_Date_From__c, CNT_Tipo_de_Ajuste__c
		FROM Case
		WHERE CaseNumber in :caseNumberList AND RecordType.DeveloperName = 'CNT_ChangeProduct' and status='CNT0002'];

for(Case caso : listCaso)
{
	try
	{
		CNT_IntegrationHelper.CCC_BR_239 reqCCC239 = flow_CCC_Tecnica_BR(caso.Id);
		System.debug('JSON reqCCC239: ' + System.JSON.serialize(reqCCC239)); 
		
		if (reqCCC239.Body.subclasse_nova.Equals('REBRBPC'))
		{
			reqCCC239.Body.numero_nb =  reqCCC239.Body.numero_nis;
			reqCCC239.Body.numero_nis  = '0';			
		}
		
		HttpResponse response = CNT_IntegrationHelper.CCC_239_BRSync(reqCCC239, caso.Id);
		
		

		System.debug('response: ' + response);

		resultBackOffice = CNT_Utility.resultRequest(response, JSON.serialize(reqCCC239), caso, 'Sistema Tecnico');

		CNT_Contracting_Status_BO__c backOffice = new CNT_Contracting_Status_BO__c();

		backOffice = (CNT_Contracting_Status_BO__c) resultBackOffice.get('backoffice');

		backOffice.CNT_Technical_Information__c = JSON.serialize(reqCCC239);
				
		backOfficeList.add(backOffice);

		Boolean responseStatusSynergia = ((Boolean) resultBackOffice.get('status'));

		if (responseStatusSynergia)
		{	
			CNT_IntegrationHelper.CCC_BR_242 reqBaja = new CNT_IntegrationHelper.CCC_BR_242();
			
			reqBaja = CNT_IntegrationDataBR.flow_CCC_SAP_BR(caso.Id);//(caso.Id);
				//reqBaja.Body.VENDE = String.valueOf(Date.Today());
				//reqBaja.Body.VENDE = '2021-12-02';
				System.debug('JSON SAP: '+JSON.serialize(reqBaja));
				HttpResponse responseSAP = CNT_IntegrationHelper.CCC_242_BRSync(reqBaja,caso.Id);
				System.debug('responseSAP: '+responseSAP);
				resultBackOffice = CNT_Utility.resultRequest(responseSAP, JSON.serialize(reqBaja), caso, 'Facturador');
				backOffice = (CNT_Contracting_Status_BO__c) resultBackOffice.get('backoffice');
				backOfficeList.add(backOffice);
				Boolean responseStatusSAP = ((Boolean) resultBackOffice.get('status'));
				
				if(responseStatusSAP)
				{
					caso.Status = 'CNT0008';
					caso.cnt_processStatus__c = '';
					caso.CNT_ByPass__c = true;
					ListaCasos.add(caso);
					lstCaseCompleteOrder.add(caso.id);		
				}else
				{
					caso.status ='CNT0009';
					ListaCasos.add(caso);
				}
			
		}
		else
		{
			caso.status ='CNT0009';
			ListaCasos.add(caso);
		}
	}
	catch(Exception ex)
	{
		caso.status ='CNT0009';
		caso.CaseRemarks__c = 'ERRO: ' + ex.getStackTraceString(); 
		ListaCasos.add(caso);
		
		continue;
	}
}

		if(ListaCasos.size()>0)
		{
			update ListaCasos;
		}
		
		
		if(backOfficeList.size()>0)
		{
			insert backOfficeList;
		}
		
		if (lstCaseCompleteOrder.size() >0)
		{
			try
			{
				System.debug('Lista do complete : '+  lstCaseCompleteOrder);
				CNT_CompleteOrderBatchFlow sch = new CNT_CompleteOrderBatchFlow(lstCaseCompleteOrder);
				ID highPriorityJobId = Database.executebatch(sch, 4);			
				FlexQueue.moveJobToFront(highPriorityJobId);
			}
			catch(Exception ex)
			{
				System.debug('Erro : '+  ex);
				//CNT_ConfigurationUtility.completeOrderList(listIdOrder);
			}
		}
		

public static CNT_IntegrationHelper.CCC_BR_239 flow_CCC_Tecnica_BR(Id caseId)
{
        System.debug('::Running flow_CCC_Tecnica_BR');

        Contract contrato = new Contract();

        Case c = [Select Id, CaseNumber, CNT_Contract__c, CNT_Observation__c, Description, Account.CompanyID__c, Account.MainPhone__c, CNT_Change_Type__c, SubCauseBR__c From Case Where Id =: caseId];
        
        Id orderId = [Select Id From NE__Order__c Where CNT_Case__c =: caseId And RecordType.DeveloperName = 'Order' And CNT_Change_Type__c != null].Id;
                
        for(Contract loopCntr : [Select id, CNT_Numero_Documento_Beneficio__c, CNT_Document_Type__c, CNT_ExternalContract_ID__c, CNT_Economical_Activity__c From Contract Where Id =: c.CNT_Contract__c limit 1]){ contrato = loopCntr;}
       

        CNT_IntegrationDataBR.AttributeBR att = getAttributes(orderId);
        
        String codMot = c.CNT_Change_Type__c.left(1);
        String motivo = ('1,2,3,4,6,9'.containsIgnoreCase(codMot) ? codMot : '');
        
        System.debug('Checkpoint 4');
        String changeType = c.CNT_Change_Type__c.substring(0,c.CNT_Change_Type__c.indexOf('-')).trim();
  
        //prepare data
        CNT_IntegrationHelper.CCC_BR_239 req = new CNT_IntegrationHelper.CCC_BR_239();
        
        CNT_IntegrationHelper.Header reqHeader = new CNT_IntegrationHelper.Header();
        reqHeader.SistemaOrigen = 'SFDC';
        reqHeader.FechaHora = date.today();
        reqHeader.Funcionalidad = 'Cambio Tarifa';
        reqHeader.CodSistema = [SELECT Label FROM Brasil_Integration_Parameter__mdt
                                WHERE CompanyID__c = :c.Account.CompanyID__c AND 
                                Sistema_Destino__c = :ECH_VFC036_GeneralConstants.WS_SYSTEM_CODE_SYNERGIA].Label;
        
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

  public static CNT_IntegrationDataBR.AttributeBR getAttributes(Id quoteId)
  {
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

        NE__OrderItem__c item = [SELECT NE__ProdName__c FROM NE__OrderItem__c WHERE NE__OrderId__c =: quoteId LIMIT 1];
        
        if (item.NE__ProdName__c.contains('A')){
            result.Grupo = 'A';
        }else{
            result.Grupo = 'B';
        }
        String regexValorTensao = '[a-zA-Z]{1,}';
        for (NE__Order_Item_Attribute__c att : [SELECT Id, Name, NE__Value__c, NE__Action__c FROM NE__Order_Item_Attribute__c WHERE NE__Order_Item__r.NE__OrderId__c =: quoteId AND  NE__Order_Item__r.NE__ProdName__c LIKE 'Grupo%' AND NE__Order_Item__r.NE__Action__c IN ('Add', 'Change')]){
            
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
        System.debug('Checkpoint 2' + result.TipoTensao);
        return result;
        
    }
	
	public static CNT_IntegrationHelper.Telephone getTelefone(String mainPhone)
	{
        CNT_IntegrationHelper.Telephone tel = new CNT_IntegrationHelper.Telephone();
		
		system.debug('O valor do tel é: ' + mainPhone);
		
		if(mainPhone == null || mainPhone.length() < 10)
		{
        	tel.prefixo_ddd = '99';
			Integer phoneLength = 11;
			tel.numero_telefone = '999999999';
			tel.ind_contato = 'S';
			tel.tipo_telefone = '01';
        }
        else
		{
			tel.prefixo_ddd = (String.isNotBlank(mainPhone))? mainPhone.substring(0,2) : '';
			Integer phoneLength = mainPhone.length() > 11 ? 11 : mainPhone.length();
			tel.numero_telefone = (String.isNotBlank(mainPhone))? mainPhone.substring(2, phoneLength): '';
			tel.ind_contato = 'S';
			tel.tipo_telefone = '01';
        }
		
        return tel;
    }