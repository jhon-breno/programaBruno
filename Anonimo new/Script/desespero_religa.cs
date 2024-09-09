private static String sistemaOrigen = 'SFDC';
    private static ATCL_VFC042_wsdlUtils.EXP_SOAP_SALESFORCE_Soap ws;
String recordType = ECH_VFC036_GeneralConstants.FIELD_OPERATION_RECORD_TYPE_REINSTATEMENT;
Case currentCase;

            currentCase= [SELECT Id, Reason, Subject, Status, SubStatus__c, AssocObjectExtId__c,SubCauseBR__c, Contact.Email, Contact.SecondaryEmail__c, AttentionNumber__c,
                            Contact.IdentityNumber__c, NumberOfOrder__c, SubCausePE__c, RecordTypeId, SubCause__c,
                            TraceErrorInservice__c, Type, Type_of_claim__c, Channel_Type__c, InserviceSentError__c,
                            OwnerId, PointofDelivery__c, CaseNumber, Origin ,ContactId, GenericOrder__c,Rolebox__c, CompanyID__c, Country__c, Refund__c,IsContinueOrden__c, PointofDelivery__r.PointofDeliveryNumber__c
                            FROM Case WHERE Id = '5001o00004PNVXX' LIMIT 1];

System.debug('invokePostCorteRepoProrroga -> iniciando servicio invokePostCorteRepoProrroga');
    
        String numeroOrden = '', status = '', subStatus = '', numOrderPE = '', documentoSuspensao = '';
        String recordTypeId = ATCL_VFC005_RecordTypeInfo.get(Field_Operation__c.SObjectType, recordType);
        
		List<Field_Operation__c> fieldOpList = [SELECT Id, Reason__c, Order_Subtype__c,Start_Date__c,End_date__c,Observation__c, EventDate__c, PointDeliveryId__r.PointofDeliveryNumber__c, PointDeliveryId__c,PointDeliveryId__r.CompanyID__c, 
                                                RecordType.DeveloperName, RecordType.Name, DaysRequestedExtension__c, AssociatedCase__r.CaseNumber, OperationType__c, Event__c, ApplyCharges__c, Status__c ,Description__c
                                                FROM Field_Operation__c WHERE AssociatedCase__c =:currentCase.Id AND RecordTypeId =: recordTypeId];
        if(!fieldOpList.isEmpty()){
            Field_Operation__c fieldOp = fieldOpList.get(0);
            
            ATCL_VFC042_wsdlUtils.HeaderRequestType header = new ATCL_VFC042_wsdlUtils.HeaderRequestType();
            header = ATCL_WS00_Utils.initHeader('postCorteRepoProrroga', ECH_VFC036_GeneralConstants.WS_SYSTEM_CODE_SYNERGIA);          
            
            ATCL_VFC042_wsdlUtils.BodyPostCorteRepoProrrogaRequest_element body = new ATCL_VFC042_wsdlUtils.BodyPostCorteRepoProrrogaRequest_element();
            
            body.NumeroSuministro = fieldOp.PointDeliveryId__r.PointofDeliveryNumber__c;
            body.CodigoEmpresa = fieldOp.PointDeliveryId__r.CompanyID__c;
            body.NumeroCaso = currentCase.CaseNumber;
            
            if(ECH_VFC036_GeneralConstants.FIELD_OPERATION_RECORD_TYPE_EXTENSION.equals(recordType)) body.Motivo = ATCL_VFC051_GeneralIntegrationConstants.motivoProrroga.get(fieldOp.Reason__c);
            else body.Motivo = ATCL_VFC051_GeneralIntegrationConstants.motivoCorteRepoPr.get(fieldOp.Reason__c);
			
                header = ATCL_WS00_Utils.initHeaderBRA('postCorteRepoProrroga', ECH_VFC036_GeneralConstants.WS_SYSTEM_CODE_FICA, fieldOp.PointDeliveryId__r.CompanyID__c);

				System.debug('LAIJSIHDIJD' +  header);
                body.Motivo = fieldOp.Reason__c;

                if(recordType.equals(ECH_VFC036_GeneralConstants.FIELD_OPERATION_RECORD_TYPE_CUTOFF)){
                    body.Evento = '01';
                }else if(recordType.equals(ECH_VFC036_GeneralConstants.FIELD_OPERATION_RECORD_TYPE_REINSTATEMENT)){
                    body.Evento = '02';
                }
                if(fieldOp.Start_Date__c!=null) body.FechaInicioDesligamento = fieldOp.Start_Date__c;
                if(fieldOp.End_date__c!=null) body.FechaFinDesligamento = fieldOp.End_date__c;
				
				
            system.debug('HOJE É FESTA ' + body);
          
            
            try {
                ws = new ATCL_VFC042_wsdlUtils.EXP_SOAP_SALESFORCE_Soap();
                ATCL_VFC042_wsdlUtils.Post_CorteRepoProrrogaResponse_element response = ws.Post_CorteRepoProrroga(header, body);  
                System.debug('invokePostCorteRepoProrroga -> response : '+response);
                if(response <> null && response.BodyPostCorteRepoProrrogaResponse.CodigoResultado == '0') {
                    //Hacemos el tratamiento de los datos, revisar numero de orden ya no esta en esa lista , estará dentro de los suministros.
                    if(response.BodyPostCorteRepoProrrogaResponse.NumeroOrden <> null) {
                        subStatus = ECH_VFC036_GeneralConstants.COD_SUBESTADO_SOLICITUD_BO;
                        numeroOrden = response.BodyPostCorteRepoProrrogaResponse.NumeroOrden;
                        if(ECH_VFC106_UtilOneOrg.isValidProfile(new String[]{ECH_VFC036_GeneralConstants.COD_COUNTRY_CHILE})){
                            //MS:  Nicolas dice que al crearse la prorroga en Synergia el caso deberia quedar cerrado (14/06/2019)
                            status = ECH_VFC036_GeneralConstants.COD_ESTADO_CERRADO;
                        }
                    } else {
                        status = ECH_VFC036_GeneralConstants.COD_ESTADO_CERRADO;
                        subStatus = '';
                    }
                    if (ECH_VFC106_UtilOneOrg.isValidProfile(new String[]{ECH_VFC036_GeneralConstants.COD_COUNTRY_BRASIL})){
                        if (response.BodyPostCorteRepoProrrogaResponse.DocumentoSuspensao <> null) {
                            documentoSuspensao = response.BodyPostCorteRepoProrrogaResponse.DocumentoSuspensao;
                            ATCL_WS00_Utils.updateWorkOrderCorteRepoBR(currentCase.Id, documentoSuspensao, status, response.BodyPostCorteRepoProrrogaResponse.CodigoEmpresa, response.BodyPostCorteRepoProrrogaResponse.Evento);
                            //aj - 12/11/2019 añadir el numero de orden
                            ATCL_WS00_Utils.updateCaseBR(currentCase, 'ESTA003', subStatus, '', response.BodyPostCorteRepoProrrogaResponse.CodigoEmpresa+''+documentoSuspensao+''+response.BodyPostCorteRepoProrrogaResponse.Evento, numeroOrden);
                        }
                    }       
                }else {
                    System.debug ( 'FAIL' + response.BodyPostCorteRepoProrrogaResponse.DescripcionResultado + 'code: ' + response.BodyPostCorteRepoProrrogaResponse.CodigoResultado );
                    ATCL_WS00_Utils.updateCase(currentCase, ECH_VFC036_GeneralConstants.COD_ESTADO_CERRADO, '', response.BodyPostCorteRepoProrrogaResponse.DescripcionResultado, '');
                    //EHG 23/09/2019 Actualizar el FO a cancelado
                    if (ECH_VFC106_UtilOneOrg.isValidProfile(new String[]{ECH_VFC036_GeneralConstants.COD_COUNTRY_BRASIL})){
                        fieldOp.Status__c = 'Cancelled';
                        update fieldOp;
                    }
                }
            }catch(Exception ex) {
                String errorMsg = 'Se ha producido un error. Mensaje: ' + ex.getMessage() + '. Traza: ' + ex.getStackTraceString();
                System.debug(errorMsg);
                //EHG 23/09/2019 Actualizar el caso a cancelado y FO
                if (ECH_VFC106_UtilOneOrg.isValidProfile(new String[]{ECH_VFC036_GeneralConstants.COD_COUNTRY_BRASIL})){
                    ATCL_WS00_Utils.updateCase(currentCase, ECH_VFC036_GeneralConstants.COD_ESTADO_CERRADO, ECH_VFC036_GeneralConstants.COD_SUBESTADO_COMUNICACION_INTERRUMPIDA, '', '');
                    
                    fieldOp.Status__c = 'Cancelled';
                    update fieldOp;
                }else{
                    ATCL_WS00_Utils.updateCase(currentCase, '', ECH_VFC036_GeneralConstants.COD_SUBESTADO_COMUNICACION_INTERRUMPIDA, errorMsg, '');
                }
                
            }
        }