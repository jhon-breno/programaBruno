// Enviar ALTERAÇÃO CONTRATUAL SAP massivamente ate 50 registros
List<String> caseNumberList = new List<String>{{0}};
List<id> lst_id = new List<id>();
		
List<Case> listCaso = new List<Case>();
listCaso = [select Id, AssetId, PointofDelivery__c, CNT_Potencia__c, CNT_Conexion_Transitoria__c, CNT_Transit_Date_From__c, 
                 CNT_Transit_Date_To__c,  CNT_Con_Transit_PotenciaMax__c, CNT_Con_Transit_PotenciaSimult__c, 
                 CNT_Con_Transit_CosenoPhi__c, CNT_Transit_Potencia_Total__c, Account.Country__c, RecordType.DeveloperName, CNT_Contract__c,
                 CNT_Observation__c, CNT_Hora_Diaria__c, CNT_Total_Dias_Utilizacao__c, CNT_Ramo__c, CNT_CIIU__c, CNT_Public_Ilumination__c,
                 CNT_Change_Type__c, RecordTypeId, CNT_Economical_Activity__c, Asset.NE__Order_Config__c, 
                 CNT_Mandate_Code__c, CNT_Mandate_Amount__c, Contact.Name,Account.Name, Account.Id,   PointofDeliveryAddress__c, PointofDeliveryNumber__c,
                 CNT_Ticket_Expiration_Date__c, Status, CNT_LastInvoiceOptions__c,CreatedDate,CNT_Quote__c
	from Case where CNT_Contract__r.contractnumber IN: caseNumberList AND(RecordType.DeveloperName = 'CNT_OwnershipChange')];

List<CNT_Contracting_Status_BO__c> backOfficeList = new List<CNT_Contracting_Status_BO__c>();
List<Case> updateListCase = new List<Case>();

for(Case caso : listCaso)
{
		try
		{			
		
				NE__Order__c ord;
				
				try
				{
					ord = [select id from NE__Order__c where CNT_Case__r.id= :caso.id and RecordTypeId != '01236000000yF3XAAU'];					
				}
				catch(Exception ex)
				{
					ord = [select id from NE__Order__c where CNT_Case__r.id= :caso.id and RecordTypeId = '01236000000yF3XAAU'];					
					ord.RecordTypeId ='01236000000yF3ZAAU';
					
					update ord;
				}
				
				
				try
				{
					ord = [select id from NE__Order__c where CNT_Case__r.id= :caso.id and RecordTypeId = '01236000000yF3XAAU'];					
				}
				catch(Exception ex)
				{
					CNT_ConfigurationUtility.cloneOrder(caso.CNT_Quote__c,null,caso.id);
					ord = [select id from NE__Order__c where CNT_Case__r.id= :caso.id and RecordTypeId = '01236000000yF3XAAU'];					
				}
				
				lst_id.add(ord.id);
				
				caso.Status = 'CNT0008';
				caso.cnt_processStatus__c = '';
		   		updateListCase.add(caso);
				
        	
		}
		catch(Exception ex)
		{
			continue;
		}
}

