param (
    [string]$type
 )

echo $type

$LAST_TAG=$(git tag|where{$_ -Match "^\d+.\d+.\d+$"})[-1]
$URL=$(git remote get-url origin).Replace('.git', '') + "/commit/";
$COMMITS=$(git cherry $LAST_TAG HEAD)
foreach ($commit in $COMMITS){ 
    $commit=$commit.Replace('+', '').Trim();
    $message=$(iex $(echo "git show -s --format=%B $($commit)"))[0];
    $message=$message.Replace("'", "''''");
    if ($type -ne "simple") {
        $message="[$message]($URL$commit)";
    }
    echo $message;    
}