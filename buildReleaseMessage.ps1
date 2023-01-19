$LAST_TAG=$(git tag|where{$_ -Match "^\d+.\d+.\d+$"})[-1]
$URL=$(git remote get-url origin).Replace('.git', '') + "/commit/";
$COMMITS=$(git cherry $LAST_TAG HEAD)
foreach ($commit in $COMMITS){ 
    $commit=$commit.Replace('+', '').Trim();
    $message=(iex (echo "git show -s --format=%B $($commit)")); 
    $message="[$(iex "echo $($message.Replace("'", "''''"))")](" + $URL + $commit + ")";
    echo $message;
}