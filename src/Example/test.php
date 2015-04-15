<html>
<head>
  <title>PHP Test page</title>
</head>
<body>

<?PHP

    echo "<pre><h2>Sharpserver and Phalanger have been configured succesfully!</h2><p></pre>";

    echo date('l jS \of F Y h:i:s A');

    echo "<pre>";
    foreach($_SERVER as $key_name => $key_value) 
    {
        echo $key_name . " = " . $key_value . "<br>";
    }
    echo "</pre>";

 ?>

</body>
</html>