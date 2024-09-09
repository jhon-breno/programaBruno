select * from asset b, pointofdelivery__c c 
left join account a on b.accountid = a.id
Where 
b.pointofdelivery__c = c.id
and c.Segmenttype__c ='A'	
and c.companyid__c in ('2003','2005');

select * from asset b, pointofdelivery__c c, serviceproduct__c x 
left join contact a on x.contact__c = a.id
Where 
b.pointofdelivery__c = c.id
and x.asset__c = b.id
and c.Segmenttype__c ='A'
and c.companyid__c in ('2003','2005');

select * from pointofdelivery__c where pointofdeliverynumber__c='521500'
and companyid__c ='2003';

select * from asset where pointofdelivery__c='a0B3600000IpsADEAZ';

select * from account where id='001360000129MdgAAE';


select id, firstname,  lastname, identitytype__c, identitynumber__c
from contact 
where companyid__c ('2003','2005') limit 10;

select 
a.id,
a.externalid__c, 
a.firstname,  
a.lastname, 
a.identitytype__c, 
a.identitynumber__c, 
a.phone, 
a.additionalphone__c, 
a.secondaryphone__c,
a.mobilephone,
a.email,
a.secondaryemail__c,
a.user_facebook_name__c,
a.lastmodifieddate
from contact a
where a.companyid__c in ('2003','2005') limit 1000;