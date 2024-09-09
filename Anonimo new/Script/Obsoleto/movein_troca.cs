String caseNumber ='{0}';
//String[] caseNumber = x.Split('>');

Case caso = [select Id, AssetId, PointofDelivery__c, CNT_Potencia__c, CNT_Conexion_Transitoria__c, CNT_Transit_Date_From__c, 
                 CNT_Transit_Date_To__c,  CNT_Con_Transit_PotenciaMax__c, CNT_Con_Transit_PotenciaSimult__c, 
                 CNT_Con_Transit_CosenoPhi__c, CNT_Transit_Potencia_Total__c, Account.Country__c, RecordType.DeveloperName, CNT_Contract__c,
                 CNT_Observation__c, CNT_Hora_Diaria__c, CNT_Total_Dias_Utilizacao__c, CNT_Ramo__c, CNT_CIIU__c, CNT_Public_Ilumination__c,
                 CNT_Change_Type__c, RecordTypeId, CNT_Economical_Activity__c, Asset.NE__Order_Config__c, 
                 CNT_Mandate_Code__c, CNT_Mandate_Amount__c, Contact.Name,Account.Name, Account.Id,   PointofDeliveryAddress__c, PointofDeliveryNumber__c,
                 CNT_Ticket_Expiration_Date__c, Status, CNT_LastInvoiceOptions__c,CreatedDate
                 from Case where casenumber =: caseNumber];


CNT_IntegrationHelper.ALTA_BR_242 reqAlta = new CNT_IntegrationHelper.ALTA_BR_242();
reqAlta = CNT_IntegrationDataBR.flow_ALTA_SAP_BR(caso.Id,true, true);
//reqAlta.Body.DATA_VALIDITA = date.ValueOf(caseNumber[1]).addDays(1);
reqAlta.Body.DATA_VALIDITA = date.ValueOf(caso.CreatedDate).addDays(1);
//reqAlta.Body.DATA_VALIDITA = date.ValueOf('2019-07-27').addDays(1);

HttpResponse response = CNT_IntegrationHelper.ALTA_242_BRSync(reqAlta,caso.Id);

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

                                                    bo.CNT_Technical_Information__c = JSON.serialize(reqAlta);
           
                                                    insert bo;
                                                    
                                                    CNT_ConfigurationUtility.completeOrder([select id from NE__Order__c where CNT_Case__r.id= :caso.id and RecordTypeId = '01236000000yF3XAAU'].id);
                                                    
                                                    caso.status = 'CNT0008';
                                                    caso.cnt_processStatus__c = '';
                                                    caso.CNT_ByPass__c = true;
                                                    Update caso;

}catch(system.DmlException ex){
                                                    system.debug('ERROR>>>'+ex);
                                                }