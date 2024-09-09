List<String> caseNumberList = new List<String>{{0}};
//List<String> caseNumberList = new List<String>{'520816630','520816631','520816632','520816633','520816634','520816635','520817328'};

List<Case> ListaCasos = new List<Case>();

List<Case> ListaCasosCompletos = new List<Case>();

List<CNT_Contracting_Status_BO__c> backOfficeList = new List <CNT_Contracting_Status_BO__c> ();

List<Contract> contratos = new List<Contract>();

Map<String,Object> resultBackOffice = new Map<String,Object>();

public List<Id> lstCaseCompleteOrder  = new List<Id>(); 

List<Case> listCaso = new List<Case>();

List<PointofDelivery__c> listPointOfdelivery = new List<PointofDelivery__c>();

List<NE__Order_Item_Attribute__c> OrderUpdated = new List<NE__Order_Item_Attribute__c>();

List<NE__AssetItemAttribute__c> AssetItemUpdated = new List<NE__AssetItemAttribute__c>();

List<NE__Order__c> OrdersList = new List<NE__Order__c>();


public void puxarDado(Case caso) {
    List<NE__Order_Item_Attribute__c> orderAttributes = [SELECT Id, Name, NE__Value__c, NE__Old_Value__c, NE__Action__c
                                                         FROM NE__Order_Item_Attribute__c
                                                         WHERE Name IN ('Classe BR', 'Categoria de Tarifa BR', 'SubClasse BR')
                                                         AND NE__Order_Item__c IN (
                                                             SELECT Id
                                                             FROM NE__OrderItem__c
                                                             WHERE NE__OrderId__r.CNT_Case__c = :caso.Id
                                                         )];

    String tarifaValue = '';
    String classeValue = '';
    String subClasseValue = '';

    for (NE__Order_Item_Attribute__c attribute : orderAttributes) {
        if (attribute.Name == 'Categoria de Tarifa BR') {
            attribute.NE__Old_Value__c = attribute.NE__Value__c;
            attribute.NE__Value__c = 'B1_RESID - Categoria De Tarifa B1 Residencial';
            tarifaValue = attribute.NE__Value__c;
        } else if (attribute.Name == 'Classe BR') {
            classeValue = attribute.NE__Value__c;
        } else if (attribute.Name == 'SubClasse BR') {
            subClasseValue = attribute.NE__Value__c;
        }

        attribute.NE__Action__c = '';
        OrderUpdated.add(attribute);
    }

    List<NE__AssetItemAttribute__c> assetAttributes = [SELECT Id, Name, NE__Value__c, NE__Old_Value__c, NE__Action__c
                                                       FROM NE__AssetItemAttribute__c
                                                       WHERE Name IN ('Classe BR', 'Categoria de Tarifa BR', 'SubClasse BR')
                                                       AND NE__Asset__c = :caso.AssetId];

    for (NE__AssetItemAttribute__c assetAttribute : assetAttributes) {
        if (assetAttribute.Name == 'Categoria de Tarifa BR') {
            assetAttribute.NE__Old_Value__c = assetAttribute.NE__Value__c;
            assetAttribute.NE__Value__c = tarifaValue;
        } else if (assetAttribute.Name == 'Classe BR') {
            assetAttribute.NE__Old_Value__c = assetAttribute.NE__Value__c;
            assetAttribute.NE__Value__c = classeValue;
        } else if (assetAttribute.Name == 'SubClasse BR') {
            assetAttribute.NE__Old_Value__c = assetAttribute.NE__Value__c;
            assetAttribute.NE__Value__c = subClasseValue;
        }

        assetAttribute.NE__Action__c = '';
        AssetItemUpdated.add(assetAttribute);
    }

    NE__Order__c order = new NE__Order__c(Id = caso.CNT_Contract__r.CNT_Quote__c);
    order.NE__OrderStatus__c = 'Completed';
    OrdersList.add(order);

    PointofDelivery__c pointOfDelivery = new PointofDelivery__c(Id = caso.PointofDelivery__c);
    pointOfDelivery.CNT_LowIncomeType__c = caso.CNT_Cliente_Baixa_Renda__c;
    listPointOfdelivery.add(pointOfDelivery);
}


listCaso = [SELECT Id, RecordTypeId, RecordType.DeveloperName, CaseNumber, CNT_Contract__c, CNT_Contract__r.CNT_EndDate__c,CaseRemarks__c,
		CNT_Contract__r.StartDate, CNT_Transit_Potencia_Total__c, AssetId, CNT_Quote__c, CNT_Conexion_Transitoria__c, 
		CNT_Payment_Method__c, CNT_Change_Type__c, PointofDelivery__r.PointofDeliveryNumber__c, CNT_Transit_Date_To__c, 
		SubCauseBR__c, CNT_ProcessStatus__c, CNT_LastInvoiceOptions__c, PointofDelivery__c, CNT_Documento_Pagamento__c, Status, 
		CNT_Controller242Success__c, CNT_Transit_Date_From__c, CNT_Tipo_de_Ajuste__c, PointofDelivery__r.CNT_IsGD__c, CNT_Contract__r.CNT_Quote__c,CNT_Cliente_Baixa_Renda__c
		FROM Case
		WHERE CaseNumber in :caseNumberList AND Asset.NE__Status__c != 'Disconnected' AND RecordType.DeveloperName = 'CNT_ChangeProduct' and status in ('CNT0008')];

for(Case caso : listCaso)
{
	try
	{
			CNT_Contracting_Status_BO__c backOffice = new CNT_Contracting_Status_BO__c();
			CNT_IntegrationHelper.CCC_BR_242 reqBaja = new CNT_IntegrationHelper.CCC_BR_242();
		
			
			reqBaja = CNT_IntegrationDataBR.flow_CCC_SAP_BR(caso.Id);
			
			reqBaja.Body.TARIFTYP = 'B1_RESID';

			System.debug('JSON SAP: '+JSON.serialize(reqBaja));
		
			puxarDado(caso);
		
	}
	catch(Exception ex)
	{
		caso.status ='CNT0009';
		caso.CaseRemarks__c = 'ERRO: ' + ex.getStackTraceString(); 
		ListaCasos.add(caso);		
		continue;
	}
}

		
			try
			{				
				Database.Update(OrderUpdated, false);
				Database.Update(OrdersList, false);
				Database.Update(listPointOfdelivery, false);
				Database.Update(AssetItemUpdated, false);
			}
			catch(Exception ex)
			{
				System.debug('Erro : '+  ex);				
			}


		

