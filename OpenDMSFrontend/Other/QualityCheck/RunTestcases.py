from ScriptCollection.TFCPS.NodeJS.TFCPS_CodeUnitSpecific_NodeJS import TFCPS_CodeUnitSpecific_NodeJS_Functions,TFCPS_CodeUnitSpecific_NodeJS_CLI
from ScriptCollection.TFCPS.TFCPS_VisualRegressionTests import TFCPS_VisualRegressionTests


def run_testcases():
    tf:TFCPS_CodeUnitSpecific_NodeJS_Functions=TFCPS_CodeUnitSpecific_NodeJS_CLI.parse(__file__)
    tf.run_testcases()
    vrt=TFCPS_VisualRegressionTests(tf)
    vrt.run(False)
    vrt.check_screenshots_are_similar()



if __name__ == "__main__":
    run_testcases()
