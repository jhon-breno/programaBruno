// Enviar ALTERAÇÃO CONTRATUAL SAP massivamente ate 50 registros
List<String> caseNumberList = new List<String>{'81151029'};

CNT_IntegrationHelper.ALTA_BR_242 reqAlta = new CNT_IntegrationHelper.ALTA_BR_242();

CNT_Contracting_Status_BO__c backOffice = new CNT_Contracting_Status_BO__c();
Map<String,Object> resultBackOffice = new Map<String,Object>();

List<id> lst_id = new List<id>();
	
List<Case> listCaso = new List<Case>();
listCaso = [select Id, AssetId, PointofDelivery__c, CNT_Potencia__c, CNT_Conexion_Transitoria__c, CNT_Transit_Date_From__c, 
                 CNT_Transit_Date_To__c,  CNT_Con_Transit_PotenciaMax__c, CNT_Con_Transit_PotenciaSimult__c, 
                 CNT_Con_Transit_CosenoPhi__c, CNT_Transit_Potencia_Total__c, Account.Country__c, RecordType.DeveloperName, CNT_Contract__c,
                 CNT_Observation__c, CNT_Hora_Diaria__c, CNT_Total_Dias_Utilizacao__c, CNT_Ramo__c, CNT_CIIU__c, CNT_Public_Ilumination__c,
                 CNT_Change_Type__c, RecordTypeId, CNT_Economical_Activity__c, Asset.NE__Order_Config__c, 
                 CNT_Mandate_Code__c, CNT_Mandate_Amount__c, Contact.Name,Account.Name, Account.Id,   PointofDeliveryAddress__c, PointofDeliveryNumber__c,
                 CNT_Ticket_Expiration_Date__c, Status, CNT_LastInvoiceOptions__c,CreatedDate
	from Case where casenumber IN: caseNumberList];

List<CNT_Contracting_Status_BO__c> backOfficeList = new List<CNT_Contracting_Status_BO__c>();
List<Case> updateListCase = new List<Case>();

for(Case item : listCaso)
{

		System.debug('RUN::altaTransitDocGen');
		CNT_IntegrationHelper.ALTA_BR_242 reqAlta = new CNT_IntegrationHelper.ALTA_BR_242();
		CNT_Contracting_Status_BO__c backOffice = new CNT_Contracting_Status_BO__c();
		Map<String, Object> resultBackOffice = new Map<String, Object> ();
		HttpResponse response = new HttpResponse();

		Case caso = [SELECT Id, CNT_Contract__r.StartDate, CNT_Payment_Method__c, CNT_Contract__r.CNT_EndDate__c,
		             CNT_Transit_Potencia_Total__c, CNT_Documento_Pagamento__c FROM Case WHERE id = :item.id LIMIT 1];

		Contract cont = [SELECT Id, CNT_ExternalContract_ID__c, CNT_Free_Client__c, StartDate, CNT_Economical_Activity__c
		                 FROM Contract WHERE CNT_Case__c = :caso.Id];

		Contract_Line_Item__c cli = [SELECT Billing_Profile__r.CNT_Due_Date__c, Billing_Profile__r.Delivery_Type__c,
		                             Billing_Profile__r.Address__r.StreetMD__r.Name
		                             FROM Contract_Line_Item__c WHERE Contract__c = :cont.Id AND CNT_Product__c LIKE 'Grupo%'];

		reqAlta = CNT_IntegrationDataBR.flow_ALTA_SAP_BR(caso.Id, false, true);
		
		if (caso.CNT_Payment_Method__c != null && caso.CNT_Payment_Method__c.equalsIgnoreCase('CNT001')) { //  PAGAMENTO ANTECIPADO
			reqAlta.Body.CLASSE = 'MOVE IN';
		}
		reqAlta.Body.ZZFLG_ONLINE = 'X';
		reqAlta.Body.ATTIVITA = 'INTERIM';
		reqAlta.Body.DATA_VALIDITA = caso.CNT_Contract__r.StartDate;
		reqAlta.Body.MI_VENDE = caso.CNT_Contract__r.CNT_EndDate__c;
		reqAlta.Body.IM_FACTOR_1 = String.valueOf(caso.CNT_Transit_Potencia_Total__c);
		reqAlta.Body.AC_ZAHLKOND = cli.Billing_Profile__r.CNT_Due_Date__c.length() == 1 ? 'CP0' + cli.Billing_Profile__r.CNT_Due_Date__c : 'CP' + cli.Billing_Profile__r.CNT_Due_Date__c; //data vencimento //String.valueOf(Date.today());

		response = CNT_IntegrationHelper.ALTA_242_BRSync(reqAlta, caso.Id);

		System.debug('response: ' + response);
		// Jonas C. Jr. - BUGFIX-41 - 6/28/2019
		resultBackOffice = CNT_Utility.resultRequest(response, JSON.serialize(reqAlta), caso, 'Facturador');
		backOffice = (CNT_Contracting_Status_BO__c) resultBackOffice.get('backoffice');

		// Jonas C. Jr. - Issue#77 - 8/26/2019
		String checkStringHttp = (String) resultBackOffice.get('message');

		if (String.isBlank(caso.CNT_Documento_Pagamento__c)) {
			caso.CNT_Documento_Pagamento__c = (String) resultBackOffice.get('message');
		} else if (checkStringHttp.contains('http')) {
			caso.CNT_Documento_Pagamento__c = (String) resultBackOffice.get('message');
		}
		//caso.CNT_Documento_Pagamento__c = (String) resultBackOffice.get('message');

		caso.CNT_Ticket_Expiration_Date__c = Date.today().addDays(5);

		Update caso;
		Insert backOffice;
}

Insert backOfficeList;
if(updateListCase.size() > 0){ update updateListCase; }

if (lst_id.size() > 0){CNT_ConfigurationUtility.completeOrderFlow(lst_id);}

if(updateListCase.size() > 0){ update updateListCase; }
