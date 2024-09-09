List<String> lstCasoTexto =  new List<String>{{0}};
    public final String query;
    public Set<String> setCaseId = new Set<String> ();
    List<String[]> batches;
    private String cod_uc {get;set;}
    private String[] csv{get;set;}   
    public List<CNT_Contracting_Status_BO__c> listBackOffice = new List<CNT_Contracting_Status_BO__c> ();
    public Map<Id, Case> casesMapCnt = new Map<Id, Case> ();
    public List<Id> lstCaseCompleteOrder  = new List<Id>(); 
    
    public List<Contract> updateContractList = new List<Contract> ();
    public List<WorkOrder> listWorkOrder = new List<WorkOrder> ();
    Set<Id> setIdCase = new Set<Id> ();
    private String idAgente {get;set;}
    private Map<String,String> codUcXcsvLine = new Map<String,String>();
    private Map<String,PointofDelivery__c> PointsofDelivery = new Map<String,PointofDelivery__c>();
    private Map<String,String> idAgenteXcsvLine = new Map<String,String>();
    private Map<String,String> podIdXestadoCadastral = new Map<String,String>();
    private Map<String,Csv_ForMessage> podForMessageXCsvForMessage = new Map<String,Csv_ForMessage>();
    private Map<String,String> podForMessageXestadoCadastral = new Map<String,String>();
    private Set<String> PointOfDeliveryNamesForMessage = new Set<String>();
    private Set<String> idAgenteForMessage = new Set<String>();
    private Map<String,String> podIdXnis = new Map<String,String>();
    private Map<String,String> podIdXsubc = new Map<String,String>();
    private BaixaRendaIntegracao.DML dml = new BaixaRendaIntegracao.DML();
    private Boolean cadastramento = false; //cadastro - ver cadastramento
    List<Id> casesID = new List<Id>();
	private List < Case > ListCaseToUpdate = new  List < Case >();
	private List < Case > ListCaseQuery = new  List < Case >();
    private List<Case> upCsBypass = new List<Case>();
    private Set<Case> CompleteCases = new Set<Case>();
    
	ListCaseQuery = [SELECT id, casenumber
            FROM Case 
            WHERE CNT_Tipo_de_Ajuste__c = 'Adesão/Exclusão Baixa Renda'             
            AND CNT_Cliente_Baixa_Renda__c = True 
            AND (Status = 'CNT0002') 
            AND casenumber in :lstCasoTexto];       
    
        Set<Id> listIdCases = new Set<Id>();
        
		System.debug('Lista de casos: ' + ListCaseQuery);
		
		for(Case cs : ListCaseQuery)
		{
			System.debug('Caso número: '+ cs.casenumber);
            listIdCases.add(cs.id);
        }
        
        dml = BaixaRendaIntegracao.callIntegrations(listIdCases);
		
        ListCaseToUpdate.addall(dml.updateMapCase.values());
        ListCaseToUpdate.addall(dml.updateMapCaseError.values());
        upCsBypass.addall(dml.upCsBypass);
        listWorkOrder.addall(dml.listWorkOrder);
        listBackOffice.addall(dml.listBackOffice);
        CompleteCases.addall(dml.updateMapCase.values());
		
		System.debug('Batch finalizado.');
		
		if(ListCaseToUpdate.size()>0)
		{
			update ListCaseToUpdate;
		}
		
		if(listWorkOrder.size()>0)
		{
			insert listWorkOrder;
		}
		
		if(listBackOffice.size()>0)
		{
			insert listBackOffice;
		}
		
		if(!upCsBypass.isEmpty())
		{
			update upCsBypass;
		}
      
		for (Case cs: CompleteCases)
		{
            lstCaseCompleteOrder.add(cs.Id);
        }
      
		try
		{
			System.debug('Lista do complete : '+  lstCaseCompleteOrder);
			CNT_ConfigurationUtility.completeOrderFlow(lstCaseCompleteOrder);
		}
		catch(Exception ex)
		{
			System.debug('Erro : '+  ex);
			//CNT_ConfigurationUtility.completeOrderList(listIdOrder);
		}
		
		//CNT_CompleteOrderBatch sch = new CNT_CompleteOrderBatch(lstCaseCompleteOrder);
		//Database.executebatch(sch, 4);    