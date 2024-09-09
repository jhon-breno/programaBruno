// Enviar ALTERAÇÃO CONTRATUAL SAP massivamente ate 50 registros
List<String> caseNumberList = new List<String>{'{0}'};

CNT_IntegrationHelper.CCC_BR_242 reqBaja = new CNT_IntegrationHelper.CCC_BR_242();

CNT_Contracting_Status_BO__c backOffice = new CNT_Contracting_Status_BO__c();
Map<String,Object> resultBackOffice = new Map<String,Object>();

List<id> lst_id = new List<id>();
	
List<Contract> listCaso = new List<Contract>();

listCaso = [select Id,  RecordType.DeveloperName,
                  Account.CompanyID__c, Description, CNT_Quote__c,                   
                  ContractNumber, CNT_ExternalContract_ID_2__c,
                  CNT_Public_Ilumination__c, CNT_Free_Client__c, StartDate, Status
                  From Contract Where CNT_ExternalContract_ID_2__c =: caseNumberList];

for(Contract c : listCaso)
{
		try
		{
				String clienteNumber = (!String.isBlank(c.CNT_ExternalContract_ID_2__c))? c.CNT_ExternalContract_ID_2__c: c.ContractNumber;
								
				CNT_IntegrationDataBR.AttributeBR att = CNT_IntegrationDataBR.getAttributes(c.CNT_Quote__c);
				
				String codSistema;
				if (c.Account.CompanyID__c == '2005')
				{
					CodSistema = 'AMPSAP';
				}
				else
				{
					CodSistema = 'COESAP';
				}
        
				String changeType = '5';
				
				System.debug('CHANGE TYPE >>>>>>>' + changeType);
				
				//prepare data
				CNT_IntegrationHelper.CCC_BR_242 req = new CNT_IntegrationHelper.CCC_BR_242();
				
				CNT_IntegrationHelper.Header reqHeader = new CNT_IntegrationHelper.Header();
				reqHeader.CodSistema = codSistema;
				reqHeader.SistemaOrigen = 'SFDC';
				reqHeader.FechaHora = date.today();
				reqHeader.Funcionalidad = 'Troca Contrato';
				
				CNT_IntegrationHelper.BodyCCC_242 reqBody = new CNT_IntegrationHelper.BodyCCC_242();
				
				reqBody.INSTALLED_LOAD = '';
				
				reqBody.TIPOOPERAZIONE = 'CHANGE';
				reqBody.ID_RICHIESTA = '';
				reqBody.ID_FO = '18071987';
				reqBody.PARTNER = clienteNumber;
				reqBody.ANLAGE = clienteNumber;
				reqBody.TARIFTYP = att.Tarifa;
				reqBody.AC_KOFIZ_SD = att.Classe;
				


				List<CNT_IntegrationHelper.ListDistributedsGeneration> distributedList = new List<CNT_IntegrationHelper.ListDistributedsGeneration>();
				CNT_IntegrationHelper.ListDistributedsGeneration podDistributed = new CNT_IntegrationHelper.ListDistributedsGeneration();
				podDistributed.RATIO = '';
				podDistributed.ANLAGE_RATIO = '';
				distributedList.add(podDistributed);
				reqBody.LISTDISTRIBUTEDSGENERATION = distributedList;

				// Iluminação pública - Wanderson Dantas
				List<CNT_IntegrationHelper.ListIluminacaoPublica> iluminacaoList = new List<CNT_IntegrationHelper.ListIluminacaoPublica>();
				CNT_IntegrationHelper.ListIluminacaoPublica caseIlumPublica = new CNT_IntegrationHelper.ListIluminacaoPublica();
				
				if (c.CNT_Public_Ilumination__c) 
				{
					System.debug(':: CNT_Public_Ilumination__c >>> '+c.CNT_Public_Ilumination__c);
					List<CNT_Lamp__c> lamps = [SELECT id, CNT_Quantity__c, CNT_Lamp_Code__c, CNT_Charge__c, CNT_Totalizer__c, CNT_Loss__c FROM CNT_Lamp__c WHERE CNT_Contract__c =: c.id];
					Map<Decimal, CNT_Lamp__c> mapLamps = new Map<Decimal, CNT_Lamp__c>();
					
					for(CNT_Lamp__c lamp: lamps) 
					{
						if(lamp.CNT_Quantity__c > 0 && !mapLamps.containsKey(lamp.CNT_Lamp_Code__c)){
							mapLamps.put(lamp.CNT_Lamp_Code__c, lamp);
						}
					}
					System.debug('mapLamps --> '+mapLamps);

					for(CNT_Lamp__c lamp: mapLamps.values()) 
					{
						caseIlumPublica = new CNT_IntegrationHelper.ListIluminacaoPublica();
						caseIlumPublica.CODE_TYPE = String.valueOf(lamp.CNT_Lamp_Code__c);
						Decimal podency = 0.00;
						
						if(lamp.CNT_Charge__c > 0 )
						{
							podency =  lamp.CNT_Charge__c.divide(1000, 8);
						}

						caseIlumPublica.POTENCY = String.valueOf(podency);
						caseIlumPublica.FACTOR = String.valueOf(lamp.CNT_Loss__c);
						caseIlumPublica.QUANTITY = lamp.CNT_Totalizer__c;
						iluminacaoList.add(caseIlumPublica);
					}
					reqBody.ILUMI_PUB = iluminacaoList;
				} 
				else 
				{
					caseIlumPublica.CODE_TYPE = '0';
					caseIlumPublica.POTENCY = '0';
					caseIlumPublica.FACTOR = '0';
					caseIlumPublica.QUANTITY = 0;
					iluminacaoList.add(caseIlumPublica);
					reqBody.ILUMI_PUB = iluminacaoList;
				}
				System.debug(':: LISTILUMINACAOPUBLICA>>> '+reqBody.ILUMI_PUB);
				
				reqBody.TEMP_AREA = att.SubClasse;
				if(c.CNT_Free_Client__c)
				{
					reqBody.VENDE = String.valueOf(c.StartDate);
				}
				else
				{					
					reqBody.VENDE = String.valueOf(System.Today());					
				}

				if (c.CNT_Free_Client__c){ //cliente livre
					// changed artur.miranda 12-06-2018 (Verified with SAP e Mulesoft) 
					if(String.isNotBlank(att.ModalidadeTarifaria) && att.ModalidadeTarifaria.contains('Azul')){
						reqBody.DI_CONTPTL = String.valueOf(att.DemandaPonta);
						reqBody.DI_CONTFPL = String.valueOf(att.DemandaForaPonta);
						reqBody.DI_CONTUL = '';
						reqBody.DI_CONTRPT = 0;
						reqBody.DI_CONTRFP = 0;
						reqBody.DI_CONTRAT = 0;
					}else if(String.isNotBlank(att.ModalidadeTarifaria) && att.ModalidadeTarifaria.contains('Verde')){
						reqBody.DI_CONTUL = String.valueOf(att.Demanda);
						reqBody.DI_CONTPTL = '';
						reqBody.DI_CONTFPL = '';
						reqBody.DI_CONTRPT = 0;
						reqBody.DI_CONTRFP = 0;
						reqBody.DI_CONTRAT = 0;
					}
					reqBody.IM_DI_CONTRAT = 0;
					reqBody.IM_DI_CONTRPT = 0;
					reqBody.IM_DI_CONTRFP = 0;
					reqBody.IM_DI_CONTUL = 0;
					reqBody.IM_DI_CONTFPL = 0;
					reqBody.IM_DI_CONTPTL = 0;
				}else{
					if(String.isNotBlank(att.ModalidadeTarifaria) && att.ModalidadeTarifaria.contains('Azul')){
						reqBody.IM_DI_CONTRPT = att.DemandaPonta;
						reqBody.IM_DI_CONTRFP = att.DemandaForaPonta;
						reqBody.IM_DI_CONTRAT = 0;
						reqBody.IM_DI_CONTUL = 0;
						reqBody.IM_DI_CONTFPL = 0;
						reqBody.IM_DI_CONTPTL = 0;
					} else if(String.isNotBlank(att.ModalidadeTarifaria) && att.ModalidadeTarifaria.contains('Verde')){
						reqBody.IM_DI_CONTRAT = att.Demanda;
						reqBody.IM_DI_CONTRPT = 0;
						reqBody.IM_DI_CONTRFP = 0;
						reqBody.IM_DI_CONTUL = 0;
						reqBody.IM_DI_CONTFPL = 0;
						reqBody.IM_DI_CONTPTL = 0;
					} else if((String.isNotBlank(att.ModalidadeTarifaria) && att.ModalidadeTarifaria.contains('OPTANTE')) || att.Grupo == 'B'){
						reqBody.IM_DI_CONTRAT = 0;
						reqBody.IM_DI_CONTRPT = 0;
						reqBody.IM_DI_CONTRFP = 0;
						reqBody.IM_DI_CONTUL = 0;
						reqBody.IM_DI_CONTFPL = 0;
						reqBody.IM_DI_CONTPTL = 0;
					} else {
						reqBody.IM_DI_CONTRAT = 0;
						reqBody.IM_DI_CONTRPT = 0;
						reqBody.IM_DI_CONTRFP = 0;
						reqBody.IM_DI_CONTUL = 0;
						reqBody.IM_DI_CONTFPL = 0;
						reqBody.IM_DI_CONTPTL = 0;
					}
					
					reqBody.DI_CONTPTL = '';
					reqBody.DI_CONTFPL = '';
					reqBody.DI_CONTUL = '';
					reqBody.DI_CONTRPT = 0;
					reqBody.DI_CONTRFP = 0;
					reqBody.DI_CONTRAT = 0;
				}
				
				
				if(changeType == '1') { reqBody.CAUSALE = 'TARIFF'; }
				if(changeType == '2'){
					reqBody.CAUSALE = 'LOAD'; 
					reqBody.INSTALLED_LOAD= String.valueOf(att.Carga);
					reqBody.IM_DI_CONTPTL = 0;
					reqBody.IM_DI_CONTFPL = 0;
					reqBody.IM_DI_CONTRPT = 0;
					reqBody.IM_DI_CONTRFP = 0;
					reqBody.IM_DI_CONTRAT = 0;
					reqBody.IM_DI_CONTUL = 0;
					reqBody.INSTALLED_PHASE = att.TipoTensao;       //juan passando tipo de tensao pro sap
				}
				if(changeType == '3'){ reqBody.CAUSALE = 'DEMAND';}
				if(changeType == '6'){  //Acresc Decresc de Potencia
					if(att.ChangeLoad == 'Change'){ // se houver mudança de carga 
						reqBody.INSTALLED_LOAD= String.valueOf(att.Carga);
					}
					
					if(att.ActionTarifa == 'Change'){ // houve troca de tarifa
						reqBody.CAUSALE = 'TARIFF';
						
					}else{
						reqBody.CAUSALE = 'LOAD';
					}
				}// Aumento de Potencia
				if(changeType == '5'){ reqBody.CAUSALE = 'ILLUMINATION';}// Iluminação Pública - Wanderson Dantas 17-09-2018
				if(changeType == '7'){ reqBody.CAUSALE = 'INTERIM';}//Ligação provisoria - Madson Felipe 18-12-2018

				req.Header = reqHeader;
				req.Body = reqBody;
        
        
		
			//SAP
			reqBaja = req;//CNT_IntegrationDataBR.flow_CCC_SAP_BR(caso.Id);//(caso.Id);
			reqBaja.Body.VENDE = String.valueOf(Date.Today());
			System.debug('JSON SAP: '+JSON.serialize(reqBaja));
			
			HttpResponse responseSAP = CNT_IntegrationHelper.CCC_242_BRSync(reqBaja,c.Id);
			System.debug('responseSAP: '+responseSAP);
			/*resultBackOffice = CNT_Utility.resultRequest(responseSAP, JSON.serialize(reqBaja), caso, 'Facturador');
			backOffice = (CNT_Contracting_Status_BO__c) resultBackOffice.get('backoffice');
			backOfficeList.add(backOffice);
			Boolean responseStatusSAP = ((Boolean) resultBackOffice.get('status'));*/
			
			/*if(responseStatusSAP)
			{
				caso.Status = 'CNT0008';
				caso.cnt_processStatus__c = '';
	        	caso.CNT_ByPass__c = true;
				updateListCase.add(caso);
				lst_id.add(caso.id);		
			}*/
		}
		catch(Exception ex)
		{
			continue;
		}
}

