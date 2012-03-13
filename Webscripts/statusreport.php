<html>
<body>
<?php
//Basic example of how to query ony using the standard API.
//A specific API for database querying is in the planning.
$socket = socket_create(AF_INET,SOCK_STREAM,SOL_TCP);
if (!$socket) {
  echo "Failed~";
} else 
{
if(socket_connect($socket,"192.168.5.11",5000))
	{
    socket_send($socket, mb_convert_encoding("interactsteam\nInternet\nSOME_ID\nPM\nstatusreport","UTF-16"), 500, 0);
	$cheese = "D:";
	socket_recv($socket,$cheese,2048,MSG_WAITALL);
	echo $cheese;
	}
  else
  {
  echo socket_strerror(socket_last_error($socket));
  }
}
?>
</body>
</html>