 public List <Id> listCaseEndingCreated = new List <Id>();
 public List <Contract> contractListToDesactivate = new List <Contract>();
 public List <Asset> assetListToDisconnected = new List <Asset>();
 public List <Id> listPodContract = new List <Id>();
 public Id recordTypeBaja = CNT_CaseUtility.getRecordTypeID('ANY_case_Baja');
 public final String query;
 List<String> contracts = new List<String>{{0}};
 
 List<Contract> lst = [SELECT Id, Name, ContractNumber, CNT_EndDate__c, CNT_Case__c, Status, CNT_Case__r.PointofDelivery__c 
										FROM Contract WHERE ID in :contracts AND
                                        Status IN ('Activated','StandBy') AND 
                                        CNT_Conexion_Transitoria__c = true AND 
                                        CNT_EndDate__c < TODAY AND 
                                        ((CNT_Case__r.RecordType.DeveloperName = 'CNT_BR_Alta_Contrato' AND 
                                        CNT_Case__r.SubCauseBR__c = '44' AND 
                                        CNT_Case__r.Status = 'CNT0008' AND 
                                        CNT_Case__r.Country__c = 'BRASIL' AND 
                                        CNT_Case__r.CNT_Payment_Status__c IN ('CNT001','CNT003','CNT005'))
                                        OR
                                        (CNT_Case__r.RecordType.DeveloperName = 'CNT_ChangeProduct' AND 
                                        CNT_Case__r.Status = 'CNT0008'AND 
                                        CNT_Case__r.Country__c = 'BRASIL')) ];
										
										
orchesContractVigency(lst);
finish();
 