for (id orderId : lst_id)
{
	try	
	{
		system.debug('orderId>>>' + orderId);
    ID orderRTypeId = CNT_CaseUtility.getRecordTypeID('ANY_ne_order_Order');
    Map<String, CNT_CustomerAnalysis__c> mapCustomAnal = new Map<String, CNT_CustomerAnalysis__c> ();
    Case relatedCase;

    list<NE__Order__c> lstOrders = [SELECT Id, NE__OrderStatus__c, CNT_Case__c, CNT_Case__r.PointofDelivery__c,
                                    Point_of_delivery__c, NE__AccountId__c, CNT_Case__r.CNT_Change_Type__c,
                                    CNT_Case__r.CNT_Tipo_de_Ajuste__c, NE__AccountId__r.Name, NE__AccountId__r.Country__c,
                                    NE__CatalogId__c, CNT_Case__r.RecordTypeId
                                    FROM NE__Order__c
                                    WHERE Id = :orderId];
    if (lstOrders.size() > 0) {

      PointofDelivery__c pod = [SELECT Id, PointofDeliveryStatus__c, ConnectionStatus__c, CNT_Contract__c, CNT_Rural_Irrigating__c
                                FROM PointofDelivery__c
                                WHERE Id = :lstOrders[0].CNT_Case__r.PointofDelivery__c];

      if (lstOrders[0].NE__AccountId__r.Country__c == 'BRASIL') 
	  {

        relatedCase = [SELECT Id, CNT_Contract__c, AssetId, CNT_Change_Type__c, Asset.Contract__c, Account.Country__c, Accountid, CNT_Controller242Success__c, CNT_White_Rate__c, CNT_Rural_Irrigating__c, CNT_Payment_Status__c, CNT_Tipo_de_Ajuste__c, CNT_Cliente_Baixa_Renda__c, CNT_Conexion_Transitoria__c, CNT_Free_Client__c, ContactId, CNT_Payment_Method__c, Status, CNT_Public_Ilumination__c, CNT_ByPass__c
                       FROM Case
                       WHERE Id = :lstOrders[0].CNT_Case__c];

        //Contract con = [Select Id From Contract Where CNT_Case__c =: lstOrders[0].CNT_Case__c];
        System.debug('CNT_Contract__c>>>' + relatedCase.CNT_Contract__c);
        System.debug('Status>>>' + relatedCase.Status);
        Contract con = [SELECT Id, CNT_Distributed_Generation_Eval__c, Account.RecordType.DeveloperName,
                        Account.CompanyID__c, ContractNumber
                        FROM Contract
                        WHERE Id = :relatedCase.CNT_Contract__c];

						System.debug('Completo>>>' + con);
        Asset relatedAsset;

        //ALTA
        if (lstOrders[0].CNT_Case__r.RecordTypeId == CNT_CaseUtility.getRecordTypeID('BR_case_Alta') || lstOrders[0].CNT_Case__r.RecordTypeId == CNT_CaseUtility.getRecordTypeID('ANY_case_OwnerChange')) 
		{
		  System.debug('1');
          //ONLY IF ASSET DOES NOT EXIST
          if ([SELECT Id FROM Asset WHERE NE__Order_Config__c = :orderId].size() == 0) 
		  {
			System.debug('2');
            //create asset
            String ordId = NE.JS_RemoteMethods.order2asset(orderId);
            system.debug('ordId: ' + ordId);
            //relatedAsset = [Select NE__Status__c From Asset Where Id =: assetId];
            relatedAsset = [SELECT id, NE__Status__c, CNT_Case__c, Contract__c, Country__c, PointofDelivery__c, ContactId FROM Asset WHERE NE__Order_Config__c = :ordId ORDER BY CreatedDate desc LIMIT 1];
            System.debug('3');
			relatedAsset.CNT_Case__c = relatedCase.Id;
            relatedAsset.NE__Status__c = 'Active';
            relatedAsset.Contract__c = con.Id;
            relatedAsset.Country__c = relatedCase.Account.Country__c;
            relatedAsset.PointofDelivery__c = pod.Id;
            relatedAsset.ContactId = relatedCase.ContactId;

            // Jonas Junior 5/27/2019 - BUGFIX-10
            //externalIdAsset(relatedAsset, con);
			
			String codeAccount = ('B2B_BRASIL'.equalsIgnoreCase(con.Account.RecordType.DeveloperName) ? 'B2B' : ('B2C_BRASIL'.equalsIgnoreCase(con.Account.RecordType.DeveloperName) ? 'B2C' : ''));

			if (con.Account.CompanyID__c == '2003') {
			  relatedAsset.ExternalId__c = con.ContractNumber + '0113BRACOE' + codeAccount;
			  relatedAsset.Company__c = con.Account.CompanyID__c;
			} else if (con.Account.CompanyID__c == '2005') {
			  relatedAsset.ExternalId__c = con.ContractNumber + '0113BRAAMA' + codeAccount;
			  relatedAsset.Company__c = con.Account.CompanyID__c;
			}

            update relatedAsset;
            System.debug('<<<ASSET OK>>>' + ordId);
			
			Contract_Line_Item__c cli = new Contract_Line_Item__c();
			try
			{
				cli = [select id From Contract_Line_Item__c where Contract__c =: con.Id and CNT_Product__c like 'Grupo%' limit 1];
				
			}
			catch(Exception ex)
			{
				
			}
			
			cli.asset__c = relatedAsset.Id;
			
			system.debug('<<<CON OK>>>' + con.Id);
			
			system.debug('<<<CLI OK>>>' + cli.Id);
			
			system.debug('<<<ASSE OK>>>' + relatedAsset.Id);
			
			update cli;
          }
		  else
		  {
			System.debug('oioi');
			
			try
			{
				relatedAsset = [SELECT id, NE__Status__c, CNT_Case__c, Contract__c, Country__c, PointofDelivery__c, ContactId FROM Asset WHERE Contract__c = :con.Id ORDER BY CreatedDate desc LIMIT 1];
			}
			catch(Exception e)
			{
				relatedAsset = [SELECT id, NE__Status__c, CNT_Case__c, Contract__c, Country__c, PointofDelivery__c, ContactId FROM Asset WHERE NE__Order_Config__c = :orderId ORDER BY CreatedDate desc LIMIT 1];
				
			}
			
			 
            System.debug('ASSET RECUPERADO' + relatedAsset);
			relatedAsset.CNT_Case__c = relatedCase.Id;
            relatedAsset.NE__Status__c = 'Active';
            relatedAsset.Contract__c = con.Id;
            relatedAsset.Country__c = relatedCase.Account.Country__c;
            relatedAsset.PointofDelivery__c = pod.Id;
            relatedAsset.ContactId = relatedCase.ContactId;

            // Jonas Junior 5/27/2019 - BUGFIX-10
            //externalIdAsset(relatedAsset, con);
			
			String codeAccount = ('B2B_BRASIL'.equalsIgnoreCase(con.Account.RecordType.DeveloperName) ? 'B2B' : ('B2C_BRASIL'.equalsIgnoreCase(con.Account.RecordType.DeveloperName) ? 'B2C' : ''));

			if (con.Account.CompanyID__c == '2003') {
			  relatedAsset.ExternalId__c = con.ContractNumber + '0113BRACOE' + codeAccount;
			  relatedAsset.Company__c = con.Account.CompanyID__c;
			} else if (con.Account.CompanyID__c == '2005') {
			  relatedAsset.ExternalId__c = con.ContractNumber + '0113BRAAMA' + codeAccount;
			  relatedAsset.Company__c = con.Account.CompanyID__c;
			}

            update relatedAsset;
            System.debug('<<<ASSET OK blaw>>>');
			
			System.debug('oioi2' + relatedAsset);
			Contract_Line_Item__c cli = new Contract_Line_Item__c();
			
			
			try
			{
				cli = [select id From Contract_Line_Item__c where Contract__c =: con.Id and CNT_Product__c like 'Grupo%' limit 1];
				
			}
			catch(Exception ex)
			{
				
			}
			
			cli.asset__c = relatedAsset.Id;
			
			system.debug('<<<CON OK>>>' + con.Id);
			
			system.debug('<<<CLI OK>>>' + cli.Id);
			
			system.debug('<<<ASSE OK>>>' + relatedAsset.Id);
			
			update cli;  
			  
		  }
		  

          //activated the pod
          pod.CNT_Contract__c = con.Id;

          if (lstOrders[0].CNT_Case__r.RecordTypeId == CNT_CaseUtility.getRecordTypeID('BR_case_Alta')) 
		  {
            CNT_ConfigurationUtility.updateAsset(pod, relatedCase, 'BR_case_Alta');
          } 
		  else 
		  {
            /*for (CNT_CustomerAnalysis__c loopCustomAnal :[SELECT Id, CNT_DebtAnalysis__c, CNT_Case__c, CNT_Status__c FROM CNT_CustomerAnalysis__c WHERE CNT_Case__c = :relatedCase.Id ORDER BY CreatedDate asc]) {
              mapCustomAnal.put(loopCustomAnal.CNT_Case__c, loopCustomAnal);
            }*/
			
            CNT_ConfigurationUtility.updateAsset(pod, relatedCase, 'ANY_case_OwnerChange');
            //CNT_ConfigurationUtility.disconnectOldAssetContract(relatedCase);
			
			List<Asset> listOldAsset = new List<Asset> ();
			List<Contract> listOldContract = new List<Contract> ();

			listOldAsset = [SELECT id, Status FROM Asset WHERE PointofDelivery__c = :pod.id and accountid != :relatedCase.Accountid];
			listOldContract = [SELECT id, Status FROM Contract WHERE id = :relatedCase.Asset.Contract__c];
			System.debug('::listOldAsset ' + listOldAsset);
			System.debug('::listOldContract ' + listOldContract);

			for(Asset ass : listOldAsset) 
			{
			  ass.NE__Status__c = 'Disconnected';
			  update ass;
			}
			
			/*if (listOldContract.size() > 0) {
			  listOldContract.get(0).Status = 'Inactivated';
			  update listOldContract;
			}*/
          }
		  
		  

          //update pod;

        }

        system.debug('<<<ORDER OK>>>' + lstOrders[0].Id);
		
		 

      }
	  
	  
    }
	}
	catch(Exception ex)
	{
		System.debug(' Erro: ' +ex);
		continue;
	}
}

if(updateListCase.size() > 0){update updateListCase;}