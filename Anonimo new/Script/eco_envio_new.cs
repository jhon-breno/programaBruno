
	createOrder('{0}','M02');
    
    public void createOrder( String wk, String ServiceCode){
		String JSON_res;
        system.debug('ENTROU EM CREATE ORDER BRASIL');
        WSEecoCreateOrderBrazil.Request require = new WSEecoCreateOrderBrazil.Request();
        WSEecoCreateOrderBrazil.Header header =  new WSEecoCreateOrderBrazil.Header();
        WSEecoCreateOrderBrazil.Body body =  new WSEecoCreateOrderBrazil.Body();
        require.Header = header;
        require.Body = body;  
        
        body.flowCode = ECH_VFC036_GeneralConstants.ECO_flowCodeCreate;
        body.serviceCode = ServiceCode;
        header.SistemaOrigen=ECH_VFC036_GeneralConstants.ECO_SistemaOrigen;
        header.Funcionalidad=ECH_VFC036_GeneralConstants.ECO_FuncionalidadCreate;
        header.CodSistema='BR';
        header.IdPeticion='';
        header.FechaHora=System.now();
        
        WorkOrder  workOrder = [SELECT Id , WorkOrderNumber,PointOfDelivery__c,ANSDaysCount__c,NumberofPointofDelivery__c,OwnerId,CreatedDate,AreaWA__c,ReasonWAT__c,CreatedBy.Name,Status,CNT_TechnicalUser__c,
                                OrderAggravatingCondition__c,AggravatingCondition__c,CNT_DataComunicado__c, ManagementWA__c,DueDate__c,NumberOfPointOfDelivery__r.CompanyId__c,
                                FieldReading__c,WOFailureHourDate__c,Subject,Description,Observations__c , CNT_FlowStatus__c,case.Account.CNT_Client_Type__c,
                                ActivedaytimeReading__c,ReActiveDaytimeReading__c,CustomerPresence__C,Case.BlockType__c,Case.CaseNumber,Case.Origin, Case.ReProgramateNumber__c,
                                ToLabel(Case.Unity__c),Case.fileNumber__c,Case.Reason,Case.SubCauseBR__c,Case.Type_of_claim__c,Case.Type,Case.Address__r.Name, Case.Address__r.Reference__c, Case.Createddate,
                                case.Account.Name,case.Account.FathersLastName__c,case.Account.IdentityType__c,case.Account.IdentityNumber__c,case.Account.PrimaryEmail__c,case.Account.MainPhone__c,case.Account.Id,case.Account.Company__c,
                                Contact.FullName__c,Contact.LastName,Contact.IdentityType__c,Contact.IdentityNumber__c,Contact.Company__c,Order_Number_BE__c,
                                Contact.Email,Contact.MobilePhone,Contact.Phone,Case.ParentId,Case.PointofDelivery__r.CompanyID__c,Case.PointofDelivery__r.ReadingType__c, Contact.FirstName,
                                Case.PointofDelivery__r.ReadingProcess__c , Case.Parent.CaseNumber, DetailAddress__c, Case.subject, Case.Description, Case.Observations__c,
                                Case.CaseRemarks__c
                                FROM WorkOrder 
                                WHERE Case.casenumber=:wk and Case.PointofDelivery__r.SegmentType__c='B' limit 1];  
        system.debug('workOrder'+workOrder);
        system.debug(' :: PointOfDelivery::: '+workOrder.PointOfDelivery__c);

		wk = workOrder.WorkOrderNumber;
        String pointOfDeliveryCase;	
	
        if(workOrder != null){
            system.debug(' Ingreso a if ');
            pointOfDeliveryCase = workOrder.NumberofPointofDelivery__c;
        }
       
        PointofDelivery__c PointofDelivery = new PointofDelivery__c();
        system.debug(' :: pointOfDeliveryCase :: ' + pointOfDeliveryCase);
        if(!String.isBlank(pointOfDeliveryCase) && pointOfDeliveryCase != null){
            
            
           List<Contract_Line_Item__c> cliAsset = [select Asset__r.PointofDelivery__r.Name , Asset__r.PointofDelivery__r.PointofDeliveryAddress__c, Asset__r.PointofDelivery__r.DetailAddress__r.StreetMD__r.Municipality__c,
		   Asset__r.PointofDelivery__r.DetailAddress__r.StreetMD__r.Region__c,Asset__r.PointofDelivery__r.SegmentType__c,Asset__r.PointofDelivery__r.DetailAddress__r.StreetMD__r.NeighbourhoodCode__c ,
		   Asset__r.PointofDelivery__r.DetailAddress__r.StreetMD__r.LocationCode__c ,Asset__r.PointofDelivery__r.DetailAddress__r.Reference__c, contract__r.CNT_ExternalContract_ID__c 
							   from Contract_Line_Item__c where  
                        Asset__r.PointofDelivery__c =: workOrder.NumberOfPointOfDelivery__c 
                        AND Asset__r.accountid = : workOrder.case.account.id AND 
                        (Asset__r.Name = 'Eletricity Service' OR Asset__r.Name = 'Grupo B') limit 1];
            
            
			if(cliAsset.size()> 0){
                PointofDelivery = cliAsset[0].Asset__r.PointofDelivery__r;
				PointOfDelivery.CNT_Contract__r = cliAsset[0].contract__r;
                
            }
			else{
				system.debug('Não tem CL');
				return;
            }				
		
            system.debug('PointofDelivery'+PointofDelivery);   
            system.debug('CNT_Contract__r.CNT_ExternalContract_ID__c'+PointofDelivery.CNT_Contract__r.CNT_ExternalContract_ID__c);                    
            
            WSEecoCreateOrderBrazil.damageDetails damageDetails = new WSEecoCreateOrderBrazil.damageDetails();
            damageDetails.responsabilidadCivil = workOrder.CNT_FlowStatus__c;
            
            WSEecoCreateOrderBrazil.requestIdentifier requestIdentifier = new WSEecoCreateOrderBrazil.requestIdentifier();
            requestIdentifier.sfdcMarketOrderNumber = workOrder.WorkOrderNumber;
            requestIdentifier.traderCompanyCode = ''; 
            requestIdentifier.sfdcSynergiaOrderNumber = WorkOrder.Order_Number_BE__c;
            requestIdentifier.supplierEnergyCompany = workOrder.Case.PointofDelivery__r.CompanyID__c;
            
            WSEecoCreateOrderBrazil.technicalData technicalData = new WSEecoCreateOrderBrazil.technicalData();
           // technicalData.readingType = workOrder.Case.PointofDelivery__r.ReadingType__c;  
            technicalData.readingType = '1';          
            technicalData.podNumber= workOrder.PointOfDelivery__c;
            technicalData.measureType='ACFP';

            system.debug('PointofDelivery'+PointofDelivery);   
            system.debug('CNT_Contract__r.CNT_ExternalContract_ID__c'+PointofDelivery.CNT_Contract__r.CNT_ExternalContract_ID__c); 
            
            WSEecoCreateOrderBrazil.duenoDetails duenoDetails = new WSEecoCreateOrderBrazil.duenoDetails();
            duenoDetails.ownerName = workOrder.Case.Account.Name;
            duenoDetails.ownerLastname = '.';
            duenoDetails.ownerIdentificationType = '5';                        
            duenoDetails.CNT_ExternalContract_ID = PointofDelivery.CNT_Contract__r.CNT_ExternalContract_ID__c;
            duenoDetails.ownerId = workOrder.Case.Account.IdentityNumber__c;
            duenoDetails.ownerCompanyName = workOrder.Case.Account.Name;
            duenoDetails.ownerEmail = workOrder.Case.Account.PrimaryEmail__c;
            duenoDetails.ownerPhone = workOrder.Case.Account.MainPhone__c;
            if (duenoDetails.ownerPhone  == null || duenoDetails.ownerPhone.isAlpha()) {
                duenoDetails.ownerPhone  = '999999999';
            }          
            //validations
            if(duenoDetails.ownerCompanyName == null){
                duenoDetails.ownerName = workOrder.Case.Account.Name;
                duenoDetails.ownerLastname = '.';
            }

            if(duenoDetails.ownerName == null && duenoDetails.ownerLastname == null){
                duenoDetails.ownerCompanyName  = 'XXXXXXXXXX';
            }


            if (PointofDelivery.SegmentType__c == 'A') {
                duenoDetails.CNT_Client_Type = '1';
            }else if (PointofDelivery.SegmentType__c == 'B'){ 
                duenoDetails.CNT_Client_Type = '2';
            }           
            
            if (duenoDetails.CNT_ExternalContract_ID == null) {
                duenoDetails.CNT_ExternalContract_ID = '000000';
            }
			
			workOrder.Account = workOrder.case.account;
			
            WSEecoCreateOrderBrazil.solecitanteDetails solecitanteDetails = new WSEecoCreateOrderBrazil.solecitanteDetails();
            solecitanteDetails.solecitanteName = (String.isNotBlank(workOrder.Contact.FirstName )) ? workOrder.Contact.FirstName  : 'Sem Primeiro nome';
            solecitanteDetails.solecitanteLastname = (String.isNotBlank(workOrder.Contact.LastName )) ? workOrder.Contact.LastName  : '.';
            solecitanteDetails.solecitanteIdentificationType = 'CPF';
            solecitanteDetails.solecitantePhone = workOrder.Contact.Phone;
            if (solecitanteDetails.solecitantePhone == null || solecitanteDetails.solecitantePhone.isAlpha()) {
                solecitanteDetails.solecitantePhone = '999999999';
            }
            solecitanteDetails.solecitanteId = workOrder.Contact.IdentityNumber__c;
            if (solecitanteDetails.solecitanteId  == null) {
                solecitanteDetails.solecitanteId  = '999999999';
            }
            solecitanteDetails.solecitanteCompanyName = workOrder.Account.Company__c;
            solecitanteDetails.solecitanteEmail = workOrder.Contact.Email;
            solecitanteDetails.solecitanteSecondaryPhone = workOrder.Contact.MobilePhone;
            if (solecitanteDetails.solecitanteSecondaryPhone  == null || solecitanteDetails.solecitanteSecondaryPhone.isAlpha()) {
                solecitanteDetails.solecitanteSecondaryPhone  = '999999999';
            }          
            
            WSEecoCreateOrderBrazil.firmanteDetails firmanteDetails = new WSEecoCreateOrderBrazil.firmanteDetails();
            firmanteDetails.firmanteName = workOrder.Contact.FirstName;
            firmanteDetails.firmanteLastname = '.';
            firmanteDetails.firmanteIdentificationType = workOrder.Contact.IdentityType__c;          
            firmanteDetails.firmanteId = workOrder.Contact.IdentityNumber__c;
            firmanteDetails.firmanteCompanyName = workOrder.Account.Company__c;
            firmanteDetails.firmanteEmail = workOrder.Contact.Email;
            firmanteDetails.firmantePhone = workOrder.Contact.Phone;
            
			if (firmanteDetails.firmantePhone == null || firmanteDetails.firmantePhone.isAlpha()) {
                firmanteDetails.firmantePhone  = '999999999';
            }          
     
            /*gerando data de expiração*/
            DateTime aDate = System.now();
            System.debug(aDate);
            DateTime expDate = DateTime.newInstance(aDate.date().addDays(3), aDate.time());
            System.debug(expDate);
			WSEecoCreateOrderBrazil.claimDetails claimDetails = new WSEecoCreateOrderBrazil.claimDetails();			 
            List<BR_EcoClaims__mdt> lstBuzones = new List<BR_EcoClaims__mdt>();

            lstBuzones = [ SELECT AreaWA__c, Reason__c,ReasonWAT__c,SubCauseBR__c,Type__c,Company__c,ServiceCode__c
                               FROM BR_EcoClaims__mdt 
                               WHERE Reason__c =: workOrder.case.Reason  
                               AND SubCauseBR__c =: workOrder.case.SubCauseBR__c
                               AND Company__c =: workOrder.NumberofPointofDelivery__r.CompanyID__c
                               AND Type__c =: workOrder.case.Type
                               LIMIT 1]; 
            
            
                if(lstBuzones.size() > 0 ){
                    claimDetails.buzon = lstBuzones[0].AreaWA__c;
                    claimDetails.subBuzon = lstBuzones[0].ReasonWAT__c;  
					body.serviceCode = lstBuzones[0].ServiceCode__c;					
                }else{
                    claimDetails.buzon = workOrder.AreaWA__c;
            		claimDetails.subBuzon = workOrder.ReasonWAT__c;
                }     
            
            //codigo replicado, setando valores para teste.
            claimDetails.origin = workOrder.Case.Origin;
            List<CNT_General_Settings__c> caseOrigin = [select Value__c from CNT_General_Settings__c where name='EcoClaimsOrigins' limit 1];  
            
            if(caseOrigin.size()> 0){
                if(caseOrigin[0].Value__c.contains('\'' + claimDetails.origin + '\'' )){
                    return;
                }
                
            }
               
               
            claimDetails.caseIdMkt = workOrder.CaseId;
            claimDetails.caseNumberMkt = workOrder.Case.CaseNumber;

            claimDetails.unit = workOrder.Case.Unity__c;
            claimDetails.radicado = workOrder.Case.fileNumber__c;
            claimDetails.motivo = workOrder.Case.Reason;
            claimDetails.subMotivo = workOrder.Case.SubCauseBR__c;
            
            claimDetails.dateAndTimeOrderCreation = workOrder.CreatedDate.format('yyyy-MM-dd\'T\'HH:mm:ss','UTC'); 
            claimDetails.createdBy = workOrder.CreatedBy.Name;          
            claimDetails.status = 'STA01';
            claimDetails.feasibilityNumber = workOrder.CNT_TechnicalUser__c;
            claimDetails.priority = workOrder.OrderAggravatingCondition__c;
            claimDetails.priorityDescription = workOrder.AggravatingCondition__c;
            claimDetails.ansStartDateAndTime = system.now().format('yyyy-MM-dd\'T\'hh:mm:ss');
            claimDetails.requestDueDate =  workOrder.DueDate__c <> null ? workOrder.DueDate__c.format('yyyy-MM-dd\'T\'hh:mm:ss') : Datetime.now().format('yyyy-MM-dd\'T\'hh:mm:ss');
                			
            claimDetails.ansDayCount = '1';
            claimDetails.damageDateAndTime = workOrder.WOFailureHourDate__c <> null ? workOrder.WOFailureHourDate__c.format('yyyy-MM-dd\'T\'hh:mm:ss'):'';
            claimDetails.internalAttentionType = workOrder.Case.Type;
            //Assunto do caso ou da ordem de serviço
            claimDetails.motivoAndSubmotivo = workOrder.Case.Subject <> null ? workOrder.Case.Subject:' ';        
            claimDetails.requestNotes = workOrder.Case.Observations__c !=null ? workOrder.Case.Observations__c : workOrder.Case.CaseRemarks__c!=null ? workOrder.Case.CaseRemarks__c :workOrder.Case.Description !=null ? workOrder.case.Description : 'Observações está vazio no caso';
            
          

            String bodyStsr = JSON.serialize(claimDetails); 
            system.debug('claimDetails:::::::'+claimDetails);
            system.debug('bodyStsr:::::::'+bodyStsr);
            
            WSEecoCreateOrderBrazil.contestedReading contestedReading = new WSEecoCreateOrderBrazil.contestedReading();
            contestedReading.contestedActiveReadingF1 =	'0.00';       
            contestedReading.contestedReactiveReadingF1 = workOrder.ReActiveDaytimeReading__c;
            contestedReading.customerPresence = workOrder.CustomerPresence__C == true ? 'Y' : 'N';
            
            WSEecoCreateOrderBrazil.readingMadeByClient readingMadeByClient = new WSEecoCreateOrderBrazil.readingMadeByClient();
            
            WSEecoCreateOrderBrazil.supplyingAndPodLocation supplyingAndPodLocation = new WSEecoCreateOrderBrazil.supplyingAndPodLocation();
            
            if(PointofDelivery.Name == '000000-0'){
                system.debug('Ingreso a no cliente');
                Address__c address = new Address__c();
                address= [SELECT Name , StreetMD__r.Municipality__c , StreetMD__r.Region__c ,StreetMD__r.NeighbourhoodCode__c,StreetMD__r.LocationCode__c  , Reference__c
                          FROM Address__c Where Id=: workOrder.DetailAddress__c];
                             
                
                supplyingAndPodLocation.address = address.Name;
                supplyingAndPodLocation.locationMunicipality =address.StreetMD__r.Municipality__c;
                supplyingAndPodLocation.department = address.StreetMD__r.Region__c;
                supplyingAndPodLocation.neighborhood =address.StreetMD__r.NeighbourhoodCode__c ;
                supplyingAndPodLocation.location =	address.StreetMD__r.LocationCode__c;
                supplyingAndPodLocation.drivingDirections =address.Reference__c;
                
                
            }else{              
                supplyingAndPodLocation.address = PointofDelivery.PointofDeliveryAddress__c;
                supplyingAndPodLocation.locationMunicipality = PointofDelivery.DetailAddress__r.StreetMD__r.Municipality__c;
                supplyingAndPodLocation.department = PointofDelivery.DetailAddress__r.StreetMD__r.Region__c;
                supplyingAndPodLocation.neighborhood = PointofDelivery.DetailAddress__r.StreetMD__r.NeighbourhoodCode__c ;
                supplyingAndPodLocation.location = PointofDelivery.DetailAddress__r.StreetMD__r.LocationCode__c;
                supplyingAndPodLocation.drivingDirections = PointofDelivery.DetailAddress__r.Reference__c;
            }
                       
            body.requestIdentifier = requestIdentifier;
            body.damageDetails = damageDetails;
            body.technicalData = technicalData;
            body.duenoDetails = duenoDetails;
            body.solecitanteDetails = solecitanteDetails;
            body.firmanteDetails = firmanteDetails;
            body.claimDetails = claimDetails;
            body.contestedReading = contestedReading;
            body.readingMadeByClient = readingMadeByClient;
            body.supplyingAndPodLocation = supplyingAndPodLocation;        
        }
        
        String headerStr = JSON.serialize(header); 
        String bodyStr = JSON.serialize(body); 
        String reqStr = JSON.serialize(require); 
        system.debug(' :: headerStr :: ' + headerStr);
        system.debug(' :: bodyStr :: ' + bodyStr);
        system.debug(' :: reqStr :: ' + reqStr);
        system.debug(' :: wk :: ' + wk);
        system.debug('CHAMOU CALLOUT');
        callout(reqStr , workOrder.Id);
    }
       
    static void callout(String reqStr , String workOrderNumber){
		String JSON_res;
        
        System.debug(' ::param:::' + ECH_VFC036_GeneralConstants.ECO_MetaCreate);
        
        RestCallout__mdt param = [ SELECT NameCredentials__c, Body__c,DeveloperName,Endpoint__c,Header1__c,HeaderValue1__c,Id,Label,Language,MasterLabel,Method__c 
                                  FROM RestCallout__mdt WHERE Name__c='Eco_CreateOrder_Brasil']; 
                                  
                        
        
        System.debug(' ::param:::' + param);
        		
        Blob headerValue = Blob.valueOf(param.NameCredentials__c);

		String authorizationHeader = 'Basic ' + EncodingUtil.base64Encode(headerValue);
        system.debug('authorizationHeader ' + authorizationHeader);
        
        HttpRequest request = new HttpRequest();
        request.setEndpoint(param.EndPoint__c);
        request.setMethod(param.Method__c);
        request.setHeader(param.Header1__c, param.HeaderValue1__c);
        request.setHeader('Authorization', authorizationHeader); 
        request.setBody(reqStr);
        request.setTimeout(120000);
        System.debug('  :: Peticion CreateOrder::: '+reqStr );  
        if (!Test.isRunningTest()) {
            Http http = new Http();
            HttpResponse res = http.send(request);
            JSON_res = res.getBody();
            
            system.debug(' :: getStatus::: '+ res.getStatus());
            system.debug(' :: getStatusCode::: '+ res.getStatusCode());
            system.debug(' :: getBody::: '+ res.getBody());               
            
            fromJSON json = (fromJSON)JSON.deserialize(JSON_res, fromJSON.class);          
            procesaRespuesta(json,workOrderNumber);
        }       
    }  

    public static void procesaRespuesta(fromJSON response  , String workOrderNumber) {
        system.debug('  :: Procesar Respuesta  ::: ' + response);
        MarketEvolutionControllerBrasil mkc = new MarketEvolutionControllerBrasil(); 
        WorkOrder updWorkOrder =  new WorkOrder();  
        updWorkOrder.id = workOrderNumber;
        system.debug('  :: response.Body.meta.status ::: ' + response.Body.meta.status);
        system.debug('  :: updWorkOrder::: ' + updWorkOrder);
        if(response.Body.meta.status  == 'OK'  && updWorkOrder != null){
            //updWorkOrder.Order_Number_BE__c = (String.isNotBlank(response.Body.data.caseNumber)) ? response.Body.data.caseNumber : response.Body.data.caseId;
            updWorkOrder.CNT_NumberOrderChild__c  = (String.isNotBlank(response.Body.data.caseNumber)) ? response.Body.data.caseNumber : response.Body.data.caseId;              
        }else{                  
            updWorkOrder.Status = ECH_VFC036_GeneralConstants.ECO_Status_ComInterrumpida;  
            if(response.Body.data != null){              
                updWorkOrder.ExecutiveObservations__c = (String.isNotBlank(response.Body.data.motivationError)) ? response.Body.data.motivationError : null;             
            }else{
                updWorkOrder.ExecutiveObservations__c = (String.isNotBlank(response.Body.meta.message)) ? response.Body.meta.message : null;             
            }       
        }
        try{ 
            system.debug('updWorkOrder : ' + updWorkOrder);
            update updWorkOrder ; 
        } catch(DmlException e) {
            System.debug('The following exception has occurred: ' + e.getMessage());
        }
    }
