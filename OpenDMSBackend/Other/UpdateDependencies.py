import sys
import os
from pathlib import Path
from ScriptCollection.GeneralUtilities import GeneralUtilities
import sys
from ScriptCollection.TFCPS.DotNet.TFCPS_CodeUnitSpecific_DotNet import TFCPS_CodeUnitSpecific_DotNet_Functions,TFCPS_CodeUnitSpecific_DotNet_CLI
 

def update_dependencies():
    tf:TFCPS_CodeUnitSpecific_DotNet_Functions=TFCPS_CodeUnitSpecific_DotNet_CLI.parse(__file__)
    tf.update_dependencies()
    #TODO t.update_dependencies_of_typical_python_repository_requirements(codeunit_folder, 1, sys.argv)


if __name__ == "__main__":
    update_dependencies()
