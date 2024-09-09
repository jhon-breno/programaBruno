CNT_IntegrationHelper.CT_BR_239 reqAlta = new CNT_IntegrationHelper.CT_BR_239();

CNT_Contracting_Status_BO__c backOffice = new CNT_Contracting_Status_BO__c();
Map <String, Object> resultBackOffice = new Map <String, Object> ();
List <CNT_Contracting_Status_BO__c> backOfficeList = new List <CNT_Contracting_Status_BO__c> ();
List <Case> listCaso = new List <Case> ();
List<Case> updateListCaseSAP = new List<Case>();

String dados = '3701012;02558157001134;2003;enel@hotmail.com';

String pointOfDelivery = dados.Split(';')[0];
String conta = dados.Split(';')[1];
String empresa = dados.Split(';')[2];
String emailCadastro = dados.Split(';')[3];

List<Asset> listAsset = [SELECT Id, PointofDelivery__c, PointofDelivery__r.Name, PointofDelivery__r.DetailAddress__c, PointofDelivery__r.CNT_IsGD__c, Contract__c, Contract__r.CNT_Generation_Capability__c, NE__Order_Config__c, NE__Status__c, AccountId, Account.Name, Account.IdentityNumber__c, ContactId, Contact.Name
						FROM Asset
						WHERE Name in ('Grupo B', 'Grupo A', 'Eletricity Service')
						and PointofDelivery__r.CompanyID__c = :empresa
						and PointofDelivery__c in (SELECT PointofDelivery__c FROM Device__c WHERE Status__c = 'I') AND PointOfDelivery__r.Name = :pointOfDelivery ORDER BY CreatedDate DESC LIMIT 1];						

List<Contact> listContact = [SELECT Id, AccountId, Account.Name, Account.IdentityNumber__c, Account.ExternalID__c
						FROM Contact
						WHERE Account.IdentityNumber__c = :conta
						and Email = :emailCadastro
						AND Account.CompanyID__c = :empresa ORDER BY CreatedDate DESC LIMIT 1];

List<NE__Order__c> newOrder = new List<NE__Order__c>();
for (Asset asset : listAsset) {
	NE__Order__c order = new NE__Order__c();

	if (!listContact.isEmpty()) {
        Id accountId = listContact[0].AccountId;
        order.NE__AccountId__c = accountId;
		order.NE__BillAccId__c = accountId;
		order.NE__ServAccId__c = accountId;
    }

	order.NE__Description__c = asset.PointofDelivery__r.Name;
	order.CurrencyIsoCode = 'BRL';
	order.RecordTypeId = '01236000000yF3XAAU';
	order.NE__CatalogId__c = 'a101o00000EBAXWAA5';
	order.NE__CommercialModelId__c = 'a141o00000GhwG3AAJ';
	order.NE__ConfigurationStatus__c = 'Valid';
	order.NE__OrderStatus__c = 'Pending';
	order.Company__c = 'COELCE';
	order.Country__c = 'BRASIL';
	order.CNT_ContractStatus__c = 'Confirmed';

	newOrder.add(order);
}

if (!newOrder.isEmpty()){
	insert newOrder;
}

List<NE__OrderItem__c> newOrderItem = new List<NE__OrderItem__c>();


for(Asset asset : listAsset){
	NE__OrderItem__c orderItem = new NE__OrderItem__c();
		
	if (!listContact.isEmpty()) {
        Id accountId = listContact[0].AccountId;
		orderItem.NE__Account__c = accountId;
		orderItem.NE__Billing_Account_Asset_Item__c = accountId;
		orderItem.NE__Service_Account__c = accountId;
    }

	if (!newOrder.isEmpty()){
		Id orderId = newOrder[0].Id;
		orderItem.NE__OrderId__c = orderId;
	}
	orderItem.NE__Description__c = asset.PointofDelivery__r.Name; 
	orderItem.CurrencyIsoCode = 'BRL';
	orderItem.NE__Action__c = 'Add';
	orderItem.NE__BaseOneTimeFee__c = 0;
	orderItem.NE__BaseRecurringCharge__c = 0;
	orderItem.NE__CatalogItem__c = 'a0z1o000003z2FOAAY';
	orderItem.NE__Catalog__c = 'a101o00000EBAXWAA5';
	orderItem.NE__Commitment__c = false;
	orderItem.NE__Country__c = 'BRASIL';
	orderItem.NE__Discount_One_time__c = 0;
	orderItem.NE__Discount__c = 0;
	orderItem.NE__Generate_Asset_Item__c = true;
	orderItem.NE__Hidden_in_Cart__c = false;
	orderItem.NE__IsPromo__c = false;
	orderItem.NE__OneTimeFeeOv__c = 0;
	orderItem.NE__One_Time_Cost__c = 0;
	orderItem.NE__Optional__c = false;
	orderItem.NE__Penalty_Activated__c = false;
	orderItem.NE__Penalty__c = false;
	orderItem.NE__ProdId__c = 'a1f1o00000bsF6tAAE';
	orderItem.NE__Qty__c = 1;
	orderItem.NE__RecurringChargeFrequency__c = 'Monthly';
	orderItem.NE__RecurringChargeOv__c = 0;
	orderItem.NE__Recurring_Cost__c = 0;
	orderItem.NE__Remove_from_total__c = false;
	orderItem.NE__Status__c = 'Pending';
	orderItem.NE__OneTimeFeeOVApplied__c = false;
	orderItem.NE__RecurringChargeOVApplied__c = false;
	orderItem.NE__configured__c = false;
	
	newOrderItem.add(orderItem);

	
}

