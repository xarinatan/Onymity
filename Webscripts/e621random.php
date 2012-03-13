<html>
<body>
<a href=e621random.php >Next image!</a>
<?php
$socket = socket_create(AF_INET,SOCK_STREAM,SOL_TCP);
if (!$socket) {
  echo "Failed to create socket.";
} else {
if(socket_connect($socket,"192.168.5.11",5000)){
    socket_send($socket, mb_convert_encoding("interactsteam\nInternet\nSOME_ID\nPM\nrecommendsmut","UTF-16"), 500, 0);
	socket_recv($socket,$cheese,2048,MSG_WAITALL);
	$apples = str_replace("\0",'',mb_convert_encoding(trim($cheese),"ASCII", "auto")); //ony send a shitload of nulls that confuse preg_match. 
	preg_match('#http://(.*)\.(gif|jpg|png)#siu', $apples ,$matches);
	echo "<iframe src=\"".$matches[0]."\" width=100% height=100% frameborder=0> Your browser doesn't support iframes. I need those otherwise e621 won't grant me the images.</iframe>";
  }
  else
  {
  echo socket_strerror(socket_last_error($socket));
  }
}
?>

</body>
</html>