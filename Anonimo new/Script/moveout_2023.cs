// Enviar ALTERAÇÃO CONTRATUAL SAP massivamente ate 50 registros
List<String> caseNumberList = new List<String>{{0}};
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
		caso.id =  item.Split('>')[0];
		caso.CNT_Controller242Success__c =  item.Split('>')[1];
		//caso.CNT_LastInvoiceOptions__c =  '7';
		caso.CNT_LastInvoiceOptions__c =  item.Split('>')[2];
		caso.PointofDelivery__c = item.Split('>')[3];
					
		system.Debug('estou aqui 1');
		
		
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
		else 
		{
		
			reqBaja = CNT_IntegrationDataBR.flow_BAJA_SAP_BR(caso.Id,caso.CNT_LastInvoiceOptions__c);
			System.debug('JSON: '+JSON.serialize(reqBaja));
					
			HttpResponse response = CNT_IntegrationHelper.BAJA_242_BRSync(reqBaja,caso.Id);
					
			resultBackOffice = CNT_Utility.resultRequest(response, JSON.serialize(reqBaja), caso, 'Facturador');
			backOffice = (CNT_Contracting_Status_BO__c) resultBackOffice.get('backoffice');
			backOfficeList.add(backOffice);
			Boolean responseStatusBaixa = (Boolean) resultBackOffice.get('status');				
						
			if (backOffice.Status__c == 'Envio Facturador : ERROR >> Resultado:"NÃO É POSSÍVEL REALIZAR MODIFICAÇÕES, INSTALAÇÃO BLOQUEADA POR MOTIVO DE LEITURA EM CAMPO"')
			{
				caso.cnt_processStatus__c = 'CNT004';		        	
				updateListCase.add(caso);
				continue;
			}
				
			if(responseStatusBaixa)
			{
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