@{
    sqlserver = 'SERVER=localhost,11433;DATABASE=tempdb;USER ID=sa;Password=Password12!'
    postgres = 'SERVER=localhost;PORT=5432;DATABASE=jerry_testdb;USER ID=jerry;PASSWORD=Password12!;ENLIST=true'
    mysql = 'SERVER=localhost;DATABASE=jerry_testdb;UID=jerry;PWD=Password12!'
    oracle = 'DATA SOURCE=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=localhost)(PORT=1521))(CONNECT_DATA=(SERVER=DEDICATED)(SERVICE_NAME=ORCLCDB.localdomain)));USER ID=sys;PASSWORD=Oradoc_db1;DBA Privilege=SYSDBA'
    user = @{
        oracle = 'DATA SOURCE=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=localhost)(PORT=1521))(CONNECT_DATA=(SERVER=DEDICATED)(SERVICE_NAME=ORCLCDB.localdomain)));USER ID=jerryuser;PASSWORD=Password12'
    }
}