import sys
from ScriptCollection.TFCPS_CodeUnitSpecific_NodeJS import TFCPS_CodeUnitSpecific_NodeJS_Functions,TFCPS_CodeUnitSpecific_NodeJS_CLI
 
def build():

    tf:TFCPS_CodeUnitSpecific_NodeJS_Functions=TFCPS_CodeUnitSpecific_NodeJS_CLI.parse(__file__)
    tf.linting() 


if __name__ == "__main__":
    build()
