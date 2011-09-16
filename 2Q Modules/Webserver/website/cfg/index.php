<?
//AaronGET
$query = getenv("QUERY_STRING");
$m = strcasecmp( getenv("REQUEST_METHOD"), "GET" );
$t = 0;

if ( $m != 0 ) {
    $m = strcasecmp( getenv("REQUEST_METHOD"), "POST" );
    if ( $m == 0 ) { $t = 1; }
}

if ( $query != NULL ) {
    $tok1 = strtok($query, "=&");
    $tok2 = strtok( "=&" );

    while ( $tok1 != NULL ) {
        if ( $t == 0 ) { $_GET[$tok1] = $tok2; }
        elseif ( $t == 1 ) { $_POST[$tok1] = $tok2; }
        $tok1 = strtok( "=&" );
        $tok2 = strtok( "=&" );
    }
}
//AaronGET

$m_amount   = intval( getenv( "QQ_ENV_MNUM" ) );
$m_names    = ParseKey( getenv( "QQ_ENV_MNAMES" ), ";" );
$m_loaded   = ParseKey( getenv( "QQ_ENV_MLOADED" ), ";" );

function QuickModuleInfo(){
    $m_amount   = intval( getenv( "QQ_ENV_MNUM" ) );
    $m_names    = ParseKey( getenv( "QQ_ENV_MPRETTYNAMES" ), ";" );
    $m_loaded   = ParseKey( getenv( "QQ_ENV_MLOADED" ), ";" );
    for ( $i = 0; $i < $m_amount; $i++ ) {
        if ($m_names[$i] == true)
            echo "{$m_names[$i]}<br />";
        else
            echo "<div class=\"ModuleOffline\"> {$m_names[$i]} </div>";
        //printf("<tr><td>%s</td><td>%s</td></tr>", $m_names[$i], $m_loaded[$i] == "True" ? "Yes" : "No");
    }
}

function QuickChannelInfo(){
    
    $s_names = ParseKey( getenv( "QQ_ENV_SNAMES" ), ";" );
    $s_amount = getenv('QQ_ENV_SNUM');
    for ( $i = 0; $i < $s_amount; $i++ ){
        echo "<div class=\"ChannelServer\"><a href=\"?s=$i\">" . $s_names[$i] . "</a></div>";

            $a = getenv( "QQ_ENV_S". $i . "_CHANNELS");
            if ($a == null) break;
            
            $q = ParseKey( $a , ";");
            for ($g = 0; $g < sizeof($q); $g++){
                $r = $q[$g];
                echo "<a href=\"?s=$i&c=$g\">$r</a><br />";
            }

        echo "\n";
    }
}

function ChannelName( $c, $s ){
    $a = getenv( "QQ_ENV_S". $s . "_CHANNELS");
    $b = ParseKey( $a , ";");
    return $b[$c];
}

function ServerName( $s ) {
    $a = getenv( "QQ_ENV_SNAMES" );
    $b = ParseKey( $a , ";");
    return $b[$s];
}

function ListChannels( $server ){
    $q = "qq_env_s" . $server . "_channels";
    if ( getenv($q) == "" )
        return "Non-Existant Server.";
    else {
        $chans = "";
        $c = ParseKey( getenv($q), ";" );
        for ( $i = 0; $i < sizeof($c); $i++ ){
            $chans = $chans . "<a href=\"?s=" . $server . "&c=" . $i . "\">" . $c[$i] . "</a>";
        }
        return $chans;
    }
}

function ListUsers( $server, $channel ){
    $q = "qq_env_s" . $server . "_c" . $channel . "_names";
    
    if ( getenv($q) == "" )
        return "Incorrect Input!";
    else {
        $users = "";
        $u = ParseKey( getenv($q), ";" );
        if ( natsort($u) ) {
        for ( $i = 0; $i < sizeof($u); $i++ ){
            $users = $users . $u[$i] ."<br/>";
        }
        return $users;
        } else
        return "natsort";
    }
}

function ParseSemiColons( $string ) {
    $tmp = array();
    $tok = strtok($string,';');
    $i = 0;
    while ( $tok !== false ) {
        $tmp[$i++] = $tok;
        $tok = strtok(';');
    }
    return $tmp;
}

function natSortKey(&$arrIn )
{
    $key_array = array();
    $arrOut = array();
  
    foreach ( $arrIn as $key=>$value ) {
        $key_array[]=$key;
    }
    natsort( $key_array);
    foreach ( $key_array as $key=>$value ) {
        $arrOut[$value]=$arrIn[$value];
    }
    $arrIn=$arrOut;
    
}

