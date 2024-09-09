
Case myCase = [SELECT Id, Country__c, RecordTypeId, CNT_Change_Type__c, CNT_Tipo_de_Ajuste__c, AccountId, CNT_Free_Client__c, AssetId, Asset.CNT_RateType__c, RecordType.DeveloperName, CNT_White_Rate__c,CNT_Potencia__c, CNT_Public_Ilumination__c,CNT_Rural_Irrigating__c, CNT_Cliente_Baixa_Renda__c, Description FROM Case WHERE casenumber= '{0}' WITH SECURITY_ENFORCED];

Id resultDetails = setQuoteOwnerShipBR(myCase.Id, myCase.AccountId);

myCase.cnt_quote__c = resultDetails;
myCase.CNT_Documentation_Validated__c= true;
update myCase;

getNewOwnerChangeThree(mycase.Id, myCase, resultDetails);

public static Id setQuoteOwnerShipBR(String caseId, String accountId)
{ 	            // SELECT Account to check which one we'll working on 
                Account acc = [SELECT Id, RecordType.Name FROM Account WHERE Id =: accountId];
                
                boolean b2cValidation = true;
                
                Set<String> NEValues = new Set<String> { 
                    '40 - Rural',
                    '11 - Residencial Baixa Renda',
                    '41 - Rural C/ ICMS'
                };

                Map<String,String> result = new Map<String,String>();
                String orderId;
                
                orderId = 'a1b06000008SS5KAAW';
				NE__Order__c newQuote = new NE__Order__c();
            
                Map<String,NE__Order_Item_Attribute__c> valuesOrderLineItens = new Map<String,NE__Order_Item_Attribute__c>();
                
                
                if (orderId != null)
				{
                  NE__Order__c myOrderToClone = [Select Id, NE__CatalogId__c, NE__One_Time_Fee_Total__c, NE__CommercialModelId__c,
                                             NE__Recurring_Charge_Total__c, NE__TotalRecurringFrequency__c, NE__Parameters__c,
                                             NE__Type__c, RecordTypeId From NE__Order__c Where Id = :orderId] [0];
                    
                 //Modificado 30-11-2017 por fg - agregado "Grupo" a la query para Brasil    
                 NE__OrderItem__c myOrdItToClone =   [SELECT NE__ProdId__c, NE__Status__c, NE__Action__c, NE__CatalogItem__c, NE__Qty__c, NE__RecurringChargeFrequency__c
                                                FROM NE__OrderItem__c
                                                WHERE NE__OrderId__c = :orderId ORDER BY CreatedDate DESC][0];
                    
                    
               try{     
                
                if (myOrderToClone != null && myOrdItToClone != null) {
                newQuote = myOrderToClone.clone();
                system.debug('newQuote>>>' + newQuote);
                newQuote.RecordTypeId = CNT_CaseUtility.getRecordTypeID('ANY_ne_order_Quote');
                newQuote.NE__Type__c = 'InOrder';
                newQuote.CNT_Case__c = caseId;
                newQuote.NE__OrderStatus__c = 'Pending';
                newQuote.CNT_ContractStatus__c = 'Pending';
                newQuote.CNT_isOwnershipChange__c = True;//duvida
                newQuote.NE__AccountId__c = accountId;
                newQuote.NE__BillAccId__c = accountId;
                newQuote.NE__ServAccId__c = accountId;
                insert newQuote;
        
                NE__OrderItem__c newOrdIt = myOrdItToClone.clone();
                
                if(Test.IsRunningTest()){
                    List<RecordType> orderItemRT = [SELECT Id FROM RecordType WHERE SObjectType ='NE__OrderItem__c' LIMIT 1];
                    if(orderItemRT.size() > 0) newOrdIt.RecordTypeId = orderItemRT[0].Id;
                }

                system.debug('newQuote>>>' + newOrdIt);
                newOrdIt.NE__OrderId__c = newQuote.Id;
                newOrdIt.NE__Account__c = accountId;
                newOrdIt.NE__Billing_Account__c = accountId;
                newOrdIt.NE__Service_Account__c = accountId;
                newOrdIt.NE__OneTimeFeeOv__c = 0;
				newOrdIt.NE__Action__c = 'Add';
                insert newOrdIt;
                //ver los estados del Order Item
				
                    List <NE__Order_Item_Attribute__c> newOrderItemAttrs = new List <NE__Order_Item_Attribute__c> ();
                    List <NE__Order_Item_Attribute__c> listOrderItem = new List <NE__Order_Item_Attribute__c> ();
                
        
                Set<String> names = new Set<String> { 'Carga KW BR', 'Categoria de Tarifa BR', 'Nivel de Tensão BR', 'Tipo de Tensão BR', 'Valor de Tensão BR' };
                  
                   //Inicio Correção PRB121264 - ENGDB
                    listOrderItem = [SELECT NE__Order_Item__c, Name, NE__Value__c, NE__Old_Value__c, NE__FamPropId__c, NE__FAMPROPEXTID__c FROM NE__Order_Item_Attribute__c WHERE NE__Order_Item__c = :myOrdItToClone.id order by name]; List<NE__Order_Item_Attribute__c> lstOrderByRule = new List<NE__Order_Item_Attribute__c>();
                    	Map<Integer, NE__Order_Item_Attribute__c> mapOrderItem = new Map<Integer, NE__Order_Item_Attribute__c>();
                   		 Integer contador = 4;
                    		for (NE__Order_Item_Attribute__c item : listOrderItem) {
                    			if(item.Name == 'Classe BR'){
                   				 mapOrderItem.put(1, item);
                    			}else if(item.Name == 'SubClasse BR'){
                    					mapOrderItem.put(2, item);
                   				}else if(item.Name == 'Categoria de Tarifa BR'){
                   						 mapOrderItem.put(3, item);
                    			}else{
                    					mapOrderItem.put(contador, item);
                                        contador++;
                    }
                    
                        } 		for (Integer i = 1; i <= mapOrderItem.size(); i++) {
                    			if(mapOrderItem.containsKey(i)){
                    				lstOrderByRule.add(mapOrderItem.get(i));
                               }
                            }
                    //FIM Correção PRB121264 - ENGDB


                boolean changeSubclass = false;
                 //lista ordenada usada no for lstOrderByRule PRB121264 - ENGDB
                  for (NE__Order_Item_Attribute__c sourceItemAttr : lstOrderByRule) {
            
                    NE__Order_Item_Attribute__c newAtt = new NE__Order_Item_Attribute__c();
            
                    newAtt.NE__Order_Item__c = newOrdIt.id;
                    newAtt.Name = sourceItemAttr.Name;
                    newAtt.NE__Value__c = sourceItemAttr.NE__Value__c;
                    newAtt.NE__Old_Value__c = sourceItemAttr.NE__Old_Value__c;
                    
					if(names.contains(sourceItemAttr.Name) &&  String.isBlank(sourceItemAttr.NE__Value__c))
					{
                         return null; 
                    }
                    //Inicio Correção PRB121264 - Jefferson Rocha ENGDB
                    if(sourceItemAttr.Name.equals('Classe BR')){
                        if(sourceItemAttr.NE__Value__c.equals('40 - Rural') || sourceItemAttr.NE__Value__c.equals('41 - Rural C/ICMS') || sourceItemAttr.NE__Value__c.equals('11 - Residencial Baixa Renda') || String.isBlank(sourceItemAttr.NE__Value__c) || sourceItemAttr.NE__Value__c.equals('10 - Residencial'))
                    //FIM Correção PRB121264 - Jefferson Rocha ENGDB
                        {
                            sourceItemAttr.NE__Value__c = '10 - Residencial';
                            newAtt.NE__Old_Value__c = sourceItemAttr.NE__Value__c;
                            newAtt.NE__Value__c = '10 - Residencial'; // remove
                            changeSubclass = true;
                        }
                        
                        // 2020-12-28 BIA-REQ 50
                        
                        if(NEValues.contains(sourceItemAttr.NE__Value__c) && b2cValidation){
                            newAtt.NE__Old_Value__c = sourceItemAttr.NE__Value__c;
                            newAtt.NE__Value__c = '10 - Residencial';
                            sourceItemAttr.NE__Value__c = '10 - Residencial';
                            changeSubclass = true;
                        }
                    	// 2020-12-28 BIA-REQ 50
                       
                    }
                    if(sourceItemAttr.Name.equals('SubClasse BR')) 
                    {
                        if(changeSubclass || String.isBlank(sourceItemAttr.NE__Value__c)){
                            sourceItemAttr.NE__Value__c = 'REPLN - Residencial Pleno';
                            newAtt.NE__Old_Value__c = sourceItemAttr.NE__Value__c;  
                            newAtt.NE__Value__c = 'REPLN - Residencial Pleno';
                        }
                        // 2020-12-28 BIA-REQ 50
                        if(b2cValidation && changeSubclass){
                           	newAtt.NE__Old_Value__c = sourceItemAttr.NE__Value__c;
                            sourceItemAttr.NE__Value__c = 'REPLN - Residencial Pleno';
                            newAtt.NE__Value__c = 'REPLN - Residencial Pleno';
                              
                        }
                        // 2020-12-28 BIA-REQ 50
                    }

                    // 2020-12-29 BIA-REQ 50
                    if(sourceItemAttr.Name.equals('Categoria de Tarifa BR'))  
                    {
                        if(b2cValidation  && changeSubclass){
                            system.debug('Entrou na Tarifa BR');
                            newAtt.NE__Old_Value__c = sourceItemAttr.NE__Value__c;
                            sourceItemAttr.NE__Value__c = 'B1_RESID - Categoria de tarifa B1 residencial';
                            newAtt.NE__Value__c = 'B1_RESID - Categoria de tarifa B1 residencial';
                        }
                    }    
                    
                    newAtt.NE__FamPropId__c = sourceItemAttr.NE__FamPropId__c;
                    newAtt.NE__FAMPROPEXTID__c = sourceItemAttr.NE__FAMPROPEXTID__c;
            
                    newOrderItemAttrs.add(newAtt);
                  }
            
                  insert newOrderItemAttrs;
                 
                  }
                  else{
                        result.put('resultado','false');
                 }
                 result.put('resultado',newQuote.id);	
                  
             }
             catch(Exception ex){
                   result.put('resultado','false'); 
             }
           }
    
            return newQuote.Id; 
}


