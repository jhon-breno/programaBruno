Case casos = [SELECT Id, AssetRelation__r.id, AssetRelation__r.IsCompetitorProduct,
                                   SubCause__c, Account.Type, Account.Name, Account.CompanyID__c, Account.Phone, Account.PrimaryEmail__c,  
                                   Account.Country__c, Account.IdentityType__c, Account.IdentityNumber__c, Account.CNT_Account_Type__c, CompanyID__c,
                                   PointofDeliveryNumber__c, Contact.Name, Contact.FirstName, Contact.LastName, Contact.Email, Account.Address__r.Name, Account.Address__r.Number__c, Account.Address__r.Postal_Code__c, Account.Address__r.Municipality__c, CNT_Contract__r.ContractNumber, AssetRelation__r.PointofDelivery__r.PointofDeliveryNumber__c, 
                                   AssetRelation__r.InstallDate, AssetRelation__r.Contract__r.StartDate, AssetRelation__r.NE__UsageCode__c
                                   FROM Case where casenumber='{0}' limit 1];
								   

XC_VasDeactivation.vasDeactivation(casos);