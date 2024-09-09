String Operation = '/Contrato/ContratoFacturacion/Alta/Create';

HttpResponse response = CNT_WebServiceHelper.makeCallOut(Operation, 'POST', '{0}');
                String body = response.getBody();                                                                  
                                        Integer cStart = response.getBody().indexOf('DescripcionResultado') + 24; 
                                        Integer cEnd = response.getBody().indexOf('Header', cStart) - 10;       
                                        String detail = '';                                                                                         
                                        if (cEnd > cStart){                                                                                         
                                            detail = response.getBody().substring(cStart, cEnd);                                                    
                                    }
                                        
                                         //Case c = [Select Id, ExternalID__c, Account.Country__c, SubStatus__c From Case Where Id = '5001o00003cQLgI'];
                                        try
                                        {
                                                    CNT_Contracting_Status_BO__c bo = new CNT_Contracting_Status_BO__c();
                                                    bo.Name = String.valueOf(datetime.now());
                                                    bo.Case__c = '5001o00003cQLgI';
                                                    bo.Date_Time__c = datetime.now();
                                                    

			                                        if (response.getStatusCode() == 200)
			                                        {                                                                       
            	                                        bo.Status__c = 'Envio Facturador: OK >> ' + detail;   
                                                    }
                                                    else
                                                    {                                                                                                      
            	                                        bo.Status__c = 'Envio Facturador: ERROR >> ' + detail;
        	                                        }

                                                    bo.CNT_Technical_Information__c = '{0}';
           
                                                    insert bo;
            
            
                                                }catch(system.DmlException ex){
                                                    system.debug('ERROR>>>'+ex);
                                                }