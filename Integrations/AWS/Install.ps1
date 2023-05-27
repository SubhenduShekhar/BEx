cd C:\inetpub\wwwroot\Aura\Api

[Net.ServicePointManager]::SecurityProtocol = [Net.SecurityProtocolType]::Tls12
# Set your required framework version here
$version = "2.0.0"

$header = @{
    "Authorization"= "token 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"
    "Accept"= "application/vnd.github.v3+json"
}
$list_releases_endpoint = "https://api.github.com/repos/MAKnowledgeServices/BATA-Framework/releases"
$parameter = @{
    Method = "GET"
    Uri = $list_releases_endpoint
}

$response = Invoke-RestMethod @parameter -Headers $header

$asset_id = -1

foreach($each_response in $response[0]) {
    if($each_response.name -eq $version) {
        foreach($each_asset in $each_response.assets) {
            if($each_asset.name -eq "BATA.zip") {
                $asset_id = $each_asset.id
                break
            }
        }
    }
}

if($asset_id -eq -1) {
    echo "No BATA version maches to version : " $version
    exit 1
}
echo "BATA v2.0.0 maps to asset id : " + $asset_id

$header = @{
    "Authorization"= "token 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"
    "Accept"= "application/octet-stream"
}
$download_asset_endpoint = "https://api.github.com/repos/MAKnowledgeServices/BATA-Framework/releases/assets/" + $asset_id
$parameter = @{
    Method = "GET"
    Uri = $download_asset_endpoint
}

Invoke-RestMethod @parameter -Headers $header -OutFile bata.zip

Expand-Archive ./bata.zip -DestinationPath ./

./BATA.exe

exit $LastExitCode