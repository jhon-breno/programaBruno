	// Enviar ALTERAÇÃO CONTRATUAL SAP massivamente ate 50 registros
List<String> caseNumberList = new List<String>{{0}};

	CNT_IntegrationHelper.ALTA_BR_242 reqAlta = new CNT_IntegrationHelper.ALTA_BR_242();
	CNT_IntegrationHelper.BAJA_BR_242 reqBaja = new CNT_IntegrationHelper.BAJA_BR_242();

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
		from Case where CaseNumber IN: caseNumberList and status != 'CNT0008'];

	List<CNT_Contracting_Status_BO__c> backOfficeList = new List<CNT_Contracting_Status_BO__c>();
	List<Case> updateListCase = new List<Case>();

	for(Case caso : listCaso)
	{

			try
			{
					reqAlta = CNT_IntegrationDataBR.flow_ALTA_SAP_BR(caso.Id,false, false);//(caso.Id);
					
					
					reqAlta.Body.DATA_VALIDITA = date.ValueOf(caso.CreatedDate).addDays(1);
					reqAlta.Body.CLASSE = 'TRANSFER';
					reqAlta.Body.ATTIVITA = 'ACCOUNT';
					
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
				continue;
			}
	}

	Insert backOfficeList;
	if(updateListCase.size() > 0){ update updateListCase; }

	if (lst_id.size() > 0){CNT_ConfigurationUtility.completeOrderPB(lst_id);}

	if(updateListCase.size() > 0){ update updateListCase; }
