			
				List<Id> relatedPodId = new List<Id> ();
				List<PointofDelivery__c> relatedPod;
				String numSuministro='';
				String CodigoEmpresa='';
				String entrada ='2994932';
				Billing_Profile__c perfildfact;
				List<Billing_Profile__c> perfildfactlist;  
				
				try
				{
					perfildfactlist = [SELECT AccountContract__c, Account__c,Account__r.AccountClientNumber__c, Account__r.name,Account__r.IdentityNumber__c,Account__r.IdentityType__c,
					PointofDelivery__c FROM Billing_Profile__c WHERE AccountContract__c = :entrada and company__c ='2003' 
					and PointofDelivery__r.SegmentType__c ='B'  LIMIT 1];
					
					perfildfact = perfildfactlist[0];					
					
					PointofDelivery__c sumSel = new PointofDelivery__c();
					sumSel = [SELECT Id,PointofDeliveryNumber__c, CompanyID__c FROM PointofDelivery__c WHERE Id =:perfildfact.PointofDelivery__c  LIMIT 1];
					numSuministro = sumSel.PointofDeliveryNumber__c; 
					CodigoEmpresa = sumSel.CompanyID__c;
				} 
				catch(Exception e)
				{ 
					system.debug(e.getMessage());
				} 
				
				Map<String, String> args = new Map<String,String>(); 
					
			
					//MAPEO CAMPOS BRASIL
					args.put('NumeroSuministro',numSuministro);
					args.put('CodigoEmpresa', '2003');    
					args.put('NumeroCaso',  '1');   					    
					          
					args.put('NumeroCuentaContrato',perfildfact.AccountContract__c);
					args.put('Instalacion',numSuministro);          		
					args.put('TipoServicioSAP', 'PARTNER');
					args.put('MotivoTipoServicio', 'MODIFY');
					args.put('NumeroCuentaCliente',perfildfact.Account__r.AccountClientNumber__c);
					args.put('NombreEmpresa',perfildfact.Account__r.name);
		
					args.put('Nombre', perfildfact.Account__r.name);
					args.put('NombreCompleto', perfildfact.Account__r.name);
                    args.put('NombrePersona', 'VERUSKA');
                    args.put('ApellidoPersona', 'MONICA TIMBO DE LIMA');
					args.put('NumeroDocumento', perfildfact.Account__r.IdentityNumber__c);
					args.put('CodigoFiscal', perfildfact.Account__r.IdentityNumber__c);
					args.put('TipoDocumento',perfildfact.Account__r.IdentityType__c);

					
					System.Debuf('XXXX>>>> ' + ATCL_VFC043_GeneralIntegration.invokePutSuministro(args,null)); 