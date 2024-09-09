	// Enviar MOVE IN massivamente ate 50 registros
	//List<String> caseNumberList = new List<String>{'157999702'};	
	List<String> caseNumberList = new List<String>{{0}};
	Boolean comCompleteOrder = false;
	String moveInDate = '';

	//moveInDate = 'DIA_ATUAL';// Move In baseado no dia anterior
	moveInDate = 'DIA_POSTERIOR';// Move In baseado no dia posterior /*PADRAO*/
	//moveInDate = 'CUSTOM_DATE';// Move In baseado no retorno da OS 
	//moveInDate = 'OS_DATE';// Move In baseado no retorno da OS
	//moveInDate = 'SET_DATE';// Move In baseado na data informada

	Date curretDate = Date.today();
	Integer day = curretDate.day();
	Integer month = curretDate.month();
	Integer year = curretDate.year();

	// MOVE IN with custom Date 
	Map<String,String> mapDateMoveIn =  new Map<String,String>();
	if( moveInDate == 'CUSTOM_DATE'){
		List<String> moveinDateList = new List<String>{''};
		Integer iten = 0;
		for(String myCase : caseNumberList){
			mapDateMoveIn.put(myCase, moveinDateList.get(iten) );
			iten++;
		}
	}

	// --------------- End MOVE IN with custom Date --------------------------

	CNT_IntegrationHelper.ALTA_BR_242 reqAlta = new CNT_IntegrationHelper.ALTA_BR_242();
	CNT_Contracting_Status_BO__c backOffice = new CNT_Contracting_Status_BO__c();
	Map<String,Object> resultBackOffice = new Map<String,Object>();
	List<CNT_Contracting_Status_BO__c> backOfficeList = new List<CNT_Contracting_Status_BO__c>();
	List<Case> updateListCase = new List<Case>();
	List<Case> caseErros = new List<Case>();
	List<Id> caseIdList = new List<Id>();

	List<Case> listCaso = [select Id, CaseNumber, AssetId, PointofDelivery__c, CNT_Potencia__c, CNT_Conexion_Transitoria__c, 
					CNT_Transit_Date_From__c, CNT_Transit_Date_To__c, CNT_Con_Transit_Guardia__c, CNT_Con_Transit_PotenciaMax__c,
					CNT_Con_Transit_PotenciaSimult__c, CNT_Con_Transit_CosenoPhi__c, CNT_Transit_Potencia_Total__c, Account.Country__c, 
					RecordType.DeveloperName, CNT_Contract__c, CNT_Observation__c, CNT_Hora_Diaria__c, CNT_Total_Dias_Utilizacao__c, CNT_Ramo__c,
					CNT_CIIU__c, CNT_Public_Ilumination__c, CNT_Change_Type__c, RecordTypeId, CNT_Economical_Activity__c, 
					Asset.NE__Order_Config__c, CNT_Documento_Pagamento__c, CNT_Payment_Method__c, CNT_Payment_Status__c, CNT_Mandate_Code__c, 
					CNT_Mandate_Amount__c, Contact.Name,Account.Name, Account.Id,  SuppliedEmail, PointofDeliveryAddress__c, 
					PointofDeliveryNumber__c, CNT_Ticket_Expiration_Date__c, Status, CNT_ProcessStatus__c
					from Case where CaseNumber IN: caseNumberList];

	Map<String, String> mapDate = new Map<String, String>(); 
	if(moveInDate == 'OS_DATE'){
		List<WorkOrder> listWorkOrder  = [select Id, Case.CaseNumber, CNT_DateExecutionVisit__c from WorkOrder WHERE Case.CaseNumber IN: (caseNumberList) AND CNT_FlowStatus__c = 'CNT003' ];
		for(WorkOrder myWorkOrder : listWorkOrder){
			mapDate.put(myWorkOrder.Case.CaseNumber,  String.valueOf(myWorkOrder.CNT_DateExecutionVisit__c));
		}
	}

	for(Case caso : listCaso){
		try{

			reqAlta = CNT_IntegrationDataBR.flow_ALTA_SAP_BR(caso.Id,true, true);
			// --------------- Tratamento de Data retroativa
			switch on moveInDate {
				when 'SET_DATE' {
					//reqAlta.Body.DATA_VALIDITA = Date.newInstance(2022,02,03);
					//reqAlta.Body.DATA_VALIDITA = Date.newInstance({ano},{mes},{dia});
				}
				when 'DIA_ATUAL' {
					reqAlta.Body.DATA_VALIDITA = Date.today();
				}	
				when 'DIA_POSTERIOR' {
					reqAlta.Body.DATA_VALIDITA = curretDate.addDays(1);
					
				}
				when 'OS_DATE' {// Baseado no dia de retorno da OS +2 (Pois Move Out eh feito em +1)
					if(String.isNotBlank(mapDate.get(caso.CaseNumber))){
						String moveOutCaseDate = mapDate.get(caso.CaseNumber);// Year/day/month
						Integer month = Integer.valueOf(moveOutCaseDate.substringAfterLast('-')) ;
						Integer day = Integer.valueOf(moveOutCaseDate.substringBetween('-')) ;
						Integer year = Integer.valueOf(moveOutCaseDate.substringBefore('-')) ;
						curretDate = date.newInstance(year, month, day);// Year, month, day
						reqAlta.Body.DATA_VALIDITA = curretDate.addDays(2);
					} else {
						reqAlta.Body.DATA_VALIDITA = Date.today();
					}
				}
				when 'CUSTOM_DATE' {
					if( String.isNotBlank(mapDateMoveIn.get(caso.CaseNumber)) ){
						month = Integer.valueOf(mapDateMoveIn.get(caso.CaseNumber).substringBefore('/')) ;
						day = Integer.valueOf(mapDateMoveIn.get(caso.CaseNumber).substringBetween('/')) ;
						year = Integer.valueOf(mapDateMoveIn.get(caso.CaseNumber).substringAfterLast('/')) ;
					}
					curretDate = date.newInstance(year, month, day);// Year, month, day
					reqAlta.Body.DATA_VALIDITA = curretDate.addDays(1);
					System.debug(':: myDate >>> '+day +'/'+month +'/'+year +' | '+reqAlta.Body.DATA_VALIDITA);
				}	
			}
			// --------------- update date ----------------------
			System.debug(':: JSON >>> '+JSON.serialize(reqAlta));
			
			HttpResponse response = CNT_IntegrationHelper.ALTA_242_BRSync(reqAlta,caso.Id);
			System.debug('response: '+response);

			resultBackOffice = CNT_Utility.resultRequest(response, JSON.serialize(reqAlta), caso, 'Facturador');

			backOffice = (CNT_Contracting_Status_BO__c) resultBackOffice.get('backoffice');
			backOfficeList.add(backOffice);
			
			Boolean responseStatus = (Boolean) resultBackOffice.get('status');
			if(responseStatus){
				caso.CNT_ProcessStatus__c = '';// Remove da fila
				caso.Status = 'CNT0008';
				updateListCase.add(caso);
				caseIdList.add(caso.Id);
			}
		}catch(Exception ex){
			System.debug('ERROR: '+ex.getStackTraceString() +' | '+ ex.getMessage() + ' | '+ex.getCause()); 
				
			caso.CNT_Controller242Success__c = 'AT';
			caso.CNT_ProcessStatus__c = 'CNT001';// Põe na fila de erro SAP
			caseErros.add(caso);

			backOffice = CNT_Utility.registerResult('Warning: Process Request', (String.isNotBlank(JSON.serialize(reqAlta))?JSON.serialize(reqAlta):''), caso)[0];
			backOffice.CNT_Comments__c = 'ERROR: '+ex.getStackTraceString() +' | '+ ex.getMessage() + ' | '+ex.getCause();
			backOfficeList.add(backOffice);
		}
	}

	if(comCompleteOrder && (caseIdList.size() > 0) ){
		CNT_ConfigurationUtility.completeOrderFlow(caseIdList);// List < id > listCaseId
	}

	if(backOfficeList.size() > 0){
		Insert backOfficeList;
		update updateListCase;
	}
	if(caseErros.size() > 0){
		update caseErros;
	}