if (!newOrderItem.isEmpty()){
	insert newOrderItem;
}

List<NE__Order_Item_Attribute__c> newOrderItemAttributes = new List<NE__Order_Item_Attribute__c>();
for(Asset asset : listAsset){
	if (!newOrder.isEmpty()){
		Id orderId = newOrder[0].Id;
		for (NE__Order_Item_Attribute__c item : [select Name, CurrencyIsoCode, NE__Order_Item__c, NE__Action__c, NE__AttrEnterpriseIdCalc__c, NE__AttrEnterpriseId__c, NE__FamPropExtId__c, NE__FamPropId__c, NE__Old_Value__c, NE__Previous_Attribute_Value__c, NE__Value__c, CNT_GreenBlue__c, CNT_SubCat__c from NE__Order_Item_Attribute__c where NE__Order_Item__c  in (select id from NE__OrderItem__c where NE__OrderId__c = :asset.NE__Order_Config__c)])
		{
			if(!newOrderItem.isEmpty()){
				NE__Order_Item_Attribute__c newItem = new NE__Order_Item_Attribute__c();
				Id orderItem = newOrderItem[0].Id;
				newItem.NE__Order_Item__c = orderItem;	
				newItem.NE__Action__c = 'Add';
				newItem.Name = item.Name;
				newItem.NE__Value__c = item.NE__Value__c;

				newOrderItemAttributes.Add(newItem);
			}
			
		}
	}
}

Database.insert(newOrderItemAttributes, true);

List<Contract> newContracts = new List<Contract>();
for(Asset asset : listAsset){
	Contract contrato = new Contract();
		
	if (!listContact.isEmpty()) {
        Id accountId = listContact[0].AccountId;
		contrato.AccountId = accountId;
		Id contatoId = listContact[0].Id;
		contrato.CustomerSignedId = contatoId;
    }
	
	contrato.Status = 'Draft';
	contrato.CNT_Economical_Activity__c = 'J6110801-Serviços de telefonia fixa comutada - STFC';
	contrato.CNT_Numerary_Type__c = '3 Cliente Nuevo';
	for (NE__Order__c order : newOrder) {
		contrato.CNT_Quote__c = order.Id;
		break;
	}
	contrato.Company_ID__c = 'COELCE';
	contrato.CurrencyIsoCode = 'BRL';
	contrato.StartDate = System.today();
	contrato.CNT_Owner_Type_DTT__c = 'FATURA_DIGITAL';

	if (asset.PointofDelivery__r.CNT_IsGD__c == '1') {
		contrato.CNT_Distributed_Generation_Eval__c = true;
		contrato.CNT_Generation_Capability__c = asset.Contract__r.CNT_Generation_Capability__c;
		contrato.CNT_Generation_Sources__c = 'Solar,';
		contrato.CNT_Generation_Type__c = 'Consumo_Local';		
	}
	
	newContracts.add(contrato);
}

if (!newContracts.isEmpty()) {
	insert newContracts;
}

List<Case> newCases = new List<Case>();

