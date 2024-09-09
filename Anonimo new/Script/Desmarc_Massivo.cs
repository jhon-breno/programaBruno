    ATCL_VFC042_wsdlUtils.HeaderRequestType header;
    ATCL_VFC042_wsdlUtils.BodyPutPerfilFacturacionRequest_element body = new ATCL_VFC042_wsdlUtils.BodyPutPerfilFacturacionRequest_element();
    ATCL_VFC042_wsdlUtils.EXP_SOAP_SALESFORCE_Soap ws;
    private static final string CASE_RECORDTYPE_ATC_BRASIL = 'ATC_Brasil';

    BillingProfileContact__c currentBillingPr;

    currentBillingPr = [SELECT id, Billing_Profile__c, Billing_Profile__r.AccountContract__c, Billing_Profile__r.PointofDelivery__r.CompanyID__c, Billing_Profile__r.PointofDelivery__r.SegmentType__c, Contacto__c, Billing_Profile__r.PointofDelivery__c,
      Billing_Profile__r.Account__r.Id
      FROM BillingProfileContact__c
      WHERE Billing_Profile__c = 'a1u1o000003tKy7AAE'
      and Billing_Profile__r.EDEEnrolment__c = true limit 1
    ];

    if (currentBillingPr < > null && currentBillingPr.Billing_Profile__r.PointofDelivery__c != null && currentBillingPr.Billing_Profile__r.PointofDelivery__r.CompanyID__c != null) {
      body.CodigoEmpresa = currentBillingPr.Billing_Profile__r.PointofDelivery__r.CompanyID__c;
    }

    body.CuentaContrato = currentBillingPr.Billing_Profile__r.AccountContract__c;

    List < BillingProfileContact__c > listabpc = [SELECT Id, Contacto__c FROM BillingProfileContact__c WHERE Billing_Profile__c =: currentBillingPr.Billing_Profile__c ORDER BY CreatedById];
    system.debug('listabpc ' + listabpc);

    body.Tipo = '00';
    body.Motivo = 'MAIL';
    body.Cliente = currentBillingPr.Billing_Profile__r.AccountContract__c;
    body.SubMotivo = Label.ATCL_CauseSAP_DeliveryInvoice;
    body.NumeroCaso = currentBillingPr.Billing_Profile__r.AccountContract__c;
    body.EmailBilling = 'NO';

    header = ATCL_WS00_Utils.initHeaderBRA('putPerfilFacturacion', ECH_VFC036_GeneralConstants.WS_SYSTEM_CODE_SAP, body.CodigoEmpresa);

    try {
      ws = new ATCL_VFC042_wsdlUtils.EXP_SOAP_SALESFORCE_Soap();
      ATCL_VFC042_wsdlUtils.Put_PerfilFacturacionResponse_element response = ws.Put_PerfilFacturacion(header, body);
      System.debug('invokePutPerfilFacturacionNoChilds response: ' + response);

      if (response < > null && (response.BodyPutPerfilFacturacionResponse.CodigoResultado == '0' || response.BodyPutPerfilFacturacionResponse.CodigoResultado == '000')) {
        system.debug('Reponse resultado: ' + response.BodyPutPerfilFacturacionResponse.DescripcionResultado);

        if (response.BodyPutPerfilFacturacionResponse.DescripcionResultado != null && response.BodyPutPerfilFacturacionResponse.DescripcionResultado.equals(Label.ATCL_WS_ServiceOK)) {
          system.debug('Response resultado 2: ' + response.BodyPutPerfilFacturacionResponse.DescripcionResultado);

          Case newCase = new Case();
          newCase.Status = ECH_VFC036_GeneralConstants.COD_ESTADO_CERRADO;
          newCase.Subject = 'Desmarcação massiva fatura por email';
          newCase.SubcauseBR__c = 'ATBR011';
          newCase.Reason = 'MOT017';
          newCase.RecordTypeId = Schema.SObjectType.Case.getRecordTypeInfosByDeveloperName().get(CASE_RECORDTYPE_ATC_BRASIL).getRecordTypeId();
          newCase.ContactId = currentBillingPr.Contacto__c;
          newCase.AccountId = currentBillingPr.Billing_Profile__r.Account__r.Id;
          newCase.PointofDelivery__c = currentBillingPr.Billing_Profile__r.PointofDelivery__c;

          Billing_Profile__c bp = new Billing_Profile__c();
          bp.Id = currentBillingPr.Billing_Profile__c;
          bp.EDEEnrolment__c = false;

          insert newcase;
          update bp;
          delete listabpc;
        }
      }

    } catch (Exception ex) {
      String errorMsg = 'Se ha producido un error. Mensaje: ' + ex.getMessage() + '. Traza: ' + ex.getStackTraceString();
      System.debug(errorMsg);
    }