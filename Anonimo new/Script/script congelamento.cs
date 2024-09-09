List<string> usuarios = new List<string>();

List<User> lista= new List<User>();


lista = [select Id, username from User where username in ('aila.colares@enel.com.preprod',
'francisco.ailson@enel.com.preprod',
'hallisson.montenegro@enel.com.preprod',
'nildo.falcao@enel.com.preprod')];

for(User u : lista)
{
	try
	{	
		User lol = new User();
		lol.isActive = true;
		lol.id = u.id;
		update(lol);

		System.debug(Processando usuário  +  u.username);
		System.setPassword(u.id,'Enel@2023'); 
	
	
		List<UserLogin> users = [SELECT IsFrozen,UserId FROM UserLogin WHERE UserId = :u.id];
	       
		for (UserLogin ul : users) { ul.isFrozen = false; }
	        update(users);
	}
	Catch(Exception ex)
	{
		System.debug(Erro usuário  +  u.username +   + ex);	
		continue;
	}
}