private void orchesContractVigency(List<Contract> scope){
	
        Map <Id, PointofDelivery__c> mapPodGeneral = new Map <Id, PointofDelivery__c>();
		Map <Id, Id> mapContractAsset = new Map <Id, Id>();
		List <PointofDelivery__c> listPodWithDeviceAndAsset = new List <PointofDelivery__c>();
		List <Case> listPodWithClosureCase = new List <Case>();
		List <Case> listPodWithCaseToClose = new List <Case>();
		List <Id> listContractId = new List <Id>();
		
		Id caseCreateId = null;
		Id assetFromContractId = null;

		if(!scope.isEmpty()) {
			for(Contract cont : scope){
				listPodContract.add(cont.CNT_Case__r.PointofDelivery__c);
			}

			//To check if pod have device and asset
			listPodWithDeviceAndAsset = [SELECT Id FROM PointofDelivery__c WHERE Id IN (SELECT PointofDelivery__c FROM Asset) 
											AND Id IN : listPodContract];

			System.debug('listPodWithDeviceAndAsset.size() >>> '+listPodWithDeviceAndAsset.size());
			
			for(PointofDelivery__c pod : listPodWithDeviceAndAsset){
				mapPodGeneral.put(pod.Id, pod);
			}

			System.debug('mapPodGeneral.size() at creating >>> '+mapPodGeneral.size());

			listPodWithClosureCase = [SELECT PointofDelivery__c FROM CASE WHERE RecordType.DeveloperName = 'CNT_Baja_Contrato' 
									AND Status <> 'CNT0009' 
									AND PointofDelivery__c IN : listPodWithDeviceAndAsset];

			System.debug('listPodWithClosureCase.size() >>> '+listPodWithClosureCase.size());

			for(Case c : listPodWithClosureCase){
				if(mapPodGeneral.containsKey(c.PointOfDelivery__c)){
					mapPodGeneral.remove(c.PointOfDelivery__c);
				}
			}

			System.debug('JSON BEGIN >>> ');
			System.debug('mapPodGeneral.size() at ending >>> '+mapPodGeneral.size());
			System.debug(JSON.serializePretty(mapPodGeneral));
			System.debug('JSON END >>> ');
			System.debug(mapPodGeneral.values());

			// Closure list only for pods without case Inprog or closed
			listPodWithCaseToClose = [SELECT Id, AccountId, ContactId, CNT_LastInvoiceOptions__c, CNT_Contract__c, 
										CNT_Contract__r.ContractNumber, PointofDelivery__c FROM CASE WHERE 
											RecordType.DeveloperName = 'CNT_BR_Alta_Contrato' 
											AND Status = 'CNT0008' 
											AND PointOfDelivery__c IN : mapPodGeneral.keySet()];

			// Get same asset related to contract to associate to future closure case
			for (Case loopCase : listPodWithCaseToClose){
				listContractId.add(loopCase.CNT_Contract__c);
			}

			System.debug('listContractId.size() >>> '+listContractId.size());

			for (Asset ass : [SELECT Id, Contract__c FROM Asset WHERE Contract__c IN : listContractId]){
				mapContractAsset.put(ass.Contract__c, ass.Id);
			}

			System.debug('mapContractAsset.size() >>> '+mapContractAsset.size());

			for(Case loopCase : listPodWithCaseToClose){
				try{
					if (loopCase.CNT_Contract__c <> null){
						assetFromContractId = mapContractAsset.get(loopCase.CNT_Contract__c);
						System.debug('assetFromContractId >> '+ assetFromContractId);
						inactivateContractAndAsset(assetFromContractId);
						caseCreateId = autoCreateCaseToDisconnect(loopCase.Id, loopCase.AccountId, loopCase.ContactId, loopCase.CNT_Contract__c, loopCase.PointofDelivery__c, loopCase.CNT_Contract__r.ContractNumber, assetFromContractId);
						listCaseEndingCreated.add(caseCreateId);
					}
				} catch(Exception ex){ 
					System.debug('Error to create closure case >>> '+ex.getStackTraceString() +' | '+ ex.getMessage() + ' | '+ex.getCause());
				}
			}

		}
    }
    
	private void finish(){
	
		System.debug('contractListToDesactivate.size() >>> '+contractListToDesactivate.size());
		System.debug('contractListToDesactivate >> ' + contractListToDesactivate);

		System.debug('assetListToDisconnected.size() >>> '+assetListToDisconnected.size());
		System.debug('assetListToDisconnected >> ' + assetListToDisconnected);

		System.debug('listCaseEndingCreated.size() >>> '+listCaseEndingCreated.size());
		System.debug('CNT_ScheduleCheckContractVigency.finishlistCaseEndingCreated >> ' + listCaseEndingCreated);

		if(contractListToDesactivate.size() > 0){
			update contractListToDesactivate;
		}

		if(assetListToDisconnected.size() > 0){
			update assetListToDisconnected;
		}

        if(listCaseEndingCreated.size() > 0){
			CNT_ScheduleHandleBaixa sch = new CNT_ScheduleHandleBaixa(listCaseEndingCreated);
			Database.executebatch(sch, 5);
        }
		System.debug('CNT_ScheduleCheckContractVigency.finish');
    }

	private Id autoCreateCaseToDisconnect(Id idCase, Id accountId, Id contactId, Id contractId, Id podId, String contractNumber, Id assetId) {
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
		newCase.Description = 'Encerramento Contratual por término de perí­odo de Ligação Provisória. Contrato Nº: ' + contractNumber;
		newCase.Status = 'CNT0002';
		newCase.SubStatus__c = 'BOinP';
		newCase.Reason = 'MOT016';
		newCase.SubCauseBR__c = '38';
		newCase.CNT_LastInvoiceOptions__c = '5';

		insert newCase;
		return newCase.Id;
	}

	private void inactivateContractAndAsset (Id assetId){

		Asset ass = [SELECT Id, Contract__c, Status, NE__Status__c
						FROM Asset
						WHERE Id =: assetId LIMIT 1];

		Contract contrato = [SELECT Id, Status, CNT_ContractChangeDate__c
							FROM Contract
							WHERE Id =: ass.Contract__c LIMIT 1];
		try {
				ass.Status = 'Obsolete';
				ass.NE__Status__c = 'Disconnected';
				assetListToDisconnected.add(ass);

				contrato.Status = 'Inactivated';
				contrato.CNT_ContractChangeDate__c = DateTime.now();
				contractListToDesactivate.add(contrato);

			} catch(Exception Ex) {
				System.debug('Error in inactivateContractAndAsset method >>>' + Ex + '|' + Ex.getStackTraceString() + ' | ' + Ex.getMessage() + ' | ' + Ex.getCause());
			}
	}