public static Map<String, String> getNewOwnerChangeThree(Id idCase, Case myCase, String newOrderId) 
{
        System.debug(':: Runnig New Owner Change 3 >>> ');
        RecordType  quotert = [SELECT Id FROM RecordType WHERE SObjectType = 'NE__Order__c' AND Name = 'Quote' WITH SECURITY_ENFORCED];

        NE__Order__c conf = new NE__Order__c();
        conf.id = newOrderId;
        conf.RecordTypeId = quotert.Id;
        
		if (!Schema.sObjectType.NE__Order__c.fields.RecordTypeId.isUpdateable()) {
            return null;
        }
        update conf;

        String orderResult = CNT_ConfigurationUtility.gerateOrder(idCase);

        Case resultCase = myCase;
        String changeType = resultCase.CNT_Change_Type__c;
        String tipotAjuste = resultCase.CNT_Tipo_de_Ajuste__c;
        String validGerarContrato = 'true';
		
        if ( validGerarContrato == 'true') 
		{
            boolean isProductChanged = false;
            NE__Order__c neOrder = [SELECT Id, NE__Type__c FROM NE__Order__c WHERE Id =: newOrderId WITH SECURITY_ENFORCED LIMIT 1];
            Id neOrderId = neOrder.Id;
            
			String neOrderType = neOrder.NE__Type__c;

            String resultOrderValid = '';//CNT_ConfigurationUtility.OrderValidations(NEOrderId);
            
			if (resultOrderValid.contains('ERRO')) 
			{
                return MessageToast.getMessage('', '', resultOrderValid);
            }
			else 
			{
                System.debug('No ERROR');
                if (neOrderType != 'ChangeOrder' || 
                    isProductChanged == true || 
                    changeType == '4-Contrato Geração Distribuída' ||
                    changeType == '5-Manutenção Iluminação Pública' ||
                    changeType == '7-Prorrogação Ligação Provisória' ||
                    tipotAjuste == 'Adesão/Exclusão Tarifa Branca' ||
                    changeType == '9-Energia Adicional'
                    ) 
					{				

						List<Case> lstCasos = [select id from case where CNT_Quote__c =:neOrderId and id !=:idCase];
						List<Case> objs = new List<Case>();
						
						for (Case item : lstCasos)
						{
							item.CNT_Quote__c = null;
							objs.add(item);
						}
						
						update objs;
						
                        NE__Order__c conf1 = new NE__Order__c();
                        conf1.Id = neOrderId;
                        conf1.CNT_ContractStatus__c = 'In Progress';
                        
						if (!Schema.sObjectType.NE__Order__c.fields.CNT_ContractStatus__c.isUpdateable()) 
						{
                            return null;
                        }
                        update conf1;
						
						system.Debug('Estamos aqui: ' + neOrderId);
						
                        CNT_Utility.ButtonResponse objResponse = CNT_ConfigurationUtility.updateContractAndCase(neOrderId);                                                
                        
                }
            }
        } 
		else 
		{
            return MessageToast.getMessage('',null, validGerarContrato);
        }

        return null;
    }