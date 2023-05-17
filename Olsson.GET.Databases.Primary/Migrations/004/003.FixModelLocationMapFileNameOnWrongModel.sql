update dbo.models set 
LocationMapFileName = 'relateMat.txt'
where id = 1;

update dbo.models set 
LocationMapFileName = null
where id = 2;