//String dado  = '39823558>2005';
String dado  = '{0}';

RestRequest request = new RestRequest();
request.httpMethod = 'POST';
request.addParameter('CanalAtendimento', 'AUTO');
RestContext.request = request;

BillingCreateCaseAPI oi = new BillingCreateCaseAPI();
BillingCreateCaseAPI.Response lal = BillingCreateCaseAPI.createBilling(dado.split('>')[1],'','','ENVIARSAP',dado.split('>')[0]);

Case obj = [select id from case where casenumber=:dado.split('>')[0]];
obj.status = 'ESTA007';

upsert obj;