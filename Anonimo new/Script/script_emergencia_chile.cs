Id caseId = '5000600006ZntsxAAB';
     String COD_5D0288 = '5D0288';
     String COD_CAREQ = 'CAREQ';
    String COD_INSEQ = 'INSEQ';
     String COD_PREEQ = 'PREEQ';
     String COD_RETEQ = 'RETEQ';


GESI_VFC000_Utils.ResCaseGesi res = new GESI_VFC000_Utils.ResCaseGesi();


        Datetime start = datetime.now();
        while(System.Now().getTime()< start.getTime()+5000){}
        System.debug('Iniciando createCaseAction');
        String countryrm = 'NULL';
        String status = 'NULL';
        Case fcase;
        Case caseData;
        String country;
        List<Interaction__c> listaInteracciones = new List<Interaction__c>();
        Interaction__c interaction;
        User agent;
        Address__c addr;
        Street__c st;
        String invalidationTypeId = '';
        GESI_VFC000_Utils.CaseGesiBody caseBody;
        try{
            fcase = GESI_VFC000_Utils.findCase(caseId);
            System.debug('caso encontrado: '+fcase);

            countryrm = fcase.Country__c;

            //System.debug('POD_EventodeCorte: ' + fcase.PointofDelivery__r.Evento_del_corte__c);
            System.debug('POD_PointofDeliveryStatus__c: '+fcase.PointofDelivery__r.PointofDeliveryStatus__c);
            if(fcase.Status!=null){
                /* vb incidencia GESI 08/04/19
                    2.  Cuando se selecciona un motivo de invalidación MOT007 no enviar el SubCause o SubMotivo y si el InvalidationTypeId. 
                        En la creación de caso C02_CrearCaso los parámetros notification_subtype (no informar),
                        invalidation_type_Id (si informar, ya se hace) y el estado se debe enviar cómo se envía actualmente en Invalidado.
                    */
                if(fcase.Reason!=null &&  fcase.Reason.equals(GESI_VFC000_GeneralConstants.CODE_REASON_MOT007)
                ){
                    system.debug('DENTRO IF invalidationTypeId: motivo MOTCO007, aqui no se deberian enviar submotivo al servicio');
                    status = 'ESTACO003';
                    invalidationTypeId = fcase.SubCause__c;
                        //vb preguntar a Militza, si el submotivo se enviar en dos campos ....uno aplica y el otro no?
                // APR - CA - Si es motivo del caso es MOTCO007 o MOTCO005 con subcause SUCO055 o SUBCO059, enviamos el status invalidación (ESTACO003)
                }else if(fcase.Status=='ESTA002' || fcase.Status=='ESTACO001' || fcase.Status == 'ESTACO010'){
                    System.debug('ENTRA EN CAMBIO DE ESTADO 1:' + fcase.Status);
                    status = 'ESTACO001';
                }else{status = fcase.Status;}
            }else {status = 'NULL';}

            //JSC Cambiamos el estado de la interaccion que busca a 3, ya que se crea cerrada y si es 2 no encuentra ninguna
            System.debug('caseId '+caseId);
            listaInteracciones = GESI_VFC000_Utils.findInteraction(caseId, 3);
            if(listaInteracciones <> null & listaInteracciones.size() > 0 ){
                interaction = listaInteracciones.get(0);
            }
            System.debug('Interacción encontrada: '+interaction);
            System.debug('se va a ejecutar findAgent');
            agent = GESI_VFC000_Utils.findAgent(interaction.Agent__c);
            System.debug('agent encontrado: '+agent);
            if(fcase.Country__c.equalsIgnoreCase(GESI_VFC000_GeneralConstants.COUNTRY_COL)){
                country = GESI_VFC000_GeneralConstants.CODE_COL;
            } else if(fcase.Country__c.equalsIgnoreCase(GESI_VFC000_GeneralConstants.COUNTRY_CHL)){
                country = GESI_VFC000_GeneralConstants.CODE_CHL;
            }
            
            caseData = [SELECT PointofDelivery__r.PointofDeliveryNumber__c, Contact.FirstName, origin, Contact.Name, Description,ContactName__c,Contact.LastName, Contact.Phone, PointofDeliveryNumber__c, Contact.IdentityNumber__c, Contact.Description, Contact.ContactAddress__r.StreetMD__r.LocationCode__c , Contact.ContactAddress__r.StreetMD__r.Municipality__c, Contact.ContactAddress__r.StreetMD__r.Name, Contact.ContactAddress__r.Number__c, Contact.ContactAddress__r.Postal_Code__c, Contact.ContactAddress__r.StreetMD__r.Region__c, Observations__c, ReferenceCO__c, Account.Address__c, Address__c, Reason, Account.MainPhone__c, Account.Name, ContactPhone__c, ContactSecondaryPhone__c,Status,Company__c, Descripcion_del_ticket__c, Address__r.Number__c, toLabel(Category__c), toLabel(AggravatedCondition__c), PointofDelivery__r.DetailAddress__r.CoordinateX__c, PointofDelivery__r.DetailAddress__r.CoordinateY__c, Address__r.CoordinateX__c, Address__r.CoordinateY__c FROM Case WHERE Id = :caseId];
            // APR
            addr = GESI_VFC000_Utils.findAddress(caseData.Address__c);
            String idStreet;
            System.debug('ADDR DEBUG: '+addr);
            if(addr <> null && addr.StreetMD__c <> null){
                idStreet = addr.StreetMD__c;
                st = GESI_VFC000_Utils.findStreet(idStreet);
                System.debug('ST DEBUG: ' + st);
                
            }
        }catch(Exception e) {String edesc = Label.GESI_ERROR_DATA+' '+e.getStackTraceString();
            System.debug('Error description: '+edesc);
            System.debug('ERROR!: '+e.getMessage());
        }

        try{
            System.debug('caseData '+caseData);
            //JSON BODY BUILD
            GESI_VFC000_Utils.RequestGesiHeader intHeader = new GESI_VFC000_Utils.RequestGesiHeader(GESI_VFC000_GeneralConstants.GESI_FUNCIONALIDAD02);
            //JSC - Cambiamos caseData.Contact.Description por caseData.Observations__c
            //Cuando se actulice el WS se deberá cambiar el envio de Municipality__c a LocationCode__c
            //EHG 11/03/2019 Lo modifico a petición de Militza 
            GESI_VFC000_Utils.CommonDataCaseGesi codata;
            /*vb GESI 08/04/2019
            Cuando se crea un caso, en el AI 02_CrearCaso se pasa los datos del contacto en los parámetros contactSurname y contactName del parámetro compuesto  commonData. 
            Se debe enviar en ambos parámetros el Contact.Name del contacto que tiene el nombre completo.
            */
            //aj - 23/09/2020 modificaciones GESI Chile
            String descr = (caseData.Description != null) ? caseData.Description + '\n' : '';
            descr += (caseData.Category__c != null) ? caseData.Category__c + '\n' : '' ;
            descr += (caseData.AggravatedCondition__c != null) ? caseData.AggravatedCondition__c : '' ;

            System.debug('Observaciones'+caseData.Observations__c);
            
            System.debug('Observaciones'+caseData.Observations__c);

            //MEjora para controlar si hay algun error de falta de datos
            boolean errorControlado=false;
            GESI_VFC000_Utils.ResCaseGesi res = new GESI_VFC000_Utils.ResCaseGesi();
			List<String> dummyContact = System.Label.Dummy_Contact.split(',');   //[RA for row lock]
            //System.debug('st.LocationCode__c'+st.LocationCode__c);
            //aj - 13/10/2020 cambios fase 2 GESI CHILE
             if(((caseData.Contact <> null && caseData.Contact.Name <> null) && st != null && (st.LocationCode__c <> null)) || ( st != null && st.Municipality__c <> null && intHeader.CodSistema == GESI_VFC000_GeneralConstants.GESI_CODSISTEMA_CHILE) || (caseData.Contact <> null && caseData.Contact.Name <> null && st != null && countryrm == 'COLOMBIA' ) ){
                codata = new GESI_VFC000_Utils.CommonDataCaseGesi(caseData.Contact.Name, caseData.Contact.Name, caseData.ContactPhone__c, caseData.Observations__c, st.LocationCode__c, st.Region__c);
                //aj - 14/02/2020 Cambio para servicio GESI CHILE
                if(intHeader.CodSistema == GESI_VFC000_GeneralConstants.GESI_CODSISTEMA_CHILE){                
                    codata = new GESI_VFC000_Utils.CommonDataCaseGesi(caseData.Contact.Name, caseData.Contact.Name, caseData.ContactPhone__c, descr, st.Municipality__c, st.Municipality__c);
                    //aj - 13/10/2020 cambios fase 2 GESI CHILE
                    codata.contactName = (caseData.Contact <> null && caseData.Contact.Name <> null) ? caseData.Contact.Name : '--';
                    codata.contactSurname = (caseData.Contact <> null && caseData.Contact.Name <> null) ? caseData.Contact.Name : '--';
                    codata.contactTelephone = (caseData.ContactPhone__c <> null) ? caseData.ContactPhone__c : '999999999';
                    codata.description = (descr != '') ? descr : '--';
                    codata.municipalityId = (st != null && st.Municipality__c <> null) ? st.Municipality__c : '';
                    codata.provinceId = (st != null && st.Municipality__c <> null) ? Integer.valueof(st.Municipality__c) : null;
                }
            }else if(countryrm == 'COLOMBIA' && caseData.Contact == null && caseData.Contact.Name == null){ //[RA for row lock] 
                 codata = new GESI_VFC000_Utils.CommonDataCaseGesi(dummyContact[0], dummyContact[0], dummyContact[1], caseData.Observations__c, st.LocationCode__c, st.Region__c);
                 
            }else if (st != null && ((st.LocationCode__c <> null) || (intHeader.CodSistema == GESI_VFC000_GeneralConstants.GESI_CODSISTEMA_CHILE && st.Municipality__c == null))){
                errorControlado=true;fcase.Status = 'ESTA004';fcase.Error__c = 'Error de validacion en el campo: "municipalityId"';fcase.TraceErrorInservice__c = 'Error de validacion en el campo: "municipalityId"';
                fcase.InserviceSentError__c = false;
                //No existe Municipality
                // Pasamos el caso a 'En Verificación'
                 //SP- RITM00007460041[START]
            }else if(caseData.Origin == 'CAN007' && intHeader.CodSistema == GESI_VFC000_GeneralConstants.GESI_CODSISTEMA_CHILE ){
                       errorControlado=true;fcase.Status = 'ESTA001';fcase.Error__c = 'Error de validacion en el campo: "contacto"';fcase.TraceErrorInservice__c = 'Error de validacion en el campo: "contacto"';fcase.InserviceSentError__c = false; 
                       fcase.description = (descr != '') ? descr : '--';
                    }
            //SP- RITM00007460041[END]
            else {
                errorControlado=true;fcase.Status = 'ESTA004';fcase.Error__c = 'Error de validacion en el campo: "contacto"';fcase.TraceErrorInservice__c = 'Error de validacion en el campo: "contacto"';
                fcase.InserviceSentError__c = false;
                //No existe Contacto
                // Pasamos el caso a 'En Verificación'
            }

            System.debug('caseData reason: ' + caseData.reason);
            System.debug('caseData status: ' + caseData.Status);
            if ((!caseData.Reason.equals(GESI_VFC000_GeneralConstants.CODE_REASON_MOT007)) &&
                (caseData.Status!='ESTA002' && caseData.Status!='ESTACO001' && caseData.Status!='ESTACO010')){errorControlado=true; caseData.Error__c ='Combinacion de estado, motivo y submotivo erroneo';caseData.TraceErrorInservice__c = 'Combinacion de estado, motivo y submotivo erroneo';caseData.InserviceSentError__c = false;
            }

                system.debug('codata '+codata);
                GESI_VFC000_Utils.CustomerDataCaseGesi cusdata; 
                // APR
                //Start - [RA for row lock]
                  if(countryrm == 'COLOMBIA' && caseData.Account.Name == null){
						cusdata = new GESI_VFC000_Utils.CustomerDataCaseGesi(dummyContact[2],dummyContact[2], dummyContact[3], caseData.PointofDeliveryNumber__c, caseData.PointofDeliveryNumber__c, dummyContact[4], '', dummyContact[5], null, '', '', (addr != null) ? addr.Name:'');
					}else{
							cusdata = new GESI_VFC000_Utils.CustomerDataCaseGesi(caseData.Account.Name,caseData.Account.Name, caseData.ContactSecondaryPhone__c, caseData.PointofDeliveryNumber__c, caseData.PointofDeliveryNumber__c, caseData.Contact.IdentityNumber__c, '', caseData.Contact.ContactAddress__r.Number__c, null, '', '', (addr != null) ? addr.Name:'');
				}
				//End - [RA for row lock]
                //aj - 14/02/2020 Cambio para servicio GESI CHILE
                if(intHeader.CodSistema == GESI_VFC000_GeneralConstants.GESI_CODSISTEMA_CHILE){
                    //aj 16/11/2020 - reprocesos
                    String podNumber = '';
                    if(caseData.PointofDelivery__c != null && caseData.PointofDelivery__r.PointofDeliveryNumber__c != null){podNumber = caseData.PointofDelivery__r.PointofDeliveryNumber__c;}
                    podNumber = podNumber.leftPad(12, '0');
                    String podNumberPre = '';
                    //aj - 21/05/2020 solucion errores GESI CHILE
                    if(caseData.CompanyID__c == '6'){podNumberPre = Label.GESI_PODNumber_COL + podNumber.leftPad(12, '0');}
                    else{podNumberPre = Label.GESI_PODNumber + podNumber.leftPad(12, '0');}
                    //aj - cambios requeridos GESI CHILE
                    String streetNumber = (caseData.Contact.ContactAddress__r <> null && caseData.Contact.ContactAddress__r.Number__c <> null) ? caseData.Contact.ContactAddress__r.Number__c : (addr <> null && addr.Number__c <> null) ? addr.Number__c : null;
                    cusdata = new GESI_VFC000_Utils.CustomerDataCaseGesi(caseData.Account.Name,caseData.Account.Name, /*'42323232323'*/caseData.ContactSecondaryPhone__c, /*'CLPOD0ENL000000782812'*/podNumberPre, /*''*/podNumber, podNumber/*'000000782812'*//*caseData.Contact.IdentityNumber__c*/, '', streetNumber, null, '', '', (addr != null) ? addr.Name:'');
                    if(caseData.PointofDelivery__r.DetailAddress__c != null){
                        if(caseData.PointofDelivery__r.DetailAddress__r.CoordinateX__c != null && caseData.PointofDelivery__r.DetailAddress__r.CoordinateY__c != null){
                            cusdata.coordinateX = decimal.valueOf(caseData.PointofDelivery__r.DetailAddress__r.CoordinateX__c);
                            cusdata.coordinateY = decimal.valueOf(caseData.PointofDelivery__r.DetailAddress__r.CoordinateY__c);  
                        }
	                }
                
                }

                system.debug('cusdata '+cusdata);


                // if (country == GESI_VFC000_GeneralConstants.CODE_COL && (((fcase.Reason == 'MOTCO007') && (fcase.SubCause__c == 'MOTINVCO010'))||(fcase.Reason == 'MOTCO005' && ((fcase.SubCause__c == 'SUBCO055')||(fcase.SubCause__c == 'SUBCO059'))))) {
                //     invalidationTypeId = fcase.SubCause__c;
                // }
                caseBody = null;
                if(fcase.Country__c.equalsIgnoreCase(GESI_VFC000_GeneralConstants.COUNTRY_COL)){
                    caseBody = new GESI_VFC000_Utils.CaseGesiBody(country, fcase.CaseNumber, status, fcase.CreatedBy.Name, fcase.Origin, codata, cusdata, fcase.Reason, fcase.SubCause__c, fcase.Priority__c, invalidationTypeId, interaction.Name, interaction.StartDate__c,interaction.StartDate__c, fcase.NewPromiseDate__c, 0, (fcase.Corte_programado__c != null && fcase.Corte_programado__c.equals('True'))?true:false, '');
                } else if(fcase.Country__c.equalsIgnoreCase(GESI_VFC000_GeneralConstants.COUNTRY_CHL)){
                    Boolean corteProgramado;
                    if(fcase.Corte_programado__c <> null && fcase.Corte_programado__c.equalsIgnoreCase('True')){corteProgramado = true;} else {corteProgramado = false;
                    }

                    status = GESI_VFC000_GeneralConstants.INTERACTION_STATUS_OPEN;
                    if(fcase.SubCause__c != COD_5D0288 && fcase.SubCause__c != COD_CAREQ && fcase.SubCause__c != COD_INSEQ && fcase.SubCause__c != COD_PREEQ && fcase.SubCause__c != COD_RETEQ /*&&
                    fcase.PointofDelivery__r != null && fcase.PointofDelivery__r.Valor_corte__c != null && fcase.PointofDelivery__r.Valor_corte__c == 0*/){
                        
                        System.debug('ENTRA EN EL IF DE SUBCAUSE!!!');
                        
                        //aj - 20/08/2020 nuevo REQ para creacion de nuevo caso
                        String subC = fcase.SubCause__c;
                        if(fcase.Description != null && fcase.Description.contains(GESI_VFC000_GeneralConstants.GESI_CASE_ANULACION)){subC = GESI_VFC000_GeneralConstants.GESI_CASE_ANULACION_22;
                        }
                        String prior='';
                        if(fcase.ElectrodependantValidate__c != null && fcase.ElectrodependantValidate__c){
                            prior = '1450';//valor necesario en GESI para cliente singular
                        }else{
                            prior = fcase.SubCause__c;
                        }
                        System.debug('fcase.AggravatedCondition__c '+fcase.AggravatedCondition__c);
                        
                        
                        System.debug ('PAIS: ' + country + ' Numero de Caso: ' + fcase.CaseNumber + ' Estado: ' + status + ' Nombre del creador: ' + fcase.CreatedBy.Name + ' Canal origen:' + fcase.Origin + ' Codata: ' + codata + ' cusdata: ' + cusdata + ' SubCase: '+ subC/*'17'*/ + ' Subcause Caso: ' + fcase.SubCause__c/*'25'*/ + ' Prioridad: ' + prior/*'43'*/ + ' Tipo de Invalidacion: ' + invalidationTypeId + ' Nombre Interaccion: ' +  interaction.Name + ' Fecha Inicio Interaccion: ' +  interaction.StartDate__c + ' Fecha Inicio Interaccion 2: ' + interaction.StartDate__c + ' Fecha Promise: ' + fcase.NewPromiseDate__c + ' Corte Programado: ' + corteProgramado);
                        caseBody = new GESI_VFC000_Utils.CaseGesiBody(country, fcase.CaseNumber, status, fcase.CreatedBy.Name, fcase.Origin, codata, cusdata, subC/*'17'*/, fcase.SubCause__c/*'25'*/, prior/*'43'*/, invalidationTypeId, interaction.Name, interaction.StartDate__c,interaction.StartDate__c, fcase.NewPromiseDate__c, 0,corteProgramado, '');
                        System.debug('EL CASEBODY: ' + caseBody);
                    }else{
                        fcase.Status = GESI_VFC000_GeneralConstants.INTERACTION_ENVALIDACION_GESI;caseBody = null;
                    }
                    

                }
                if(caseBody != null){
                    system.debug('caseBody '+caseBody);
                    System.debug('caseBody json: ' + JSON.serializePretty(caseBody));
                    GESI_VFC000_Utils.CaseGesi caseGesi = new GESI_VFC000_Utils.CaseGesi(intHeader, caseBody);
                    //REST CALL
                    
                    Http http = new Http();
                    HttpRequest request = new HttpRequest();
                    request.setEndpoint(GESI_VFC000_GeneralConstants.URL_CREATE_NOTIFICATION);

                    if(!Test.isRunningTest()){
                        EnvironmentEndPoint__c envEndpoint = EnvironmentEndPoint__c.getInstance('PRO');
                        if(envEndpoint.Request_auth__c == true){
                            request.setEndpoint('callout:MuleAuthenticationREST/api/SFDC/Caso/CasoEmergencia/Create');
                        }
                    }

                    request.setMethod('POST');
                    request.setHeader('Content-Type', 'application/json;charset=UTF-8');
                    request.setTimeout(20000);    //PM INC000086594634 2022-05-16
                    request.setBody(JSON.serialize(caseGesi,true));
                    //request.setTimeout(120000);
                    System.debug('Body: '+request.getBody());
                    HttpResponse response;
                    if(!Test.isRunningTest()){
                        system.debug('LLAMADA AL SERVICIO');
                        //aj - 13/10/2020 cambios fase 2 GESI CHILE
                        if(fcase.Country__c.equalsIgnoreCase(GESI_VFC000_GeneralConstants.COUNTRY_CHL)){
                            try{
                                response = http.send(request);
                            }catch (Exception e) {
                                String edesc = Label.GESI_ERROR_DATA+' '+e.getStackTraceString();System.debug('Error description: '+edesc);system.debug('ERROR!: '+e.getMessage());
                                fcase.Error__c = '99';fcase.TraceErrorInservice__c = edesc;fcase.InserviceSentError__c = true;
                            }
                        }else{
                            response = http.send(request);
                        }
                    } else {response = GESI_VFC000_Utils.GesiMock(2, fcase.Country__c);
                    }
                    if(response != null && response.getBody() != null){
                        system.debug('RESPUESTA DEL SERVICIO: '+response);
                        res = (GESI_VFC000_Utils.ResCaseGesi)JSON.deserialize(response.getBody(), GESI_VFC000_Utils.ResCaseGesi.class);

                        if(res != null && res.Body.CodigoResultado!=null){
                            if(res.Body.CodigoResultado=='00'){
                                // CA - Esto intuyo que será así, por no dejarlo vacío...
                                fcase.InserviceNumber__c = res.Body.notificationNum;fcase.Error__c = res.Body.DescripcionResultado;fcase.InserviceSentError__c = false;
                            }else{
                                // CA - Comprobamos si el error devuelto entra dentro de los no reprocesables
                                List<String> noReprocesados = new List<String>{'01','02', '03', '04','05','06','07','08', '09', '10', '11', '12','11', '12','13','15', '16', '19', '20', '21', '22', '23', '24', '25', '26', '27','28','29','30','31','33','99'};
                                if(noReprocesados.contains(res.Body.CodigoResultado) && !fcase.Country__c.equalsIgnoreCase(GESI_VFC000_GeneralConstants.COUNTRY_CHL)){
                                    // CA - Pasamos el caso a 'En Verificación'
                                    fcase.Status = 'ESTA004';fcase.Error__c = res.Body.DescripcionResultado;fcase.TraceErrorInservice__c = res.Body.DescripcionResultado;fcase.InserviceSentError__c = false;
                                }else{
                                    fcase.Error__c = res.Body.DescripcionResultado;
                                    fcase.TraceErrorInservice__c = res.Body.DescripcionResultado;
                                    fcase.InserviceSentError__c = true;
                                }
                            }
                        }
                    }
                }

            
 
            interaction.Status__c =  GESI_VFC000_GeneralConstants.INTERACTION_STATUS_CLOSED;
            System.debug('Pintamos la fecha fin con la fecha inicio: '+interaction.StartDate__c);
            interaction.FinishDate__c = interaction.StartDate__c;
            System.debug('Fecha fin: '+interaction.FinishDate__c);
            system.debug('Actualizando la interaccion');
            update interaction;
            system.debug('Actualizando el caso GESI_VFC000_Utils');
            
            //Se incluye este indicador para controlar cuando debe llamar al servicio y cuando no 
            //fcase.EnvioGdI_ACK__c=false;
            // vb 11/042019 si por defecto es false, solo sera true cuando no queramos retorno
            fcase.EnvioGdI_ACK__c=true;
            system.debug('res.Body' +res.Body);
            if(res.Body ==null || caseBody == null){fcase.EnvioGdI_ACK__c=false;update fcase;
            }else {GESI_VFC000_Utils.updateCase(fcase, res);
            }
        } catch (Exception e) {String edesc = Label.GESI_ERROR_DATA+' '+e.getStackTraceString();System.debug('Error description: '+edesc);system.debug('ERROR!: '+e.getMessage());          
        }
    
    