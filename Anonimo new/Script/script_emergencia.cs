 ID jobID;
 List<Id> listaCasosId = new List<Id>{{0}}; 
 
 
 jobID = System.enqueueJob(new ECH_VFC040_CreateAlertInservice(listaCasosId));
 
 