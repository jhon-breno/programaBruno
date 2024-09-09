String caseNumber = '49967671';

CNT_IntegrationHelper.CCC_BR_242 reqBaja = new CNT_IntegrationHelper.CCC_BR_242();

CNT_Contracting_Status_BO__c backOffice = new CNT_Contracting_Status_BO__c();
Map<String,Object> resultBackOffice = new Map<String,Object>();

Case caso = [select Id, AssetId, PointofDelivery__c, CNT_Potencia__c, CNT_Conexion_Transitoria__c, CNT_Transit_Date_From__c, 
                 CNT_Transit_Date_To__c,  CNT_Con_Transit_PotenciaMax__c, CNT_Con_Transit_PotenciaSimult__c, 
                 CNT_Con_Transit_CosenoPhi__c, CNT_Transit_Potencia_Total__c, Account.Country__c, RecordType.DeveloperName, CNT_Contract__c,
                 CNT_Observation__c, CNT_Hora_Diaria__c, CNT_Total_Dias_Utilizacao__c, CNT_Ramo__c, CNT_CIIU__c, CNT_Public_Ilumination__c,
                 CNT_Change_Type__c, RecordTypeId, CNT_Economical_Activity__c, Asset.NE__Order_Config__c, CNT_Documento_Pagamento__c,
                  CNT_Mandate_Code__c, CNT_Mandate_Amount__c, Contact.Name,Account.Name, Account.Id,   PointofDeliveryAddress__c, PointofDeliveryNumber__c,
                 CNT_Ticket_Expiration_Date__c, Status, CNT_LastInvoiceOptions__c, CreatedDate, PointofDelivery__r.ConnectionStatus__c
                 from Case where casenumber =: caseNumber];


reqBaja = CNT_IntegrationDataBR.flow_CCC_GD_SAP_BR(caso.Id);

System.debug('JSON: '+JSON.serialize(reqBaja));

String Operation = '/Contrato/ContratoFacturacion/CambioCondContr/Update';
        
String JSONbody = CNT_WebServiceHelper.JSONFormat(reqBaja);
        
system.debug('JSON CCC_BR_242>>>'+JSONbody);

HttpResponse response = CNT_WebServiceHelper.makeCallOut(Operation, 'POST', JSONbody);

System.debug('response: '+response);

CNT_ConfigurationUtility.completeOrder([select id from NE__Order__c where CNT_Case__r.id= :caso.id and RecordTypeId = '01236000000yF3XAAU'].id);

 String body = response.getBody();                                                                  
                                        Integer cStart = response.getBody().indexOf('DescripcionResultado') + 24; 
                                        Integer cEnd = response.getBody().indexOf('Header', cStart) - 10;       
                                        String detail = '';                                                                                         
                                        if (cEnd > cStart){                                                                                         
                                            detail = response.getBody().substring(cStart, cEnd);                                                    
                                    }

                                        try
                                        {
                                                    CNT_Contracting_Status_BO__c bo = new CNT_Contracting_Status_BO__c();
                                                    bo.Name = String.valueOf(datetime.now());
                                                    bo.Case__c = caso.Id;
                                                    bo.Date_Time__c = datetime.now();
                                                    

			                                        if (response.getStatusCode() == 200)
			                                        {  

                                                                                                                          
            	                                        bo.Status__c = 'Envio Facturador: OK >> ' + detail;   
                                                        caso.cnt_processStatus__c = '';
                                                        
                                                        caso.status = 'CNT0008';
                                                        caso.cnt_processStatus__c = '';
                                                        //caso.CNT_Documento_Pagamento__c = detail;
                                                        caso.CNT_ByPass__c = true;
                                                        Update caso;
    
                                                    }
                                                    else
                                                    {                                                                                                      
            	                                        bo.Status__c = 'Envio Facturador: ERROR >> ' + detail;
        	                                        }

                                                    bo.CNT_Technical_Information__c = JSON.serialize(reqBaja);
           
                                                    insert bo;
            
            
                                                }catch(system.DmlException ex){
                                                    system.debug('ERROR>>>'+ex);
                                                }
