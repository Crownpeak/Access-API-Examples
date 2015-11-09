###################################################################################################
#
# Summary: AccessAPI service library
#
###################################################################################################
$Global:CP_AAPI_server = 'https://wcd.crownpeak.com'
$Global:CP_AAPI_instance = 'DevSandbox'
$Global:CP_AAPI_cc = $null
$Global:CP_AAPI_accessKey = ''
$Global:CP_AAPI_skey = ''
$Global:CP_AAPI_username = ''

#Set-Variable conFileType -Value 2 -option Constant
#Set-Variable conFolderType -Value 4 -option Constant
$conFolderType = 4
$conFileType = 2
$recursivePostCount = 0
$debug = $false

#builds the AccessAPI header
function Get-CMS_Signature([string]$absolutePath, [string]$body, [string]$method='POST') {    
    $headers = @{ 'x-api-key' = $($Global:CP_AAPI_accessKey)}    
    return $headers
}
#builds the AccessAPI header with the secret key (for server side calls only, do not use the secret key on the client)
function Get-CMS_SHA1_Signature([string]$absolutePath, [string]$body, [string]$method='POST') {    
    $datetime = (Get-Date).ToUniversalTime().ToString( "yyyy-MM-ddTHH:mm:ss.fffffffZ" )
    $data = "$method`n$absolutePath`n$body`n$datetime"

    $hmacsha1 = new-object System.Security.Cryptography.HMACSHA1;
    $hmacsha1.Key = [System.Text.Encoding]::UTF8.GetBytes($Global:CP_AAPI_skey);
    
    $signature = [System.Convert]::ToBase64String($hmacsha1.ComputeHash([System.Text.Encoding]::UTF8.GetBytes($data)));    
    $headers = @{ 'Authorization' = "CP $($Global:CP_AAPI_accessKey):$signature"; 'cp-datetime'="$datetime"; 'Accept-Encoding'= 'gzip, deflate'}    
    return $headers
}

#make http call to the CMS, handle HTTP 429 - rate limit reached, retry-after
function Invoke-CMS_POST([string]$path, [string]$body) {
    if ($recursivePostCount++ -gt 10) {
        Exit -1  #recursive limit reached
    }

    $httpVerb = 'POST'
    $absolutePath= "/$Global:CP_AAPI_instance/cpt_webservice/accessapi$path"
    $headers = Get-CMS_Signature $absolutePath $body $httpVerb
    $uri = "$Global:CP_AAPI_server$absolutePath"
    #auth must be called first, since it will setup the cookie, otherwise this call will fail due to 'access denied'
    #$resp = Invoke-RestMethod $uri -Method $httpVerb -WebSession $Global:CP_AAPI_cc -Headers $headers -Body $body -ContentType 'application/json' -ErrorVariable errorVar -ErrorAction SilentlyContinue
    $webresp = Invoke-WebRequest $uri -Method $httpVerb -WebSession $Global:CP_AAPI_cc -Headers $headers -Body $body -ContentType 'application/json' -ErrorVariable errorVar -ErrorAction SilentlyContinue    
    if($webresp -eq $null)
    {
        $webException = $($errorVar[0].GetBaseException() -as [System.Net.WebException]) 
        if($webException -ne $null -and $($webException.Response.StatusCode -as [int]) -eq 429)#no System.Net.HttpStatusCode enumeration for 429
        {
            $sleepTime = 1 #fallback value, if Retry-After header is not there        
            $retryHeader = $webException.Response.Headers["Retry-After"]                     
            if($retryHeader -ne $null -and $($retryHeader -as [int]) -ne $null)
            {
                $sleepTime = $retryHeader -as [int]
            }
            if ($debug) { write-host "429 received, retrying in $sleepTime seconds... ($recursivePostCount) " -ForegroundColor Cyan }
            Start-Sleep -s $sleepTime
            return Invoke-CMS_POST $path $body     
        }
        else
        {
            exit 1
        }
    }
    else
    {
        #success
        if ($debug -and $webresp.Headers.ContainsKey('cp-timetaken')) {
            write-host 'timetaken: ' $webresp.Headers.Item('cp-timetaken')
        }
        $resp = $webresp.Content | ConvertFrom-Json
        $recursivePostCount = 0
    }
    return $resp    
}

