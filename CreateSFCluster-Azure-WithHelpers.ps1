# Create Self Signed Certificate
Login-AzureRmAccount
Select-AzureRmSubscription -SubscriptionName "Jomit's Internal Subscription"

$certificatePath = "C:\SFHackathon\HOL"
$certificateName = "sflabcertificate1"
$certificatePassword = "pass@Word1"
$certificateDNSName = "jomitsflab"

$keyvaultRGName = "ServiceFabricTrials"
$location = "West US 2"
$keyvaultName = "sflabvalut"

$subscriptionId = "d0c802cd-23ce-4323-a183-5f6d9a84743e"


#Remove-Module ServiceFabricRPHelpers
#git clone https://github.com/ChackDan/Service-Fabric.git
Import-Module C:\github\Service-Fabric\Scripts\ServiceFabricRPHelpers\ServiceFabricRPHelpers.psm1

Invoke-AddCertToKeyVault -SubscriptionId $subscriptionId -ResourceGroupName $keyvaultRGName -Location $location -VaultName $keyvaultName -CertificateName $certificateName -Password $certificatePassword -CreateSelfSignedCertificate -DnsName $certificateDNSName -OutputPath $certificatePath

#Name  : CertificateThumbprint
#Value : E4EA021F3B2CEDE4CE868286329DC32B74BA4D02

#Name  : SourceVault
#Value : /subscriptions/d0c802cd-23ce-4323-a183-5f6d9a84743e/resourceGroups/ServiceFabricTrials/providers/Microsoft.KeyVault/vaults/sflabvalut

#Name  : CertificateURL
#Value : https://sflabvalut.vault.azure.net:443/secrets/sflabcertificate1/46c70164dd93466b85aba707bac610f3

Import-PfxCertificate -Exportable -CertStoreLocation Cert:\CurrentUser\TrustedPeople -FilePath "$certificatePath\$certificateName.pfx" -Password (Read-Host -AsSecureString -Prompt "Enter Certificate Password ")
Import-PfxCertificate -Exportable -CertStoreLocation Cert:\CurrentUser\My -FilePath "$certificatePath\$certificateName.pfx" -Password (Read-Host -AsSecureString -Prompt "Enter Certificate Password ")

