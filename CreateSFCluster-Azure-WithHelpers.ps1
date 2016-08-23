# Create Self Signed Certificate
Login-AzureRmAccount
Select-AzureRmSubscription -SubscriptionName "Jomit's Internal Subscription"

$certificatePath = "C:\SFHackathon"
$certificateName = "sflabcertificate1"
$certificatePassword = "pass@word1"
$certificateDNSName = "jomitsflab"

$keyvaultRGName = "AllVaults"
$location = "West US"
$keyvaultName = "sflabvault"

$subscriptionId = "d0c802cd-23ce-4323-a183-5f6d9a84743e"


#Remove-Module ServiceFabricRPHelpers
#git clone https://github.com/ChackDan/Service-Fabric.git
Import-Module C:\github\Service-Fabric\Scripts\ServiceFabricRPHelpers\ServiceFabricRPHelpers.psm1

Invoke-AddCertToKeyVault -SubscriptionId $subscriptionId -ResourceGroupName $keyvaultRGName -Location $location -VaultName $keyvaultName -CertificateName $certificateName -Password $certificatePassword -CreateSelfSignedCertificate -DnsName $certificateDNSName -OutputPath $certificatePath

#Name  : CertificateThumbprint
#Value : 500673EC2E38EBD521A1BB4C1BE90925235C625B

#Name  : SourceVault
#Value : /subscriptions/d0c802cd-23ce-4323-a183-5f6d9a84743e/resourceGroups/AllVaults/providers/Microsoft.KeyVault/vaults/sflabvault

#Name  : CertificateURL
#Value : https://sflabvault.vault.azure.net:443/secrets/sflabcertificate1/e6a94400297f45bbb179fe449e1562fd

Import-PfxCertificate -Exportable -CertStoreLocation Cert:\CurrentUser\TrustedPeople -FilePath "$certificatePath\$certificateName.pfx" -Password (Read-Host -AsSecureString -Prompt "Enter Certificate Password ")
Import-PfxCertificate -Exportable -CertStoreLocation Cert:\CurrentUser\My -FilePath "$certificatePath\$certificateName.pfx" -Password (Read-Host -AsSecureString -Prompt "Enter Certificate Password ")

