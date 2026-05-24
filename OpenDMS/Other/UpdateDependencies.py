from ScriptCollection.TFCPS.Docker.TFCPS_CodeUnitSpecific_Docker import TFCPS_CodeUnitSpecific_Docker_Functions,TFCPS_CodeUnitSpecific_Docker_CLI
from ScriptCollection.ImageUpdater import ConcreteImageUpdaterForGeneric

def update_dependencies():
    tf:TFCPS_CodeUnitSpecific_Docker_Functions=TFCPS_CodeUnitSpecific_Docker_CLI.parse(__file__)
    tf.tfcps_Tools_General.update_images_in_example(tf.get_codeunit_folder(),["opendms"],[ConcreteImageUpdaterForGeneric("simpleocr")])
    
if __name__ == "__main__":
    update_dependencies()
 