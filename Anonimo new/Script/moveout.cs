// Enviar ALTERAÇÃO CONTRATUAL SAP massivamente ate 50 registros
List<String> caseNumberList = new List<String>{'379969589'};

CNT_IntegrationHelper.ALTA_BR_242 reqAlta = new CNT_IntegrationHelper.ALTA_BR_242();
CNT_IntegrationHelper.BAJA_BR_242 reqBaja = new CNT_IntegrationHelper.BAJA_BR_242();
List <WorkOrder> listWorkOrder = new List <WorkOrder>();
Map < String, Object > result = new Map <String, Object>();

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
                 CNT_Ticket_Expiration_Date__c, Status, CNT_LastInvoiceOptions__c,CreatedDate,CNT_Quote__c
	from Case where CaseNumber IN: caseNumberList];

List<CNT_Contracting_Status_BO__c> backOfficeList = new List<CNT_Contracting_Status_BO__c>();
List<Case> updateListCase = new List<Case>();

for(Case caso : listCaso)
{

		try
		{
			

			CNT_IntegrationHelper.BAJA_BR_239 reqBajaSYN = new CNT_IntegrationHelper.BAJA_BR_239();
			
			reqBajaSYN = CNT_IntegrationDataBR.flow_BAJA_Tecnica_BR(caso.Id);
			result = CNT_Utility.resultRequest(CNT_IntegrationHelper.BAJA_239_BRSync(reqBajaSYN, caso.Id), JSON.serialize(reqBajaSYN), caso, 'Sistema Tecnico');
			backOfficeList.add((CNT_Contracting_Status_BO__c) result.get('backoffice'));
			
			if (result.containsKey('workorder')) 
			{
				listWorkOrder.add((WorkOrder) result.get('workorder'));
			}			
			
			reqBaja = CNT_IntegrationDataBR.flow_BAJA_SAP_BR(caso.Id,caso.CNT_LastInvoiceOptions__c);
			reqBaja.Body.CreatedDate = date.ValueOf(String.valueOf(Date.Today()));
			reqBaja.Body.Request_Date = String.ValueOf(date.ValueOf(caso.createddate));
			System.debug('JSON: '+JSON.serialize(reqBaja));
			HttpResponse response = CNT_IntegrationHelper.BAJA_242_BRSync(reqBaja,caso.Id);
			
			resultBackOffice = CNT_Utility.resultRequest(response, JSON.serialize(reqBaja), caso, 'Facturador');
			backOffice = (CNT_Contracting_Status_BO__c) resultBackOffice.get('backoffice');
			backOfficeList.add(backOffice);
			Boolean responseStatusBaixa = (Boolean) resultBackOffice.get('status');
			
			System.debug('CASOS DE BACKOFFICE: '+ backOffice);
			
			if (backOffice.Status__c == 'Envio Facturador : ERROR >> Resultado:"Conta Contrato ativa no ponto de fornecimento diferente da conta contrato enviada na solicitação"')
			{
				caso.Status = 'CNT0008';
				caso.cnt_processStatus__c = '';
		        caso.CNT_ByPass__c = true;
				updateListCase.add(caso);
				lst_id.add(caso.id);					
			}	
			
			if(responseStatusBaixa )
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
			continue;
		}
}

Insert backOfficeList;
if(updateListCase.size() > 0){ update updateListCase; }
//Insert listWorkOrder;

//if (lst_id.size() > 0){CNT_ConfigurationUtility.completeOrderPB(lst_id);}

//if(updateListCase.size() > 0){ update updateListCase; }
