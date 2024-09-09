	private void inactivateContractAndAsset (Contract_line_item__c asset)
	{

		Asset ass = new asset();
		Contract contrato = new contract();
		
		try {
				ass.Id = asset.asset__c;
				ass.Status = 'Obsolete';
				ass.NE__Status__c = 'Disconnected';
				assetListToDisconnected.add(ass);

				contrato.Id = asset.contract__c;
				contrato.Status = 'Inactivated';
				contrato.CNT_ContractChangeDate__c = DateTime.now();
				contractListToDesactivate.add(contrato);

			} catch(Exception Ex) {
				System.debug('Error in inactivateContractAndAsset method >>>' + Ex + '|' + Ex.getStackTraceString() + ' | ' + Ex.getMessage() + ' | ' + Ex.getCause());
			}
	}
	
	private Id autoCreateCaseToDisconnect( Id accountId, Id contactId, Id contractId, Id podId, String contractNumber, Id assetId) 
	{
		System.debug('Enter on autoCreateCaseToDisconnect');

		Case newCase = new Case();
		newCase.RecordTypeId = recordTypeBaja;
		newCase.CNT_Conexion_Transitoria__c = true;
		newCase.AccountId = accountId;
		newCase.ContactId = contactId;
		newCase.CNT_Contract__c = contractId;
		newCase.AssetId = assetId;
		newCase.Type = 'TIP003'; //Solicitacao
		newCase.PointofDelivery__c = podId;
		newCase.CNT_LastInvoiceOptions__c = '7';
		newCase.Description = 'Encerramento Contratual por término de período de Ligação Provisória. Contrato Nº: ' + contractNumber;
		newCase.Status = 'CNT0002';
		newCase.SubStatus__c = 'BOinP';
		newCase.Reason = 'MOT016';
		newCase.SubCauseBR__c = '38';
		newCase.CNT_LastInvoiceOptions__c = '7';

		insert newCase;
		return newCase.Id;
	}
		
	List <Id> listCaseEndingCreated = new List <Id>();
    List <Contract> contractListToDesactivate = new List <Contract>();
    List <Asset> assetListToDisconnected = new List <Asset>();
    List <Id> listPodContract = new List <Id>();
	Id recordTypeBaja = CNT_CaseUtility.getRecordTypeID('ANY_case_Baja');
    
	Id caseCreateId = null;
	Id assetFromContractId = null;
	
	Contract_line_item__c loopCase = [select  Billing_Profile__r.PointofDelivery__c, Asset__c,contract__c, Contract__r.AccountId, Contract__r.CNT_Case__r.ContactId,Contract__r.CNT_ExternalContract_ID__c 
	from Contract_Line_Item__c where AccountContract__c ='{0}' and company__c='2005'];
	
	assetFromContractId = loopCase.Asset__c;
	
	System.debug('assetFromContractId >> '+ assetFromContractId);
	inactivateContractAndAsset(loopCase);
	
	caseCreateId = autoCreateCaseToDisconnect(loopCase.Contract__r.AccountId, loopCase.Contract__r.CNT_Case__r.ContactId, loopCase.Contract__c, loopCase.Billing_Profile__r.PointofDelivery__c, loopCase.Contract__r.CNT_ExternalContract_ID__c, assetFromContractId);
	listCaseEndingCreated.add(caseCreateId);

	if(contractListToDesactivate.size() > 0)
	{
		update contractListToDesactivate;
	}

	if(assetListToDisconnected.size() > 0)
	{
		update assetListToDisconnected;
	}

    /*if(listCaseEndingCreated.size() > 0)
	{
		CNT_ScheduleHandleBaixa sch = new CNT_ScheduleHandleBaixa(listCaseEndingCreated);
		Id Jobid = Database.executebatch(sch, 100);
    }*/