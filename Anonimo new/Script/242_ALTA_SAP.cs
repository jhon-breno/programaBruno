List<CNT_Contracting_Status_BO__c> backOfficeList = new List<CNT_Contracting_Status_BO__c>();
CNT_Contracting_Status_BO__c backOffice = new CNT_Contracting_Status_BO__c();
Map<String, Object> resultBackOffice = new Map<String, Object>();
Boolean verificarEnvio = true;

// Busca o caso com os critérios especificados
Case caso = [
    SELECT Id, RecordTypeId, RecordType.DeveloperName, CaseNumber, CNT_Contract__c, CNT_Contract__r.CNT_EndDate__c,
           CNT_Contract__r.StartDate, CNT_Transit_Potencia_Total__c, AssetId, CNT_Quote__c, CNT_Conexion_Transitoria__c, 
           CNT_Payment_Method__c, CNT_Change_Type__c, PointofDelivery__r.PointofDeliveryNumber__c, CNT_Transit_Date_To__c, 
           SubCauseBR__c, CNT_ProcessStatus__c, CNT_LastInvoiceOptions__c, PointofDelivery__c, CNT_Documento_Pagamento__c, Status, 
           CNT_Controller242Success__c, CNT_Transit_Date_From__c, CNT_Tipo_de_Ajuste__c, CNT_Contract__r.CNT_Owner_Type_DTT__c
    FROM Case
    WHERE CaseNumber in ('{0}') AND RecordType.DeveloperName = 'CNT_BR_Alta_Contrato'
];

//Verifica se já existe DEFINITIVE OU INTERIM
if(verificarEnvio == true){
	List<CNT_Contracting_Status_BO__c> defitiveBO = [SELECT Id, Case__c, Status__c, Case__r.CaseNumber FROM CNT_Contracting_Status_BO__c WHERE (Status__c like '%DEFINITIVE%' OR Status__c like '%INTERIM%') AND Case__c =: caso.Id];
	
	if (!defitiveBO.isEmpty()){
		caso.Status = 'CNT0008';
		caso.cnt_processStatus__c = '';
		caso.CNT_ByPass__c = true;
		update caso;
		return;
	}
}

// Chama a integração para obter os dados de alta SAP
CNT_IntegrationHelper.ALTA_BR_242 altbr239 = CNT_IntegrationDataBR.flow_ALTA_SAP_BR(caso.Id, false, false);
System.debug('JSON altbr239: ' + System.JSON.serialize(altbr239)); 

// Realiza a sincronização com a resposta do serviço
HttpResponse response = CNT_IntegrationHelper.ALTA_242_BRSync(altbr239, caso.Id);
System.debug('response: ' + response);

// Processa a resposta da integração
resultBackOffice = CNT_Utility.resultRequest(response, JSON.serialize(altbr239), caso, 'Facturador');

Boolean responseStatusSAP = ((Boolean) resultBackOffice.get('status'));

if(responseStatusSAP)
{
	caso.Status = 'CNT0008';
	caso.cnt_processStatus__c = '';
	caso.CNT_ByPass__c = true;
	update caso;

	if(String.isNotBlank(caso.CNT_Contract__r.CNT_Owner_Type_DTT__c) && caso.CNT_Contract__r.CNT_Owner_Type_DTT__c.equals('FATURA_DIGITAL'))
	{
		CNT_VFC061_Suministro.putPerfilFacturacion_BPC_BR(caso.Id);
	}
	if (caso.SubCauseBR__c == '48'){
		System.debug('Enviei GD: || ' + caso.SubCauseBR__c);
		CNT_IntegrationHelper.CCC_BR_242 reqCCC = new CNT_IntegrationHelper.CCC_BR_242();
		reqCCC = CNT_IntegrationDataBR.flow_CCC_GD_SAP_BR(caso.Id);
	}
}

// Preenche o objeto backOffice com os dados resultantes
backOffice = (CNT_Contracting_Status_BO__c) resultBackOffice.get('backoffice');
backOffice.CNT_Technical_Information__c = JSON.serialize(altbr239);

// Adiciona o objeto à lista para inserção
backOfficeList.add(backOffice);

// Insere a lista de registros
insert backOfficeList;


if (backOfficeList[0].Status__c.contains('Indisp')) {
    caso.CNT_ProcessStatus__c = 'CNT002';
    update caso;
} else if (backOfficeList[0].Status__c.contains('Cliente não implantado no Synergia')){
	caso.CNT_ProcessStatus__c = '';
    update caso;
}