#helper function to make authentication call
function Invoke-CMS_Auth([string]$body) {
    $absolutePath= "/$Global:CP_AAPI_instance/cpt_webservice/accessapi/auth/authenticate"    
    $headers = Get-CMS_Signature $absolutePath $body 'POST'
    $uri = "$Global:CP_AAPI_server$absolutePath"
    $resp = Invoke-RestMethod $uri -Method 'POST' -SessionVariable Global:CP_AAPI_cc -Headers $headers -Body $body -ContentType 'application/json' -ErrorVariable err -ErrorAction SilentlyContinue
    if ($err -ne $null)
    {
        $err
    }
    return $resp
}

#main authentication call
function Invoke-CMS_Authenticate($cred) {
    $timeZoneOffset = ([TimeZoneInfo]::Local).BaseUtcOffset.TotalMinutes
    $body = "{`"instance`":`"$Global:CP_AAPI_instance`",`"username`":`"$($cred.UserName)`",`"password`":`"$($cred.GetNetworkCredential().Password)`",`"remember_me`":false,`"timeZoneOffsetMinutes`":$timeZoneOffset}"
    return Invoke-CMS_Auth $body
}
#logout call
function Invoke-CMS_Logout() {
    $body = ""
    return  Invoke-CMS_Post '/auth/logout' $body
}
#check if an asset exists in the CMS
function Invoke-CMS_AssetExists($assetPath) {
    $body = "{`"assetIdOrPath`":`"$assetPath`"}"
    return  Invoke-CMS_Post '/Asset/Exists' $body
}
#create an asset at the provided folderId
function Invoke-CMS_CreateAsset([string]$newName, [int]$folderId, [int]$modelId, [int]$type, [int]$devTemplateLanguage = -1, [int]$templateId = -1, [int]$workflowId = -1) {
    $req = @{}
    $req.Add('newName', "`"$newName`"")
    $req.Add('destinationFolderId', $folderId)
    $req.Add('modelId', $modelId)
    $req.Add('type', $type)
    $req.Add('devTemplateLanguage', $devTemplateLanguage)    
    $req.Add('templateId', $templateId)
    $req.Add('workflowId', $workflowId)    
    $body = $req | ConvertTo-Json
    
    return Invoke-CMS_Post '/Asset/Create' $body
}
#upload a binary asset
function Invoke-CMS_UploadAsset([string]$newName, [int]$folderId, [int]$modelId, [int]$workflowId, [string]$Base64EncodedBytes) {
    $req = @{}
    $req.Add('newName', "`"$newName`"")
    $req.Add('destinationFolderId', $folderId)
    $req.Add('modelId', $modelId)
    $req.Add('workflowId', $workflowId)    
    $req.Add('bytes', $Base64EncodedBytes)   
    $body = $req | ConvertTo-Json
    return Invoke-CMS_Post '/Asset/Upload' $body
}
#get cms assets
function Invoke-CMS_GetPagedAssets( [int]$assetId,
                                    [int]$page = 0, 
                                    [int]$pageSize=10, 
                                    [string]$sortColumn='label', 
                                    [string]$orderType = 'Ascending', 
                                    [string]$visibilityType = 'Normal') {
    $body = "{`"assetId`":$assetId,`"currentPage`":$page,`"pageSize`":$pageSize,`"sortColumn`":`"$sortColumn`",`"orderType`":`"$orderType`", `"visibilityType`":`"$visibilityType`", `"ignoreFilter`":true, `"ignoreSort`":true}"
    return Invoke-CMS_Post '/Asset/Paged' $body
}
#set configuration items for use by the script
function Set-CMS_Config($configFileName) {
    #todo: add error handling
    $config = gc config.json  | ConvertFrom-Json

    $Global:CP_AAPI_server = $config.server
    $Global:CP_AAPI_instance = $config.instance
    $Global:CP_AAPI_accessKey = $config.accessKey
    $Global:CP_AAPI_username = $config.username
}
#alternative method to read password for use in the script
function Get-CMS_Credentials {
    if ($global:CP_AAPI_cred -eq $null)
    {
        $global:CP_AAPI_cred = Get-Credential -Message 'Please enter CMS credentials' -UserName $Global:CP_AAPI_username
    }  
    return $global:CP_AAPI_cred
}