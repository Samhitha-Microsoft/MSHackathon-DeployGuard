param (
    [string]$connectionString,
    [string]$SelectedRegion,
    [int]$LockDuration
)
 
# Database connection string
$connectionString = "$(CONNECTION_STRING)"
# Check if the region is locked
$query = "SELECT lock_status, end_time FROM LockTable WHERE region='$SelectedRegion'"
$regionStatus = Invoke-Sqlcmd -ConnectionString $connectionString -Query $query
 
if ($regionStatus.lock_status -eq $true) {
    $endTime = $regionStatus.end_time
    Write-Error "Region $SelectedRegion is locked until $endTime. Please select a different region."
    exit 1
}
 
# Lock the selected region by updating the lock status and end time
$lockEndTime = (Get-Date).AddMinutes($LockDuration).ToString("yyyy-MM-dd HH:mm:ss")
$updateQuery = "UPDATE LockTable SET lock_status=1, end_time='$lockEndTime' WHERE region='$SelectedRegion'"
Invoke-Sqlcmd -ConnectionString $connectionString -Query $updateQuery
 
Write-Host "Region $SelectedRegion locked until $lockEndTime"