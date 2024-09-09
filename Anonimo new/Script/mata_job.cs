List<AsyncApexJob> lista =  new List<AsyncApexJob>();

lista = [select id from AsyncApexJob where apexclass.name ='CorrectingCaseSharingBatch' and status in ('Queued','Processing') limit 75];

for (AsyncApexJob item : lista){
System.AbortJob(item.Id);
}