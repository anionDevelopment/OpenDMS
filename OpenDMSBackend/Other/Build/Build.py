from ScriptCollection.GeneralUtilities import GeneralUtilities, Platform
from ScriptCollection.TFCPS.DotNet.TFCPS_CodeUnitSpecific_DotNet import TFCPS_CodeUnitSpecific_DotNet_Functions,TFCPS_CodeUnitSpecific_DotNet_CLI
 
def build():
    platforms:list[Platform] = [
            Platform.Windows_AMD64,
            Platform.Linux_AMD64,
            Platform.Linux_ARM64,
    ]
    tf:TFCPS_CodeUnitSpecific_DotNet_Functions=TFCPS_CodeUnitSpecific_DotNet_CLI.parse(__file__)
    tf.build([GeneralUtilities.platform_to_dotnet_runtime_identifier(p)  for p in platforms], True)
    for platform in platforms:
        pass# TODO copy codeunit-resource "TypeScript" to output-directory
    # TODO add typescript to sbom


if __name__ == "__main__":
    build()
