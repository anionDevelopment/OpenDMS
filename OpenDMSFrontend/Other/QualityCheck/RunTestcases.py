from ScriptCollection.TFCPS.NodeJS.TFCPS_CodeUnitSpecific_NodeJS import TFCPS_CodeUnitSpecific_NodeJS_Functions,TFCPS_CodeUnitSpecific_NodeJS_CLI


def run_testcases():
    tf:TFCPS_CodeUnitSpecific_NodeJS_Functions=TFCPS_CodeUnitSpecific_NodeJS_CLI.parse(__file__)    
    tf.run_testcases()



if __name__ == "__main__":
    run_testcases()
