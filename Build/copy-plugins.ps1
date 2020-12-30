param ($solutionDir, $configurationName, $targetName, $targetPath)

$pluginDir = "$solutionDirbin\$configurationName\net5.0\Plugins"

write "Running on $env:OS ..."
write "Copy Plugins to App directory '$pluginDir' ..."

rm -Recurse "$pluginDir"
mkdir "$pluginDir"
Copy $targetPath "$solutionDirbin\$configurationName\net5.0\Plugins\$targetName.dll"
Copy $targetPath "$solutionDirbin\$configurationName\net5.0\Plugins\$targetName.pdb"
Copy $targetPath "$solutionDirbin\$configurationName\net5.0\Plugins\$targetName.deps.json"

write "Copy sucessful."
exit 0