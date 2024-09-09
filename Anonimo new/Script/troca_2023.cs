// Enviar ALTERAÇÃO CONTRATUAL SAP massivamente ate 50 registros
List<String> caseNumberList = new List<String>{{0}};

CNT_IntegrationHelper.ALTA_BR_242 reqAlta = new CNT_IntegrationHelper.ALTA_BR_242();
CNT_IntegrationHelper.BAJA_BR_242 reqBaja = new CNT_IntegrationHelper.BAJA_BR_242();

CNT_Contracting_Status_BO__c backOffice = new CNT_Contracting_Status_BO__c();
Map<String,Object> resultBackOffice = new Map<String,Object>();

List<id> lst_id = new List<id>();	
List<Case> listCaso = new List<Case>();


List<CNT_Contracting_Status_BO__c> backOfficeList = new List<CNT_Contracting_Status_BO__c>();
List<Case> updateListCase = new List<Case>();

for(String item : caseNumberList)
{	
	try
	{			
		Case caso = new Case();			
		String fat_digital = '';
		
		try
		{
			caso.id =  item.Split('>')[0];
			caso.CNT_Controller242Success__c =  item.Split('>')[1];
			caso.CNT_LastInvoiceOptions__c =  item.Split('>')[2];
			fat_digital =  item.Split('>')[3];
			caso.PointofDelivery__c =  item.Split('>')[4];
		}
		catch(Exception ex)
		{
			
		}
		
		CNT_VFC061_Suministro.ResultButton rfact =  CNT_VFC061_Suministro.getSuministro(caso.PointofDelivery__c);
		Boolean resultRequestFlag = (rfact.statusResult == 'FATURAMENTO' ? true : false);
				
		System.debug(':: FATURAMENTO >>> '+ rfact.statusResult);
		
		System.debug(':: Status >>> '+ resultRequestFlag);
				
		if(resultRequestFlag){
			caso.CNT_ProcessStatus__c = 'CNT004';
			caso.CNT_Controller242Success__c = 'PF';
			caso.CNT_LastInvoiceOptions__c = '7';
			updateListCase.add(caso);
			
			backOffice = CNT_Utility.registerResult(rfact.backOfficeResult, '', caso)[0];
			backOfficeList.add(backOffice);
		}
		else {
		
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
								
				if (backOffice.Status__c.containsIgnoreCase('BLOQUEADA')) 
				{
					caso.cnt_processStatus__c = 'CNT004';	
					caso.CNT_LastInvoiceOptions__c = '7';
					updateListCase.add(caso);
					continue;
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
				if(String.isNotBlank(fat_digital) && fat_digital.equals('FATURA_DIGITAL'))
				{
					List<CNT_Contracting_Status_BO__c> listBackOff = new List<CNT_Contracting_Status_BO__c> ();
					CNT_VFC061_Suministro.ResultButton result = new CNT_VFC061_Suministro.ResultButton();
											
					result = CNT_VFC061_Suministro.PutPerfilFacturacion_BPC_BRSync(caso.Id);		
					backOfficeList.addAll(CNT_Utility.registerResult(result.backOfficeResult, result.statusResult, new Case(Id = caso.Id)));
				}		
				
				
				caso.Status = 'CNT0008';
				caso.cnt_processStatus__c = '';
				caso.CNT_ByPass__c = true;
				updateListCase.add(caso);
				lst_id.add(caso.id);	
			}        	
		}
	}
	catch(Exception ex)
	{
		System.debug(' Erro: ' +ex);
		continue;
	}
}

Insert backOfficeList;
if(updateListCase.size() > 0){update updateListCase;}

if(lst_id.size() > 0) {CNT_ConfigurationUtility.completeOrderFlow(lst_id);}