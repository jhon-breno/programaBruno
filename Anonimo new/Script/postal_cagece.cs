
	String billingPrId = '{0}';
    Billing_Profile__c currentBillingPr;
    List < Billing_Profile__c > currentBillingPrList;
    ATCL_VFC042_wsdlUtils.HeaderRequestType header;
    ATCL_VFC042_wsdlUtils.BodyPutPerfilFacturacionRequest_element body = new ATCL_VFC042_wsdlUtils.BodyPutPerfilFacturacionRequest_element();
    ATCL_VFC042_wsdlUtils.EXP_SOAP_SALESFORCE_Soap ws;

    currentBillingPrList = [SELECT DocumentType__c, AccountContract__c, Type__c, CNT_Due_Date__c, CurrentAccountNumber__c, Agency__c, BankBR__c, BillingAddress__c, EDEEnrolment__c, Main_Email__c,
            CardType__c, CardNumber__c, CurrentAccountNum__c, CBU_Arg__c, PointofDelivery__c, PointofDelivery__r.CompanyID__c,
            Delivery_Type__c, Contact__c, BallotName__c, IdentityType__c, BankDocumentOwner__c,
            PAStreet__c, PANumber__c, PAPostalCode__c, PACity__c, PANeighbourhood__c, PAState__c, PAComplement__c,Account__c
            FROM Billing_Profile__c WHERE AccountContract__c = :billingPrId and company__c='2003'];

        if (!currentBillingPrList.isEmpty()) 
        {
            currentBillingPr = currentBillingPrList.get(0);
        }

        Account currentAccount = [SELECT CompanyID__c, AccountClientNumber__c, Type, IdentityType__c FROM Account WHERE Id =: currentBillingPr.Account__c];
       

        /*======================= SAP ======================*/
        if (body.Actividad == null) 
        {
            system.debug('putPerfilFacturacion SAP ');
            header = ATCL_WS00_Utils.initHeaderBRA('putPerfilFacturacion', ECH_VFC036_GeneralConstants.WS_SYSTEM_CODE_SAP, body.CodigoEmpresa);
        }

        /*---------- POR MOTIVO ----------*/
        // CAUSALE ALTERNATIVE

            //body.Cliente = currentAccount.AccountClientNumber__c;
            body.SubMotivo = Label.ATCL_CauseSAP_DeliveryInvoice;
            body.Motivo = 'ALTERNATIVE';
            body.NumeroCaso = '1';
            body.Cliente = currentAccount.AccountClientNumber__c;
            body.SubMotivo = Label.ATCL_CauseSAP_DeliveryInvoice;
            body.Motivo = 'ALTERNATIVE';               
            body.Calle = 'RUA DO CAMPO';
            body.Numero = '160';
            body.CodigoPostal = '60510461';
            body.Ciudad = 'Fortaleza';
            body.DireccionExterna = currentBillingPr.AccountContract__c+'_MA';
            body.Barrio = 'Pici';
            body.Estado = 'CE';
            body.CuentaContrato = billingPrId; 
		    body.CodigoEmpresa = '2003';
            body.ComplementoDireccion = '3 andar';
     
        /*======================= SAP ======================*/
    System.debug('invokePutPerfilFacturacionNoChilds header ' + header);
    System.debug('invokePutPerfilFacturacionNoChilds body: ' + body);

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
