private static ATCL_VFC042_wsdlUtils.EXP_SOAP_SALESFORCE_Soap ws;
//Buscar ORdem com erro

List<WorkOrder> currentWO = [ 
                       SELECT ID, Case.Id,Case.TraceErrorInservice__c
                       FROM WorkOrder 
                        WHERE Order_Number_BE__c = null
                         and case.casenumber in ('{0}')
                         and RecordTypeId = '0121o000000oUAiAAM' 
                         and Case.Reason = 'MOT016'
                         and Case.SubCauseBR__c = 'ATBR017'];
	//enquanto houver dado na lista de ordens

	
for(WorkOrder wo:currentWO)
{
	//Verifica se o caso possui informação de erro
	//if(wo.case.TraceErrorInservice__c.contains('inexperadoFalha') ){
		
		//Buscaoms a informação do caso
		Case currentCase = [SELECT Id, Reason,AccountId, EmployeeNumberOwner__c,Subcause__c, NumberOfOrder__c, PointofDelivery__c, PointofDelivery__r.PointofDeliveryNumber__c, Status,
						   PointofDelivery__r.CompanyID__c, SubStatus__c, Description, ContactId, Contact.AccountId, Country__c, CaseRemarks__c, CaseNumber
						   FROM Case WHERE id =: wo.CaseID]; //Id = '5007Z00000JqLOT'
						   
		Contract_line_item__c CL = [select Asset__r.PointofDelivery__r.CompanyID__c, Asset__r.PointofDelivery__r.SegmentType__c, accountContract__c
									from Contract_Line_Item__c where asset__r.name in ('Eletricity Service','Grupo B', 'Grupo A')
									and  asset__r.NE__Status__c in ('Active' ,'In Progress')
									and  asset__r.PointofDelivery__r.id =: currentCase.PointofDelivery__c limit 1];
		
		//Encontrar field_operation
		List<Field_Operation__c> fieldOpList = [SELECT Id, RecordType.Name
                                                FROM Field_Operation__c WHERE AssociatedCase__c =:wo.case.Id];
        //if(!fieldOpList.isEmpty()){
            Field_Operation__c fieldOp = fieldOpList.get(0);
		//}
		//Chamada WS_FICA
		ATCL_VFC042_wsdlUtils.HeaderRequestType header = ATCL_WS00_Utils.initHeader('postCorteRepoProrroga', ECH_VFC036_GeneralConstants.WS_SYSTEM_CODE_FICA);
				
		ATCL_VFC042_wsdlUtils.BodyPostCorteRepoProrrogaRequest_element body = new ATCL_VFC042_wsdlUtils.BodyPostCorteRepoProrrogaRequest_element();
		 
		header = ATCL_WS00_Utils.initHeaderBRA('postCorteRepoProrroga', ECH_VFC036_GeneralConstants.WS_SYSTEM_CODE_FICA, CL.Asset__r.PointofDelivery__r.CompanyID__c);
		 
		 
		header = ATCL_WS00_Utils.initHeaderBRA('postCorteRepoProrroga', ECH_VFC036_GeneralConstants.WS_SYSTEM_CODE_FICA, CL.Asset__r.PointofDelivery__r.CompanyID__c);
		
		body.NumeroSuministro = CL.AccountContract__c;
		//body.CodigoEmpresa = fieldOp.PointDeliveryId__r.CompanyID__c;
		body.NumeroCaso = currentCase.CaseNumber;
		//System.debug('invokePostCorteRepoProrroga -> body_1 : '+ Pod.PointofDeliveryNumber__c + currentCase.CaseNumber );	
			   
		body.CodigoEmpresa = CL.Asset__r.PointofDelivery__r.CompanyID__c;
					/*
					FORMAT Brazil, Change made to separate the customers between low voltage and high voltage and direct them to their respective SAP instances. 
					*/
					if (body.CodigoEmpresa == '2005' || body.CodigoEmpresa == '2018'){
						header.Funcionalidad += ATCL_WS00_Utils.getGroupBySuministro(CL.Asset__r.PointofDelivery__r.SegmentType__c);
					}

					body.Motivo = 'BR01';
					body.Observaciones =  'Ordem reprocessada por GDS';

					if(fieldOp.RecordType.Name.equals(ECH_VFC036_GeneralConstants.FIELD_OPERATION_RECORD_TYPE_CUTOFF)){
						body.Evento = '01';
					}else if(fieldOp.RecordType.Name.equals(ECH_VFC036_GeneralConstants.FIELD_OPERATION_RECORD_TYPE_REINSTATEMENT)){
						body.Evento = '02';
					}
		//System.debug('invokePostCorteRepoProrroga -> body : '+body);	
		ws = new ATCL_VFC042_wsdlUtils.EXP_SOAP_SALESFORCE_Soap();
					ATCL_VFC042_wsdlUtils.Post_CorteRepoProrrogaResponse_element response = ws.Post_CorteRepoProrroga(header, body);  
					System.debug('invokePostCorteRepoProrroga -> response : '+response);
					
		//Atualizar Caso
		ATCL_WS00_Utils.updateCaseBR(currentCase, 'ESTA003', ECH_VFC036_GeneralConstants.COD_SUBESTADO_SOLICITUD_BO, '', response.BodyPostCorteRepoProrrogaResponse.CodigoEmpresa+''+response.BodyPostCorteRepoProrrogaResponse.DocumentoSuspensao+''+response.BodyPostCorteRepoProrrogaResponse.Evento, response.BodyPostCorteRepoProrrogaResponse.NumeroOrden);


		//Atualizar ordem de serviço
		 ATCL_WS00_Utils.updateWorkOrderCorteRepoBR(wo.case.Id, response.BodyPostCorteRepoProrrogaResponse.NumeroOrden, '0', response.BodyPostCorteRepoProrrogaResponse.CodigoEmpresa, response.BodyPostCorteRepoProrrogaResponse.Evento);
	 
 	//}else{
    //        system.debug('erro : ' + wo.case.TraceErrorInservice__c);
	//	  }
}

//executa criação de ordem de serviço generica
//Case c = [select id from case where id ='50006000057AbPP'];
//ATCL_WS06_PostActuacionGenerica.invokePostActuacionGenerica(c);
