List<String> casos = new List<String>{'{0}'};

List<Case> objs = [select id,accountid, contactid, pointofdelivery__c, PlannedEndDate__c, Observations__c,NumberOfOrder__c from case where casenumber in :casos];

for(Case obj : objs)
{
	WorkOrder work = new WorkOrder();

	work.NumberofPointofDelivery__c = obj.pointofdelivery__c;
	work.accountid = obj.accountid;
	work.contactid = obj.contactid;
	work.DueDate__c = obj.PlannedEndDate__c.addDays(5);
	work.Observations__c = obj.Observations__c;
	work.Order_Number_BE__c = obj.NumberOfOrder__c;
	work.caseid = obj.id;
	work.recordtypeid= '0121o000000oUAiAAM';
    insert work;
}
