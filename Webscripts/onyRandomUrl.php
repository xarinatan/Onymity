<html>
<body>
<a href=onyRandomUrl.php >Next link!</a>
<?php

$socket = socket_create(AF_INET,SOCK_STREAM,SOL_TCP);
if (!$socket) {
  echo "Failed to create socket.";
} else {
if(socket_connect($socket,"192.168.5.11",5000)){
    socket_send($socket, mb_convert_encoding("interactsteam\nInternet\nSOME_ID\nPM\nrecommendurl","UTF-16"), 500, 0);
	$cheese = "D:";
	socket_recv($socket,$cheese,2048,MSG_WAITALL);
	$apples = str_replace("\0",'',mb_convert_encoding(trim($cheese),"ASCII", "auto"));
	preg_match('#(mailto\:|(news|(ht|f)tp(s?))\://){1}\S+#siu', $apples ,$matches);
	echo "<br><a href=\"".$matches[0]."\" target=_blank>Original link</a>";
	if(strpos($matches[0],"youtube.com") == false && strpos($matches[0],"youtu.be") == false)
	{
	echo "<iframe src=\"".$matches[0]."\" width=100% height=90% frameborder=0> Your browser doesn't support iframes. I need those otherwise e621 won't grant me the images.</iframe>";
	}
	else
	{
	echo "<br>".parse_youtube_url($matches[0],'embed');
	}
  }
  else
  {
  echo socket_strerror(socket_last_error($socket));
  }
}

//added this to deal with youtube vids. Credits go to person mentioned below.
/*
* parse_youtube_url() PHP function
* Author: takien
* URL: http://takien.com
*
* @param string $url URL to be parsed, eg:
* http://youtu.be/zc0s358b3Ys,
* http://www.youtube.com/embed/zc0s358b3Ys
* http://www.youtube.com/watch?v=zc0s358b3Ys
* @param string $return what to return
* - embed, return embed code
* - thumb, return URL to thumbnail image
* - hqthumb, return URL to high quality thumbnail image.
* @param string $width width of embeded video, default 560
* @param string $height height of embeded video, default 349
* @param string $rel whether embeded video to show related video after play or not.
 
*/
 
function parse_youtube_url($url,$return='embed',$width='',$height='',$rel=0){
    $urls = parse_url($url);
   
    //url is http://youtu.be/xxxx
    if($urls['host'] == 'youtu.be'){
        $id = ltrim($urls['path'],'/');
    }
    //url is http://www.youtube.com/embed/xxxx
    else if(strpos($urls['path'],'embed') == 1){
        $id = end(explode('/',$urls['path']));
    }
     //url is xxxx only
    else if(strpos($url,'/')===false){
        $id = $url;
    }
    //http://www.youtube.com/watch?feature=player_embedded&v=m-t4pcO99gI
    //url is http://www.youtube.com/watch?v=xxxx
    else{
        parse_str($urls['query']);
        $id = $v;
        if(!empty($feature)){
            $id = end(explode('v=',$urls['query']));
        }
    }
    //return embed iframe
    if($return == 'embed'){
        return '<iframe width="'.($width?$width:560).'" height="'.($height?$height:349).'" src="http://www.youtube.com/embed/'.$id.'?rel='.$rel.'" frameborder="0" allowfullscreen></iframe>';
    }
    //return normal thumb
    else if($return == 'thumb'){
        return 'http://i1.ytimg.com/vi/'.$id.'/default.jpg';
    }
    //return hqthumb
    else if($return == 'hqthumb'){
        return 'http://i1.ytimg.com/vi/'.$id.'/hqdefault.jpg';
    }
    // else return id
    else{
        return $id;
    }
}
?>
<br>As much as I'd like to, I cannot guarantee everything in onymity's database is SFW, so please, use with caution.
</body>
</html>