// Enviar ALTERAÇÃO CONTRATUAL SAP massivamente ate 50 registros
List < String > caseNumberList = new List <String>{{0}};

CNT_IntegrationHelper.BAJA_BR_239 reqAlta = new CNT_IntegrationHelper.BAJA_BR_239();

CNT_Contracting_Status_BO__c backOffice = new CNT_Contracting_Status_BO__c();
Map < String, Object > resultBackOffice = new Map < String, Object > ();
List < CNT_Contracting_Status_BO__c > backOfficeList = new List < CNT_Contracting_Status_BO__c > ();
List < Case > listCaso = new List < Case > ();
List<Case> updateListCase = new List<Case>();

listCaso = [select Id, AssetId, PointofDelivery__c, CNT_Potencia__c, CNT_Conexion_Transitoria__c, CNT_Transit_Date_From__c,
    CNT_Transit_Date_To__c, CNT_Con_Transit_PotenciaMax__c, CNT_Con_Transit_PotenciaSimult__c,
    CNT_Con_Transit_CosenoPhi__c, CNT_Transit_Potencia_Total__c, Account.Country__c, RecordType.DeveloperName, CNT_Contract__c,
    CNT_Observation__c, CNT_Hora_Diaria__c, CNT_Total_Dias_Utilizacao__c, CNT_Ramo__c, CNT_CIIU__c, CNT_Public_Ilumination__c,
    CNT_Change_Type__c, RecordTypeId, CNT_Economical_Activity__c, Asset.NE__Order_Config__c,
    CNT_Mandate_Code__c, CNT_Mandate_Amount__c, Contact.Name, Account.Name, Account.Id, PointofDeliveryAddress__c, PointofDeliveryNumber__c,
    CNT_Ticket_Expiration_Date__c, Status, CNT_LastInvoiceOptions__c, CreatedDate, CNT_Quote__c
    from Case where casenumber IN: caseNumberList];

for (Case caso: listCaso) {

    try 
    {
       reqAlta = CNT_IntegrationDataBR.flow_BAJA_Tecnica_BR(caso.Id);
 reqAlta.Body.retirada_medidor = '1';//c.CNT_TakeDevice__c ? '1' : '0';
        System.debug('JSON: ' + JSON.serialize(reqAlta));
		
        HttpResponse response = CNT_IntegrationHelper.BAJA_239_BRSync(reqAlta, caso.Id);
        System.debug('response: ' + response);

        resultBackOffice = CNT_Utility.resultRequest(response, JSON.serialize(response.getBody()), caso, 'Sistema Tecnico');

        backOffice = (CNT_Contracting_Status_BO__c) resultBackOffice.get('backoffice');
        
        backOffice.CNT_Technical_Information__c = JSON.serialize(reqAlta);
        
        backOfficeList.add(backOffice);
		
		Boolean responseStatusBaixa = (Boolean) resultBackOffice.get('status');
		
		if (responseStatusBaixa)
		{
		}
    } 
    catch (Exception ex) 
    {
        continue;
    }
}

if(updateListCase.size() > 0){ update updateListCase; }

Insert backOfficeList;

