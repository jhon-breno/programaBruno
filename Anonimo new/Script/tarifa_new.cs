// Enviar ALTERA��O CONTRATUAL SAP massivamente ate 50 registros
List<String> caseNumberList = new List<String>{'201355144','205953342'};

CNT_IntegrationHelper.CCC_BR_242 reqBaja = new CNT_IntegrationHelper.CCC_BR_242();

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
	from Case where CaseNumber IN: caseNumberList];

List<CNT_Contracting_Status_BO__c> backOfficeList = new List<CNT_Contracting_Status_BO__c>();
List<Case> updateListCase = new List<Case>();

for(Case caso : listCaso)
{

		try
		{
			//SAP
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

//if (lst_id.size() > 0){CNT_ConfigurationUtility.completeOrderFlow(lst_id);}

for (id orderId : lst_id)
{
	try	
	{
		CNT_ConfigurationUtility.completeOrder(orderId);
	}
	catch(Exception ex)
	{
		System.debug(' Erro: ' +ex);
		continue;
	}
}

if(updateListCase.size() > 0){ update updateListCase; }
