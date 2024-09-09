// Enviar ALTERAÇÃO CONTRATUAL SAP massivamente ate 50 registros
List<String> caseNumberList = new List<String>{'46284371'};
List<id> lst_id = new List<id>();
		
List<Case> listCaso = new List<Case>();
listCaso = [select Id, AssetId, PointofDelivery__c, CNT_Potencia__c, CNT_Conexion_Transitoria__c, CNT_Transit_Date_From__c, 
                 CNT_Transit_Date_To__c,  CNT_Con_Transit_PotenciaMax__c, CNT_Con_Transit_PotenciaSimult__c, 
                 CNT_Con_Transit_CosenoPhi__c, CNT_Transit_Potencia_Total__c, Account.Country__c, RecordType.DeveloperName, CNT_Contract__c,
                 CNT_Observation__c, CNT_Hora_Diaria__c, CNT_Total_Dias_Utilizacao__c, CNT_Ramo__c, CNT_CIIU__c, CNT_Public_Ilumination__c,
                 CNT_Change_Type__c, RecordTypeId, CNT_Economical_Activity__c, Asset.NE__Order_Config__c, 
                 CNT_Mandate_Code__c, CNT_Mandate_Amount__c, Contact.Name,Account.Name, Account.Id,   PointofDeliveryAddress__c, PointofDeliveryNumber__c,
                 CNT_Ticket_Expiration_Date__c, Status, CNT_LastInvoiceOptions__c,CreatedDate,CNT_Quote__c
	from Case where CaseNumber IN: caseNumberList ];

List<CNT_Contracting_Status_BO__c> backOfficeList = new List<CNT_Contracting_Status_BO__c>();
List<Case> updateListCase = new List<Case>();

for(Case caso : listCaso)
{
		try
		{			
		
				try
				{
					NE__Order__c ord = [select id from NE__Order__c where CNT_Case__r.id= :caso.id and RecordTypeId = '01236000000yF3XAAU'];					
				}
				catch(Exception ex)
				{
					CNT_ConfigurationUtility.cloneOrder(caso.CNT_Quote__c,null,caso.id);
				}
				
				lst_id.add(caso.id);
        	
		}
		catch(Exception ex)
		{
			continue;
		}
}

if (lst_id.size() > 0){CNT_ConfigurationUtility.completeOrderFlow(lst_id);}
