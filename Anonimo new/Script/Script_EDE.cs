    ATCL_VFC042_wsdlUtils.HeaderRequestType header;
    ATCL_VFC042_wsdlUtils.BodyPutPerfilFacturacionRequest_element body = new ATCL_VFC042_wsdlUtils.BodyPutPerfilFacturacionRequest_element();
    ATCL_VFC042_wsdlUtils.EXP_SOAP_SALESFORCE_Soap ws;
    
	
	
	Billing_Profile__c currentBillingPr;
	
	currentBillingPr = [SELECT id,DocumentType__c, AccountContract__c, Type__c,CNT_Due_Date__c, CurrentAccountNumber__c, Agency__c, BankBR__c, BillingAddress__c, EDEEnrolment__c, Main_Email__c,
                                        CardType__c, CardNumber__c, CurrentAccountNum__c , CBU_Arg__c,PointofDelivery__c, PointofDelivery__r.CompanyID__c,
                                        Delivery_Type__c,Contact__c,BallotName__c, IdentityType__c, BankDocumentOwner__c,
                                        PAStreet__c,PANumber__c,PAPostalCode__c,PACity__c,PANeighbourhood__c,PAState__c,PAComplement__c, PointofDelivery__r.SegmentType__c
                                        FROM Billing_Profile__c WHERE Id = '{0}'];
                
				

                Account currentAccount = new Account();
                Contact contactoAso = new Contact();
                List<Contact> contactoAsoList = new List<Contact>();
                
                if(currentBillingPr <> null && currentBillingPr.PointofDelivery__c != null && currentBillingPr.PointofDelivery__r.CompanyID__c != null)
				{ 
                    body.CodigoEmpresa = currentBillingPr.PointofDelivery__r.CompanyID__c;
                }
				                
				
                body.CuentaContrato = currentBillingPr.AccountContract__c;
                

                
                    
                        
                        List<BillingProfileContact__c> listabpc = [SELECT Id,Contacto__c FROM BillingProfileContact__c WHERE Billing_Profile__c =:currentBillingPr.Id ORDER BY CreatedById];
                        system.debug('listabpc '+listabpc);
                                                
						if(listabpc <> null & !listabpc.isEmpty())
						{
                            system.debug('listabpc '+listabpc);
                            Contact cont = [SELECT Id, email FROM Contact WHERE Id =: listabpc.get(0).Contacto__c Limit 1];
                            if(cont <> null && cont.Email <> null){body.Email = cont.Email;}
                        }

                    

                    if(currentBillingPr.EDEEnrolment__c)
					{
                        body.Tipo = '01';
                        body.Motivo = 'MAIL';
                    }
					else
					{
                        body.Tipo = '00';
                        body.Motivo = 'MAIL';
                    }

                    body.Cliente = currentBillingPr.AccountContract__c;//currentAccount.AccountClientNumber__c;
                    body.SubMotivo = Label.ATCL_CauseSAP_DeliveryInvoice;
                    
					body.NumeroCaso = currentBillingPr.AccountContract__c;
                    
					if(currentBillingPr <> null && currentBillingPr.EDEEnrolment__c <> null && currentBillingPr.EDEEnrolment__c == true)
					{
                        body.EmailBilling = 'SI';
                    }
					else
					{ 
						body.EmailBilling = 'NO';  
					} 
					
					
					header = ATCL_WS00_Utils.initHeaderBRA('putPerfilFacturacion', ECH_VFC036_GeneralConstants.WS_SYSTEM_CODE_SAP, body.CodigoEmpresa);
						
                try 
				{
					ws = new ATCL_VFC042_wsdlUtils.EXP_SOAP_SALESFORCE_Soap();
					ATCL_VFC042_wsdlUtils.Put_PerfilFacturacionResponse_element response = ws.Put_PerfilFacturacion(header, body);
					System.debug('invokePutPerfilFacturacionNoChilds response: ' + response);
				} 
				catch (Exception ex) 
				{
					String errorMsg = 'Se ha producido un error. Mensaje: ' + ex.getMessage() + '. Traza: ' + ex.getStackTraceString();
					System.debug(errorMsg);
				}
               