function access_strcmp( $a, $b ) {     
    $oa = ord( $a );    $ob = ord( $b );      
    if ( $oa == 64 && $ob != 64 ) { return -1; }    
    if ( $oa != 64 && $ob == 64 ) { return 1; }      
    if ( $oa == 43 && $ob != 43 ) { return -1; }    
    if ( $oa != 43 && $ob == 43 ) { return 1; }      
    return strcmp( $a, $b);  
}

//PHP amazes me at it's retardism
function awkwardfunction( $a ){
    natcasesort($a);
    $b = array();
    $i = 0;
    foreach ( $a as $c ){
        $b[$i] = $c;
        $i++;
    }
    
    unset( $a );
    unset( $i );
    
    //TODO: sort by member access level or something :x
    
    usort( $b , "access_strcmp" );
    return $b;
}

function ParseKey( $string, $key) {
    $tmp = array();
    $tok = strtok($string,$key);
    $i = 0;
    while ( $tok !== false ) {
        $tmp[$i++] = $tok;
        $tok = strtok(';');
    }

    return awkwardfunction($tmp);
}

function getexists( $key ) {    return ( array_key_exists( $key, $_GET ) ) ? $_GET[$key] : NULL;  }

$s = getexists( "s" );
$c = getexists( "c" );

if ( ($s != "") && ($c != "") ){
    $page = 1; //Shows Users on Channel
    $users = ListUsers( $s, $c );
    $channel_name = ChannelName( $c, $s );
    $server_name = ServerName( $s );
}
else if ( $s != "" ){
    $page = 0; //Show Channels on Server
    $channels = ListChannels( $s );
    $server_name = ServerName( $s );
}
else {
    $page = 2; //Default
}
?>

<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
	<head>
		<!--Designed by Dylan Johnston-->
		<!--Monday, November 12, 2007 -->
		<meta http-equiv="Content-Type" content="text/html; charset=iso-8859-1" />
		<meta name="description" content="Project2Q, An opensource C# IRC Bot." />
		<title>Project2Q</title>
		<link rel="stylesheet" type="text/css" href="style.css" />
	</head>
	<body>
		<!--Outline Start-->
		<div class="OutLine">
		<!--Header Start-->
		<div class="Header">
			<div class="HeaderText">#Project2Q<br />irc.nuclearfallout.net</div>
		</div>
		<!--Header End-->
		<!--Project Information Start-->
		<div class="ProjectInformationWrapper">
			<!--Modules Start-->
			<div class="Modules">
			<!--Module Header start-->
			<div class="ModuleHeader"> Modules Installed </div>
			<!--Module Header End-->
			<? QuickModuleInfo(); ?>
			</div>
			<!--Modules End-->
			<!--Channels Start-->
			<div class="Channels">
			<!--Channel Header Start-->
			<div class="ChannelHeader"> Current Channels </div>
			<!--Channel Header End-->
			
			<? QuickChannelInfo(); ?>
			
			<!--Channels End-->
			</div>
			<!--Gap Start-->
			<div class="Gap">
			<!--Gap Header Start-->
			<div class="GapHeader">&nbsp;</div>
			<!--Gap Header End-->
			</div>
			<!--Gap End-->
		</div>
		<!--Project Information End-->
		<? 
		switch ( $page ){
        case 0:
            //SHOW CHANNELS ON A SERVER
            ?>
            <!--Information Start-->
            <div class="InformationWrapper">
            <!--Information Header Start-->
            <div class="InformationHeader"> Channels on <? echo $server_name; ?> </div>
            <!--Information Header End-->
            <? echo $channels; ?>
            </div>
            <!--Information End-->
            <?
            break;
        case 1:
            //SHOW USERS ON A CHANNEL
            ?>
            <!--Information Start-->
            <div class="InformationWrapper">
            <!--Information Header Start-->
            <div class="InformationHeader"> Users in <? echo $channel_name; ?> on <? echo $server_name; ?></div>
            <!--Information Header End-->
            <? echo $users; ?>
            </div>
            <!--Information End-->
            <?
            break;
        case 2:        
        default:
            //SHOW DEFAULT
            ?>
            <!--Information Start-->
            <div class="InformationWrapper">
            <!--Information Header Start-->
            <div class="InformationHeader"> Information </div>
            <!--Information Header End-->
            Project2Q is an modular irc bot written in C#.
            </div>
            <!--Information End-->
            <?
            break;		
		}
		?>
		<!--Bottom Start-->
		<div class="Bottom">
			<!--Bottom Text Start-->
			<div class="BottomText"> <a href="mailto: aaronl_d@hotmail.com" title="Aaron Lefkowitz">Aaron Lefkowitz</a> & <a href="mailto: dylanjohnston@gmail.com" title="Dylan Johnston"> Dylan Johnston </a> </div>
			<!--Bottom Text End-->
		</div>
		<!--Bottom End-->
		</div>
		<!--OutLine End-->
	</body>
</html>
