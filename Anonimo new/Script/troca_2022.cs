// Enviar ALTERAÇÃO CONTRATUAL SAP massivamente ate 50 registros
List<String> caseNumberList = new List<String>{'488509150'};

CNT_IntegrationHelper.ALTA_BR_242 reqAlta = new CNT_IntegrationHelper.ALTA_BR_242();
CNT_IntegrationHelper.BAJA_BR_242 reqBaja = new CNT_IntegrationHelper.BAJA_BR_242();
CNT_IntegrationHelper.CT_BR_239 reqSyn = new CNT_IntegrationHelper.CT_BR_239();

CNT_Contracting_Status_BO__c backOffice = new CNT_Contracting_Status_BO__c();
Map<String,Object> resultBackOffice = new Map<String,Object>();

List<id> lst_id = new List<id>();	
List<Case> listCaso = new List<Case>();


List<CNT_Contracting_Status_BO__c> backOfficeList = new List<CNT_Contracting_Status_BO__c>();
List<Case> updateListCase = new List<Case>();

for(String item : caseNumberList)
{

		String Tipo = '';
		String TipoEntrega = '';
		String DatadeVencimento = '';
		
		try
		{
		
		
			Case caso = [select Id,CaseNumber,AccountId,Account.Name,Account.RecordType.DeveloperName,Contact.Email,Account.IdentityType__c,
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
			Account.Id,PointofDeliveryAddress__c,PointofDeliveryNumber__c,CNT_Ticket_Expiration_Date__c,Status,cnt_processStatus__c,
			CNT_LastInvoiceOptions__c,CreatedDate,CNT_Quote__c,CNT_Controller242Success__c,Asset.Contract__r.CNT_ExternalContract_ID__c
			from Case where casenumber = :item AND(RecordType.DeveloperName = 'CNT_OwnershipChange') and status !='CNT0008'];
			
			system.Debug('estou aqui 1');
			if (caso.CNT_Controller242Success__c != 'AT')
			{				
				reqBaja = CNT_IntegrationDataBR.flow_BAJA_SAP_BR(caso.Id,caso.CNT_LastInvoiceOptions__c);
				System.debug('JSON: '+JSON.serialize(reqBaja));
				
				HttpResponse response = CNT_IntegrationHelper.BAJA_242_BRSync(reqBaja,caso.Id);
				
				resultBackOffice = CNT_Utility.resultRequest(response, JSON.serialize(reqBaja), caso, 'Facturador');
				backOffice = (CNT_Contracting_Status_BO__c) resultBackOffice.get('backoffice');
				backOfficeList.add(backOffice);
				Boolean responseStatusBaixa = (Boolean) resultBackOffice.get('status');
				
				if (!responseStatusBaixa)
				{
					
					if (backOffice.Status__c == 'Envio Facturador : ERROR >> Resultado:"NÃO É POSSÍVEL REALIZAR MODIFICAÇÕES, INSTALAÇÃO BLOQUEADA POR MOTIVO DE LEITURA EM CAMPO"')
					{
							caso.cnt_processStatus__c = 'CNT004';		        	
							updateListCase.add(caso);
							continue;
					}
				}			
			}
						
			reqAlta = CNT_IntegrationDataBR.flow_ALTA_SAP_BR(caso.Id,true, true);	
			
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
        	//}
		}
		catch(Exception ex)
		{
			System.debug(' Erro: ' +ex);
			continue;
		}
}

Insert backOfficeList;
if(updateListCase.size() > 0){update updateListCase;}

CNT_CompleteOrderBatch sch = new CNT_CompleteOrderBatch(lst_id);
Database.executebatch(sch, 2);