for(Asset asset : listAsset){
	Case caso = new Case();
		
	if (!listContact.isEmpty()) {
        Id accountId = listContact[0].AccountId;
		caso.AccountId = accountId;
		Id contatoId = listContact[0].Id;
		caso.ContactId = contatoId;
		caso.Description = 'INGRESSO MASSIVO TROCA TITULAR DA CONTA ' + asset.Account.Name  + ' DOC: ' + asset.Account.IdentityNumber__c + ' PARA A CONTA ' + listContact[0].Account.Name + ' DOC: ' + listContact[0].Account.IdentityNumber__c;
    }

	caso.Address__c = asset.PointofDelivery__r.DetailAddress__c;
	caso.PointofDelivery__c = asset.PointofDelivery__c;
	caso.AssetId = asset.Id;
    caso.Status = 'CNT0002';
	caso.CNT_ByPass__c = true;
	caso.CNT_LastInvoiceOptions__c = '7';
	caso.CNT_Potencia__c = '74';
	caso.Country__c = 'BRASIL';
	caso.FlagControlPreingresados__c = true;
	caso.Origin = '8';
	caso.Reason = 'MOT016';
	caso.SubCauseBR__c = '15';
	caso.SubStatus__c = 'CC';
	caso.Type = 'TIP003';
	caso.CNT_IsPossible_Consulting_BR__c = 'NAO';
	//caso.CNT_Description_RF_BR__c = 'INGRESSO MASSIVO';
	caso.RecordTypeId = '0121o000000oWolAAE';
	caso.CNT_Economical_Activity__c = 'Z0101000-Residencial Pleno';

	for (Contract contrato : [select Id, AccountId,CNT_Quote__c, ContractNumber from Contract WHERE Id = :newContracts[0].Id]) {
		if (contrato.AccountId == caso.AccountId){
			caso.CNT_Contract__c = contrato.Id;
			caso.AccountContract__c = contrato.ContractNumber;
			caso.CNT_Quote__c = contrato.CNT_Quote__c;
			break;
		}
	}
	
	newCases.add(caso);
}

if (!newCases.isEmpty()) {
	insert newCases;
}

Contract cntUpdate = new Contract();
System.debug('Id do caso: ' + newCases[0].Id);
cntUpdate.Id = newContracts[0].Id;
cntUpdate.CNT_Case__c = newCases[0].Id;
cntUpdate.Status = 'Activated';

NE__Order__c orderToUpdate = newOrder[0];
orderToUpdate.CNT_Case__c = newCases[0].Id;


update cntUpdate;
update orderToUpdate;

List<Billing_Profile__c> newBillingProfile = new List<Billing_Profile__c>();

for(Asset asset : listAsset){
	Billing_Profile__c bp = new Billing_Profile__c();

	if (!listContact.isEmpty()) {
        Id accountId = listContact[0].AccountId;
		bp.Account__c = accountId;
		Id contatoId = listContact[0].Id;
		bp.BallotName__c = contatoId;
		String externalId = listContact[0].Account.ExternalID__c;
		bp.PointofDelivery__c = asset.PointofDelivery__c;
		bp.Address__c = asset.PointofDelivery__r.DetailAddress__c;
		bp.Delivery_Type__c = 'N';
		bp.CurrencyIsoCode = 'BRL';
		bp.Type__c = 'Statement';
		bp.DocumentType__c = 'Factura';
		bp.CNT_Due_Date__c = 'ZFAT';
		bp.EDEEnrolment__c = true;

		for (Contract contrato : [select Id, AccountId, ContractNumber from Contract WHERE Id = :newContracts[0].Id]) {
			if (contrato.AccountId == bp.Account__c){
				System.debug('Contract ID: ' + contrato.Id);
				System.debug('Contrato ' + contrato.ContractNumber);
				bp.ExternalID__c = externalId + contrato.ContractNumber;
				bp.AccountContract__c = contrato.ContractNumber;
				break;
			}
		}
    }
	newBillingProfile.add(bp);
}
if(!newBillingProfile.isEmpty()){
	insert newBillingProfile;
}

Contract contractToline = [select Id, AccountId, ContractNumber from Contract WHERE Id = :newContracts[0].Id];
Contract_Line_Item__c contractLine = new Contract_Line_Item__c();
contractLine.AccountContract__c = contractToline.ContractNumber;
contractLine.Contract__c = contractToline.Id;
contractLine.Billing_Profile__c = newBillingProfile[0].Id;
contractLine.Company__c = empresa;
contractLine.CNT_ConfigurationItem__c = newOrderItem[0].Id;

insert contractLine;

BillingProfileContact__c bpc = new BillingProfileContact__c();
bpc.Billing_Profile__c = newBillingProfile[0].Id;
bpc.CurrencyIsoCode = 'BRL';
bpc.Name = listContact[0].Account.Name;
bpc.Contacto__c = listContact[0].Id;
bpc.PointofDelivery__c = listAsset[0].PointofDelivery__c;
bpc.ExternalID__c = listContact[0].Account.ExternalID__c + contractToline.ContractNumber;
bpc.RecordTypeID = '0121o000000shGeAAI';

