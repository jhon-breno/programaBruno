List<id> caseIds = new List<id>{{0}};	

Id prioridade = Database.executeBatch(new CNT_CompleteOrderBatch(caseIds), 5);

FlexQueue.moveJobToFront(prioridade);