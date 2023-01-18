$URL=$(git remote get-url origin).Replace('.git', '') + "/commit/";
$COMMITS=$(git cherry $($(git describe --tags).Split('-')[0]) HEAD)
foreach ($commit in $COMMITS){ 
    $commit=$commit.Replace('+', '').Trim();
    $message=(iex (echo "git show -s --format=%B $($commit)")); 
    $message="[$(iex "echo $($message.Replace("'", "''''"))")](" + $URL + $commit + ")";
    echo $message;
}