insert bpc;

reqAlta = CNT_IntegrationDataBR.flow_CT_Tecnica_BR(newCases[0].Id, contractToline.ContractNumber);
System.debug('JSON: ' + JSON.serialize(reqAlta));
        
HttpResponse response = CNT_IntegrationHelper.CT_239_BRSync(reqAlta, newCases[0].Id);
System.debug('response: ' + response);

resultBackOffice = CNT_Utility.resultRequest(response, JSON.serialize(response.getBody()), newCases[0], 'Sistema Tecnico');

backOffice = (CNT_Contracting_Status_BO__c) resultBackOffice.get('backoffice');
       
backOffice.CNT_Technical_Information__c = JSON.serialize(reqAlta);
        
backOfficeList.add(backOffice);
        
Boolean responseStatusBaixa = (Boolean) resultBackOffice.get('status');

System.debug('Inserted Back Office Details: ' + backOffice);

if (responseStatusBaixa) 
{
	CNT_IntegrationHelper.BAJA_BR_242 reqBaja = new CNT_IntegrationHelper.BAJA_BR_242();
                reqBaja = CNT_IntegrationDataBR.flow_BAJA_SAP_BR(newCases[0].Id,newCases[0].CNT_LastInvoiceOptions__c);
              
                CNT_VFC061_Suministro.ResultButton rfact =  CNT_VFC061_Suministro.getSuministro(newCases[0].PointofDelivery__c);
                Boolean resultRequestFlag = (rfact.statusResult == 'FATURAMENTO' ? true : false);
                System.debug(':: FATURAMENTO >>> '+ rfact.statusResult);
                
				if(resultRequestFlag){
                    newCases[0].CNT_Controller242Success__c = 'PF';
                    newCases[0].CNT_ProcessStatus__c = 'CNT004';
                    newCases[0].CNT_LastInvoiceOptions__c = '7';
                    updateListCaseSAP.add(newCases[0]);

                    backOffice = CNT_Utility.registerResult(rfact.backOfficeResult, '', newCases[0])[0];
					//backOffice = (CNT_Contracting_Status_BO__c) resultBackOfficeSAP.get('backoffice');
                    backOfficeList.add(backOffice);
                } else {
                    HttpResponse responseBaja = CNT_IntegrationHelper.BAJA_242_BRSync(reqBaja,newCases[0].Id);
                    System.debug('response: '+responseBaja);

                    resultBackOffice = CNT_Utility.resultRequest(responseBaja, JSON.serialize(reqBaja), newCases[0], 'Facturador');
                    backOffice = (CNT_Contracting_Status_BO__c) resultBackOffice.get('backoffice');
                    backOfficeList.add(backOffice);
                    Boolean responseStatus = (Boolean) resultBackOffice.get('status');
                    String msgReturnBaja = resultBackOffice.containsKey('message') ? (String) resultBackOffice.get('message') : '';
                    
					if(responseStatus || msgReturnBaja.containsIgnoreCase('diferente da conta contrato')){
                        CNT_IntegrationHelper.ALTA_BR_242 reqAlta = new CNT_IntegrationHelper.ALTA_BR_242();
                        reqAlta = CNT_IntegrationDataBR.flow_ALTA_SAP_BR(newCases[0].Id,true, true);
                       
                        HttpResponse responseAlta = CNT_IntegrationHelper.ALTA_242_BRSync(reqAlta,newCases[0].Id);
                        resultBackOffice = CNT_Utility.resultRequest(responseAlta, JSON.serialize(reqAlta), newCases[0], 'Facturador');

                        backOffice = (CNT_Contracting_Status_BO__c) resultBackOffice.get('backoffice');
                        backOfficeList.add(backOffice);

                        Boolean responseStatusALTA = (Boolean) resultBackOffice.get('status');
                        if(responseStatusALTA){
                            
                            if(String.isNotBlank(newCases[0].CNT_Contract__r.CNT_Owner_Type_DTT__c) && newCases[0].CNT_Contract__r.CNT_Owner_Type_DTT__c.equals('FATURA_DIGITAL')){
                                CNT_VFC061_Suministro.putPerfilFacturacion_BPC_BR(newCases[0].Id);
                            }
                            
                            newCases[0].CNT_ProcessStatus__c = ''; // Remove da fila
                            newCases[0].Status = 'CNT0008';
                            updateListCaseSAP.add(newCases[0]);
                            CNT_ConfigurationUtility.completeOrderFlow(new List<Id>{newCases[0].Id});
                        }
                        
                    }
                }
}

update updateListCaseSAP;
insert backOfficeList;